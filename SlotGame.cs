using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SlotGame : Control
{
    [Export] public Texture2D[] Symbols; 
    [Export] public GridContainer ReelGrid;
    [Export] public Label WinLabel;
    [Export] public TextureButton LeverButton;
    [Export] public Button GoToBlackjackBtn; // <--- NEW: The return button!
    
    // Audio Hooks
    [Export] public AudioStreamPlayer SpinSFX;
    [Export] public AudioStreamPlayer StopSFX;
    [Export] public AudioStreamPlayer WinSFX;

    // --- Juice Hooks ---
    [ExportCategory("Juice")]
    [Export] public Camera2D MainCamera;
    [Export] public GpuParticles2D CoinParticles; 
    
    private float _shakeStrength = 0.0f;
    private const float ShakeFade = 5.0f; 
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    // ------------------------

    private List<TextureRect> _gridSlots = new List<TextureRect>();
    private const int GridSize = 9; 
    private bool _isExiting = false;

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete) _isExiting = true;
    }

    public override void _Ready()
    {
        _rng.Randomize();

        // <--- NEW: Wire up the button click!
        if (GoToBlackjackBtn != null)
        {
            GoToBlackjackBtn.Pressed += OnGoToBlackjackPressed;
        }

        // Generate the 9 empty slots
        for (int i = 0; i < GridSize; i++)
        {
            TextureRect newSlot = new TextureRect();
            newSlot.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            newSlot.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            newSlot.CustomMinimumSize = new Vector2(60, 60); 
            
            newSlot.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            newSlot.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            
            ReelGrid.AddChild(newSlot);
            _gridSlots.Add(newSlot);
        }
    }

    // <--- NEW: The Transition Method
    private void OnGoToBlackjackPressed()
    {
        // Use the absolute path to find Main
        Main mainScene = GetNodeOrNull<Main>("/root/Main"); 
        
        if (mainScene != null)
        {
            mainScene.SwitchGame("Blackjack", "DON'T GIVE UP. DOUBLE DOWN.");
        }
        else
        {
            GD.PrintErr("CRITICAL: Could not find the Main node at /root/Main!");
        }
    }

    // --- The Screen Shake Loop ---
    public override void _Process(double delta)
    {
        if (_shakeStrength > 0.05f && MainCamera != null)
        {
            _shakeStrength = Mathf.Lerp(_shakeStrength, 0, ShakeFade * (float)delta);
            
            Vector2 randomOffset = new Vector2(
                _rng.RandfRange(-_shakeStrength, _shakeStrength),
                _rng.RandfRange(-_shakeStrength, _shakeStrength)
            );
            
            MainCamera.Offset = randomOffset;
        }
        else if (MainCamera != null && MainCamera.Offset != Vector2.Zero)
        {
            MainCamera.Offset = Vector2.Zero;
        }
    }

    public async void OnLeverPulled()
    {
        if (_isExiting) return;

        LeverButton.Disabled = true; 
        if (GoToBlackjackBtn != null) GoToBlackjackBtn.Disabled = true; // Don't let them leave while spinning!
        
        foreach (var slot in _gridSlots) slot.Modulate = Colors.White;

        if (SpinSFX != null) SpinSFX.Play();

        for (int i = 0; i < 12; i++)
        {
            RandomizeBoard();
            await ToSignal(GetTree().CreateTimer(0.05f), SceneTreeTimer.SignalName.Timeout);
            if (_isExiting) return;
        }
        
        if (SpinSFX != null) SpinSFX.Stop();
        if (StopSFX != null) StopSFX.Play();

        Texture2D[] finalBoard = RandomizeBoard();
        EvaluatePaylines(finalBoard);
        
        LeverButton.Disabled = false; 
        if (GoToBlackjackBtn != null) GoToBlackjackBtn.Disabled = false;
    }

    private Texture2D[] RandomizeBoard()
    {
        Texture2D[] currentBoard = new Texture2D[GridSize];
        for (int i = 0; i < _gridSlots.Count; i++)
        {
            Texture2D rolled = Symbols[_rng.Randi() % Symbols.Length];
            _gridSlots[i].Texture = rolled;
            currentBoard[i] = rolled;
        }
        return currentBoard;
    }

    private void EvaluatePaylines(Texture2D[] board)
    {
        int totalWin = 0;

        for (int row = 0; row < 3; row++)
        {
            int startIndex = row * 3;
            if (board[startIndex] == board[startIndex + 1] && board[startIndex + 1] == board[startIndex + 2])
            {
                totalWin += 50; 
                HighlightWinningRow(startIndex);
            }
        }

        if (totalWin > 0)
        {
            if (WinSFX != null) WinSFX.Play();
            TriggerJackpotVisuals(); 
        }
    }

    private void TriggerJackpotVisuals()
    {
        _shakeStrength = 35.0f; 
        
        if (CoinParticles != null)
        {
            CoinParticles.Restart(); 
        }
    }
    
    private void HighlightWinningRow(int startIndex)
    {
        for (int i = 0; i < _gridSlots.Count; i++)
        {
            if (i >= startIndex && i <= startIndex + 2)
            {
                _gridSlots[i].Modulate = new Color(1.5f, 1.5f, 1.5f); 
            }
            else if (_gridSlots[i].Modulate == Colors.White) 
            {
                _gridSlots[i].Modulate = new Color(0.4f, 0.4f, 0.4f); 
            }
        }
    }
}