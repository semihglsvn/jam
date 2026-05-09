using Godot;
using System;
using System.Collections.Generic;

public partial class Blackjack : Control
{
    // --- Helper Class for the Real Cards ---
    public class PlayingCard
    {
        public int ScoreValue;
        public string ImageName; 
    }

    // --- UI Hooks (Drag these in from the Inspector) ---
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
    [Export] public Label WalletLabel; 

    [ExportCategory("Buttons")]
    [Export] public Button HitBtn;
    [Export] public Button StandBtn;
    [Export] public Button DoubleBtn;
    [Export] public Button SplitBtn;

    // --- Game State ---
    private List<PlayingCard> _deck = new List<PlayingCard>();
    private List<PlayingCard> _dealerHand = new List<PlayingCard>();
    private List<PlayingCard> _hand1 = new List<PlayingCard>();
    private List<PlayingCard> _hand2 = new List<PlayingCard>();

    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private bool _isSplit = false;
    private int _activeHand = 1;
    private bool _waitingForNextRound = false; // Spacebar state
    
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

        _rng.Randomize();
        StartNewRound();
    }

    public override void _Input(InputEvent @event)
    {
        // Spacebar to deal next round
        if (_waitingForNextRound && @event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
        {
            _waitingForNextRound = false; 
            StartNewRound();
        }
    }

    private async void StartNewRound()
    {
        if (_playerWallet < _currentBet)
        {
            ResultLabel.Text = "OUT OF COINS!\nGAME OVER";
            if (WalletLabel != null) WalletLabel.Text = $"WALLET: ${_playerWallet}";
            HitBtn.Disabled = true; StandBtn.Disabled = true; DoubleBtn.Disabled = true; SplitBtn.Disabled = true;
            return;
        }

		_isSplit = false;
        _activeHand = 1;
        _isDealerRevealed = false; 
        
        Hand2Area.Visible = false;
        ResultLabel.Text = "";
        
// ... (Keep the top part of your StartNewRound exactly the same!) ...

        // 1. Clear the old numbers instantly
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

        // 2. Deal the card, THEN instantly update the score, THEN wait!
        DealCard(_hand1, Hand1UI);
        UpdateScoresUI(); // <--- ADD THIS
        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
        
        DealCard(_dealerHand, DealerHandUI); 
        UpdateScoresUI(); // <--- ADD THIS
        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
        
        DealCard(_hand1, Hand1UI);
        UpdateScoresUI(); // <--- ADD THIS
        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
        
        DealHiddenCard(); 
        UpdateScoresUI(); // <--- ADD THIS

        // UpdateScoresUI() is no longer needed here since we updated as we dealt!
        CheckActionButtons();
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
                int val = 0;
                
                if (i == 0) val = 11; 
                else if (i >= 1 && i <= 9) val = i + 1; 
                else val = 10; 

                _deck.Add(new PlayingCard 
                { 
                    ScoreValue = val, 
                    ImageName = $"{cardId.ToString("D2")}_kerenel_Cards.png" 
                });
            }
        }
    }

    private void DealCard(List<PlayingCard> hand, HBoxContainer uiContainer)
    {
        if (_deck.Count == 0) BuildDeck();

        int index = _rng.RandiRange(0, _deck.Count - 1);
        PlayingCard drawnCard = _deck[index];
        _deck.RemoveAt(index);
        hand.Add(drawnCard);

        TextureRect cardVisual = new TextureRect
        {
            CustomMinimumSize = new Vector2(80, 120),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };

string path = $"res://Cards/{drawnCard.ImageName}"; 
        if (ResourceLoader.Exists(path))
        {
            cardVisual.Texture = GD.Load<Texture2D>(path);
        }

        // --- ADD THESE 4 LINES HERE ---
        
        // 1. Load the material and duplicate it (so each card glitches independently)
        ShaderMaterial glitchMat = GD.Load<ShaderMaterial>("res://Cards/GlitchMaterial.tres").Duplicate() as ShaderMaterial;
        cardVisual.Material = glitchMat;
        
        // 2. Create a Godot Tween to animate the threshold over 0.5 seconds
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(glitchMat, "shader_parameter/threshold", 1.0f, 0.5f);

        // ------------------------------
        uiContainer.AddChild(cardVisual);
    }

    private void DealHiddenCard()
    {
        if (_deck.Count == 0) BuildDeck();

        int index = _rng.RandiRange(0, _deck.Count - 1);
        PlayingCard drawnCard = _deck[index];
        _deck.RemoveAt(index);
        _dealerHand.Add(drawnCard); 

        _hiddenCardVisual = new TextureRect
        {
            CustomMinimumSize = new Vector2(80, 120),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };

        string path = "res://Cards/28_kerenel_Cards.png"; 
        if (ResourceLoader.Exists(path))
        {
            _hiddenCardVisual.Texture = GD.Load<Texture2D>(path);
        }

        DealerHandUI.AddChild(_hiddenCardVisual);
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
        if (_isDealerRevealed)
        {
            DealerScoreLabel.Text = $"Dealer: {CalculateScore(_dealerHand)}";
        }
        else if (_dealerHand.Count > 0)
        {
            DealerScoreLabel.Text = $"Dealer: {_dealerHand[0].ScoreValue}";
        }

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

    // --- BUTTON ACTIONS ---

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
        else
        {
            ResolveDealerTurn(); 
        }
    }

    private void OnDoubleDownPressed()
    {
        _playerWallet -= _currentBet; 
        if (WalletLabel != null) WalletLabel.Text = $"WALLET: ${_playerWallet}";
        
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
        if (WalletLabel != null) WalletLabel.Text = $"WALLET: ${_playerWallet}";
        
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

        // Reveal the hidden card
        _isDealerRevealed = true;
        if (_hiddenCardVisual != null && _dealerHand.Count >= 2)
        {
            string realPath = $"res://Cards/{_dealerHand[1].ImageName}";
            _hiddenCardVisual.Texture = GD.Load<Texture2D>(realPath);
        }
        
        UpdateScoresUI();
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        // CHECK IF PLAYER BUSTED:
        bool playerHasViableHand = CalculateScore(_hand1) <= 21;
        if (_isSplit && CalculateScore(_hand2) <= 21) 
        {
            playerHasViableHand = true;
        }

        // Only draw cards if the player has a hand that can still win
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

        // Hand 1 Evaluation
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

        // Hand 2 Evaluation (If Split)
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
        if (WalletLabel != null) WalletLabel.Text = $"WALLET: ${_playerWallet}";

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