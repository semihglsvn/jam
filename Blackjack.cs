using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

    [ExportCategory("Buttons")]
    [Export] public Button HitBtn;
    [Export] public Button StandBtn;
    [Export] public Button DoubleBtn;
    [Export] public Button SplitBtn;

    // --- Game State (Now using Objects instead of ints) ---
    private List<PlayingCard> _deck = new List<PlayingCard>();
    private List<PlayingCard> _dealerHand = new List<PlayingCard>();
    private List<PlayingCard> _hand1 = new List<PlayingCard>();
    private List<PlayingCard> _hand2 = new List<PlayingCard>();

    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private bool _isSplit = false;
    private int _activeHand = 1;
    
    // Economy
    private int _playerWallet = 1000; 
    private int _currentBet = 100;
    private int _hand1Bet = 0;
    private int _hand2Bet = 0;

    public override void _Ready()
    {
        _rng.Randomize();
        StartNewRound();
    }

    private void StartNewRound()
    {
        _isSplit = false;
        _activeHand = 1;
        _hand1Bet = _currentBet;
        _hand2Bet = 0;
        
        Hand2Area.Visible = false;
        ResultLabel.Text = "";
        
        ClearUI(DealerHandUI);
        ClearUI(Hand1UI);
        ClearUI(Hand2UI);
        
        _dealerHand.Clear();
        _hand1.Clear();
        _hand2.Clear();

        BuildDeck();

        DealCard(_hand1, Hand1UI);
        DealCard(_dealerHand, DealerHandUI); 
        DealCard(_hand1, Hand1UI);

        UpdateScoresUI();
        CheckActionButtons();
    }

    private void BuildDeck()
    {
        _deck.Clear();
        
        // The starting numbers for Hearts, Spades, Diamonds, Clubs based on your sprite sheet
        int[] suitStartIndices = { 1, 15, 29, 43 }; 

        foreach (int startIdx in suitStartIndices)
        {
            for (int i = 0; i < 13; i++) // 13 cards per suit
            {
                int cardId = startIdx + i;
                int val = 0;
                
                // Map the loop index to Blackjack values
                if (i == 0) val = 11; // Ace
                else if (i >= 1 && i <= 9) val = i + 1; // 2 through 10
                else val = 10; // J (10), Q (11), K (12) are all worth 10

                _deck.Add(new PlayingCard 
                { 
                    ScoreValue = val, 
                    // "D2" forces numbers like 1 to become "01", matching your files
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

        // Load the specific image dynamically
        string path = $"res://Cards/{drawnCard.ImageName}"; 
        if (ResourceLoader.Exists(path))
        {
            cardVisual.Texture = GD.Load<Texture2D>(path);
        }
        else
        {
            GD.PrintErr($"CRITICAL: Missing card image at: {path}");
        }

        uiContainer.AddChild(cardVisual);
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
        DealerScoreLabel.Text = $"Dealer: {CalculateScore(_dealerHand)}";
        Hand1ScoreLabel.Text = $"Hand 1: {CalculateScore(_hand1)}";
        if (_isSplit) Hand2ScoreLabel.Text = $"Hand 2: {CalculateScore(_hand2)}";
    }

    private void CheckActionButtons()
    {
        List<PlayingCard> currentHand = _activeHand == 1 ? _hand1 : _hand2;
        int score = CalculateScore(currentHand);

        // Can only split if first turn, equal values, and sufficient funds
        SplitBtn.Disabled = _isSplit || _hand1.Count != 2 || _hand1[0].ScoreValue != _hand1[1].ScoreValue || _playerWallet < _currentBet;
        
        DoubleBtn.Disabled = currentHand.Count != 2 || _playerWallet < _currentBet;

        HitBtn.Disabled = false;
        StandBtn.Disabled = false;

        if (score >= 21) OnStandPressed(); 
    }

    // --- BUTTON ACTIONS ---

    public void OnHitPressed()
    {
        if (_activeHand == 1) DealCard(_hand1, Hand1UI);
        else DealCard(_hand2, Hand2UI);

        UpdateScoresUI();
        CheckActionButtons();
    }

    public void OnStandPressed()
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

    public void OnDoubleDownPressed()
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

    public void OnSplitPressed()
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
        HitBtn.Disabled = true;
        StandBtn.Disabled = true;
        DoubleBtn.Disabled = true;
        SplitBtn.Disabled = true;

        while (CalculateScore(_dealerHand) < 17)
        {
            DealCard(_dealerHand, DealerHandUI);
            UpdateScoresUI();
            await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
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

        ResultLabel.Text = resultText + $"\nWallet: ${_playerWallet}";
    }

    private void ClearUI(Node container)
    {
        foreach (Node child in container.GetChildren())
        {
            child.QueueFree();
        }
    }
}