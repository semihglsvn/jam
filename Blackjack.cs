using Godot;
using System;
using System.Collections.Generic;

public partial class Blackjack : Control
{
	public class PlayingCard
	{
		public int ScoreValue;
		public string ImageName; 
	}

	// --- UI Hooks ---
	[ExportCategory("Containers")]
	[Export] public HBoxContainer DealerHandUI;
	[Export] public HBoxContainer Hand1UI;
	[Export] public HBoxContainer Hand2UI;
	[Export] public Control Hand2Area; 

	[ExportCategory("Labels")]
	[Export] public Label DealerScoreLabel;
	[Export] public Label Hand1ScoreLabel;
	[Export] public Label Hand2ScoreLabel;
	[Export] public Label ResultLabel;

	[ExportCategory("Buttons")]
	[Export] public Button HitBtn;
	[Export] public Button StandBtn;
	[Export] public Button DoubleBtn;
	[Export] public Button SplitBtn;
	[Export] public Button GoToSlotsBtn; // <--- NEW: The transition button!

	// --- NEW: Economy Visuals ---
	[ExportCategory("Economy Visuals")]
	[Export] public Control ChipStackAnchor; 
	[Export] public Texture2D ChipTexture; 

	// --- Game State ---
	private List<PlayingCard> _deck = new List<PlayingCard>();
	private List<PlayingCard> _dealerHand = new List<PlayingCard>();
	private List<PlayingCard> _hand1 = new List<PlayingCard>();
	private List<PlayingCard> _hand2 = new List<PlayingCard>();

	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private bool _isSplit = false;
	private int _activeHand = 1;
	private bool _waitingForNextRound = false; 
	
	private TextureRect _hiddenCardVisual;
	private bool _isDealerRevealed = false;

	// --- Economy ---
	private int _playerWallet = 1000; 
	private int _currentBet = 100;
	private int _hand1Bet = 0;
	private int _hand2Bet = 0;

	public override void _Ready()
	{
		HitBtn.Pressed += OnHitPressed;
		StandBtn.Pressed += OnStandPressed;
		DoubleBtn.Pressed += OnDoubleDownPressed;
		SplitBtn.Pressed += OnSplitPressed;
		if (GoToSlotsBtn != null) GoToSlotsBtn.Pressed += OnGoToSlotsPressed;

		// <--- NEW: Force the CanvasLayer to turn invisible when Main.cs tells the game to hide! --->
		this.VisibilityChanged += () =>
		{
			CanvasLayer myCanvas = GetNodeOrNull<CanvasLayer>("CanvasLayer");
			if (myCanvas != null) myCanvas.Visible = this.Visible;
		};
		// <---------------------------------------------------------------------------------------->

		_rng.Randomize();
		UpdateVisualChips(); 
		StartNewRound();
	}

	public override void _Input(InputEvent @event)
	{
		if (_waitingForNextRound && @event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
		{
			_waitingForNextRound = false; 
			StartNewRound();
		}
	}

	// <--- NEW: The Transition Method
	private void OnGoToSlotsPressed()
	{
		// Use the absolute path to find Main
		Main mainScene = GetNodeOrNull<Main>("/root/Main"); 
		
		if (mainScene != null)
		{
			mainScene.SwitchGame("Slots", "LADY LUCK IS WAITING...");
		}
		else
		{
			GD.PrintErr("CRITICAL: Could not find the Main node at /root/Main!");
		}
	}

	private async void StartNewRound()
	{
		if (_playerWallet < _currentBet)
		{
			ResultLabel.Text = "OUT OF COINS!\nGAME OVER";
			UpdateVisualChips();
			HitBtn.Disabled = true; StandBtn.Disabled = true; DoubleBtn.Disabled = true; SplitBtn.Disabled = true;
			return;
		}

		// Deduct bet instantly
		_playerWallet -= _currentBet;
		_hand1Bet = _currentBet;
		_hand2Bet = 0;

		_isSplit = false;
		_activeHand = 1;
		_isDealerRevealed = false; 
		
		Hand2Area.Visible = false;
		ResultLabel.Text = "";
		DealerScoreLabel.Text = "";
		Hand1ScoreLabel.Text = "";
		Hand2ScoreLabel.Text = "";
		
		ClearUI(DealerHandUI);
		ClearUI(Hand1UI);
		ClearUI(Hand2UI);
		
		_dealerHand.Clear();
		_hand1.Clear();
		_hand2.Clear();

		BuildDeck();

		DealCard(_hand1, Hand1UI);
		UpdateScoresUI();
		await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
		
		DealCard(_dealerHand, DealerHandUI); 
		UpdateScoresUI();
		await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
		
		DealCard(_hand1, Hand1UI);
		UpdateScoresUI();
		await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
		
		DealHiddenCard(); 
		UpdateScoresUI();

		CheckActionButtons();
	}

	// --- Physical Chip Spawner ---
	private void UpdateVisualChips()
	{
		if (ChipStackAnchor == null || ChipTexture == null) return;

		int targetChips = _playerWallet / 100;
		int currentChips = ChipStackAnchor.GetChildCount();

		if (currentChips == targetChips) return; 

		if (targetChips > currentChips)
		{
			int chipsToAdd = targetChips - currentChips;
			for (int i = 0; i < chipsToAdd; i++)
			{
				int stackIndex = currentChips + i; 

				TextureRect chip = new TextureRect
				{
					Texture = ChipTexture,
					ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
					CustomMinimumSize = new Vector2(48, 48), 
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
					Position = new Vector2(0, -600) 
				};

				ChipStackAnchor.AddChild(chip);

				float targetY = -(stackIndex * 6); 
				
				Tween tween = GetTree().CreateTween();
				tween.TweenProperty(chip, "position", new Vector2(0, targetY), 0.4f)
					 .SetDelay(i * 0.05f) 
					 .SetTrans(Tween.TransitionType.Bounce)
					 .SetEase(Tween.EaseType.Out);
			}
		}
		else if (targetChips < currentChips)
		{
			int chipsToRemove = currentChips - targetChips;
			for (int i = 0; i < chipsToRemove; i++)
			{
				int lastChildIndex = ChipStackAnchor.GetChildCount() - 1;
				if (lastChildIndex >= 0)
				{
					Node topChip = ChipStackAnchor.GetChild(lastChildIndex);
					ChipStackAnchor.RemoveChild(topChip);
					topChip.QueueFree();
				}
			}
		}
	}

	private void BuildDeck()
	{
		_deck.Clear();
		int[] suitStartIndices = { 1, 15, 29, 43 }; 

		foreach (int startIdx in suitStartIndices)
		{
			for (int i = 0; i < 13; i++) 
			{
				int cardId = startIdx + i;
				int val = (i == 0) ? 11 : (i >= 1 && i <= 9) ? i + 1 : 10; 

				_deck.Add(new PlayingCard 
				{ 
					ScoreValue = val, 
					ImageName = $"{cardId.ToString("D2")}_kerenel_Cards.png" 
				});
			}
		}
	}

	// --- Sliding & Glitching Card ---
	private void DealCard(List<PlayingCard> hand, HBoxContainer uiContainer)
	{
		if (_deck.Count == 0) BuildDeck();

		int index = _rng.RandiRange(0, _deck.Count - 1);
		PlayingCard drawnCard = _deck[index];
		_deck.RemoveAt(index);
		hand.Add(drawnCard);

		Control placeholder = new Control { CustomMinimumSize = new Vector2(80, 120) };

		TextureRect cardVisual = new TextureRect
		{
			CustomMinimumSize = new Vector2(80, 120),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Position = new Vector2(0, -600)
		};

		string path = $"res://Cards/{drawnCard.ImageName}"; 
		if (ResourceLoader.Exists(path)) cardVisual.Texture = GD.Load<Texture2D>(path);

		ShaderMaterial glitchMat = GD.Load<ShaderMaterial>("res://Cards/GlitchMaterial.tres").Duplicate() as ShaderMaterial;
		cardVisual.Material = glitchMat;

		uiContainer.AddChild(placeholder);
		placeholder.AddChild(cardVisual);

		Tween tween = GetTree().CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(cardVisual, "position", Vector2.Zero, 0.4f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(glitchMat, "shader_parameter/threshold", 1.0f, 0.6f);
	}

	// --- Sliding & Glitching Hidden Card ---
	private void DealHiddenCard()
	{
		if (_deck.Count == 0) BuildDeck();

		int index = _rng.RandiRange(0, _deck.Count - 1);
		PlayingCard drawnCard = _deck[index];
		_deck.RemoveAt(index);
		_dealerHand.Add(drawnCard); 

		Control placeholder = new Control { CustomMinimumSize = new Vector2(80, 120) };

		_hiddenCardVisual = new TextureRect
		{
			CustomMinimumSize = new Vector2(80, 120),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Position = new Vector2(0, -600)
		};

		string path = "res://Cards/28_kerenel_Cards.png"; 
		if (ResourceLoader.Exists(path)) _hiddenCardVisual.Texture = GD.Load<Texture2D>(path);

		ShaderMaterial glitchMat = GD.Load<ShaderMaterial>("res://Cards/GlitchMaterial.tres").Duplicate() as ShaderMaterial;
		_hiddenCardVisual.Material = glitchMat;

		DealerHandUI.AddChild(placeholder);
		placeholder.AddChild(_hiddenCardVisual);

		Tween tween = GetTree().CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_hiddenCardVisual, "position", Vector2.Zero, 0.4f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(glitchMat, "shader_parameter/threshold", 1.0f, 0.6f);
	}

	private int CalculateScore(List<PlayingCard> hand)
	{
		int sum = 0;
		int aces = 0;

		foreach (PlayingCard card in hand)
		{
			sum += card.ScoreValue;
			if (card.ScoreValue == 11) aces++;
		}

		while (sum > 21 && aces > 0)
		{
			sum -= 10;
			aces--;
		}

		return sum;
	}

	private void UpdateScoresUI()
	{
		if (_isDealerRevealed) DealerScoreLabel.Text = $"Dealer: {CalculateScore(_dealerHand)}";
		else if (_dealerHand.Count > 0) DealerScoreLabel.Text = $"Dealer: {_dealerHand[0].ScoreValue}";

		Hand1ScoreLabel.Text = $"Hand 1: {CalculateScore(_hand1)}";
		if (_isSplit) Hand2ScoreLabel.Text = $"Hand 2: {CalculateScore(_hand2)}";
	}

	private void CheckActionButtons()
	{
		List<PlayingCard> currentHand = _activeHand == 1 ? _hand1 : _hand2;
		int score = CalculateScore(currentHand);

		SplitBtn.Disabled = _isSplit || _hand1.Count != 2 || _hand1[0].ScoreValue != _hand1[1].ScoreValue || _playerWallet < _currentBet;
		DoubleBtn.Disabled = currentHand.Count != 2 || _playerWallet < _currentBet;

		HitBtn.Disabled = false;
		StandBtn.Disabled = false;

		if (score >= 21) OnStandPressed(); 
	}

	private void OnHitPressed()
	{
		if (_activeHand == 1) DealCard(_hand1, Hand1UI);
		else DealCard(_hand2, Hand2UI);

		UpdateScoresUI();
		CheckActionButtons();
	}

	private void OnStandPressed()
	{
		if (_isSplit && _activeHand == 1)
		{
			_activeHand = 2; 
			CheckActionButtons();
		}
		else ResolveDealerTurn(); 
	}

	private void OnDoubleDownPressed()
	{
		_playerWallet -= _currentBet; 
		
		if (_activeHand == 1)
		{
			_hand1Bet *= 2;
			DealCard(_hand1, Hand1UI);
		}
		else
		{
			_hand2Bet *= 2;
			DealCard(_hand2, Hand2UI);
		}

		UpdateScoresUI();
		OnStandPressed(); 
	}

	private void OnSplitPressed()
	{
		_playerWallet -= _currentBet;
		
		_hand2Bet = _currentBet;
		_isSplit = true;
		Hand2Area.Visible = true;

		PlayingCard splitCard = _hand1[1];
		_hand1.RemoveAt(1);
		_hand2.Add(splitCard);

		Node cardToMove = Hand1UI.GetChild(1);
		Hand1UI.RemoveChild(cardToMove);
		Hand2UI.AddChild(cardToMove);

		DealCard(_hand1, Hand1UI);
		DealCard(_hand2, Hand2UI);

		UpdateScoresUI();
		CheckActionButtons();
	}

	private async void ResolveDealerTurn()
	{
		HitBtn.Disabled = true; StandBtn.Disabled = true; DoubleBtn.Disabled = true; SplitBtn.Disabled = true;

		_isDealerRevealed = true;
		if (_hiddenCardVisual != null && _dealerHand.Count >= 2)
		{
			string realPath = $"res://Cards/{_dealerHand[1].ImageName}";
			_hiddenCardVisual.Texture = GD.Load<Texture2D>(realPath);
		}
		
		UpdateScoresUI();
		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

		bool playerHasViableHand = CalculateScore(_hand1) <= 21;
		if (_isSplit && CalculateScore(_hand2) <= 21) playerHasViableHand = true;

		if (playerHasViableHand)
		{
			while (CalculateScore(_dealerHand) < 17)
			{
				DealCard(_dealerHand, DealerHandUI);
				UpdateScoresUI();
				await ToSignal(GetTree().CreateTimer(0.8f), SceneTreeTimer.SignalName.Timeout);
			}
		}

		EvaluateWinners();
	}

	private void EvaluateWinners()
	{
		int dealerScore = CalculateScore(_dealerHand);
		int h1Score = CalculateScore(_hand1);
		
		string resultText = "";

		if (h1Score > 21) resultText += "Hand 1: BUST. ";
		else if (dealerScore > 21 || h1Score > dealerScore) 
		{
			resultText += "Hand 1: WIN! ";
			_playerWallet += _hand1Bet * 2; 
		}
		else if (h1Score == dealerScore) 
		{
			resultText += "Hand 1: PUSH. ";
			_playerWallet += _hand1Bet; 
		}
		else resultText += "Hand 1: LOSE. ";

		if (_isSplit)
		{
			int h2Score = CalculateScore(_hand2);
			if (h2Score > 21) resultText += "\nHand 2: BUST.";
			else if (dealerScore > 21 || h2Score > dealerScore) 
			{
				resultText += "\nHand 2: WIN!";
				_playerWallet += _hand2Bet * 2;
			}
			else if (h2Score == dealerScore) 
			{
				resultText += "\nHand 2: PUSH.";
				_playerWallet += _hand2Bet;
			}
			else resultText += "\nHand 2: LOSE.";
		}

		ResultLabel.Text = resultText + "\n\n[ PRESS SPACE TO DEAL ]";
		
		UpdateVisualChips(); 

		_waitingForNextRound = true; 
	}

	private void ClearUI(Node container)
	{
		foreach (Node child in container.GetChildren())
		{
			child.QueueFree();
		}
	}
}
