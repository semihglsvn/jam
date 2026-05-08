using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SlotGame : Control
{
    [Export] public Texture2D[] Symbols; // Drag your 4 symbols (7, Cherry, Bell, BAR) here
    [Export] public GridContainer ReelGrid;
    [Export] public Label WinLabel;
    [Export] public TextureButton LeverButton;
    
    // Audio Hooks
    [Export] public AudioStreamPlayer SpinSFX;
    [Export] public AudioStreamPlayer StopSFX;
    [Export] public AudioStreamPlayer WinSFX;

    private List<TextureRect> _gridSlots = new List<TextureRect>();
    private const int GridSize = 9; // 3x3 grid
    private bool _isExiting = false;

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete) _isExiting = true;
    }

	public override void _Ready()
    {
        // Generate the 9 empty slots
        for (int i = 0; i < GridSize; i++)
        {
            TextureRect newSlot = new TextureRect();
            newSlot.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            newSlot.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            newSlot.CustomMinimumSize = new Vector2(60, 60); 
            
            // ADD THESE TWO LINES:
            newSlot.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            newSlot.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            
            ReelGrid.AddChild(newSlot);
            _gridSlots.Add(newSlot);
        }
    }
    public async void OnLeverPulled()
    {
        if (_isExiting) return;

        LeverButton.Disabled = true; // Lever stays down while spinning
        
        foreach (var slot in _gridSlots) slot.Modulate = Colors.White;

        if (SpinSFX != null) SpinSFX.Play();

        // Fake Spin Visuals
        for (int i = 0; i < 12; i++)
        {
            RandomizeBoard();
            await ToSignal(GetTree().CreateTimer(0.05f), SceneTreeTimer.SignalName.Timeout);
            if (_isExiting) return;
        }
        
        if (SpinSFX != null) SpinSFX.Stop();
        if (StopSFX != null) StopSFX.Play();

        // Final Roll
        Texture2D[] finalBoard = RandomizeBoard();
        EvaluatePaylines(finalBoard);
        
        LeverButton.Disabled = false; // Lever pops back up
    }

    private Texture2D[] RandomizeBoard()
    {
        Texture2D[] currentBoard = new Texture2D[GridSize];
        for (int i = 0; i < _gridSlots.Count; i++)
        {
            Texture2D rolled = Symbols[GD.Randi() % Symbols.Length];
            _gridSlots[i].Texture = rolled;
            currentBoard[i] = rolled;
        }
        return currentBoard;
    }

    private void EvaluatePaylines(Texture2D[] board)
    {
        int totalWin = 0;

        // Check the 3 horizontal rows (indexes: 0-1-2, 3-4-5, 6-7-8)
        for (int row = 0; row < 3; row++)
        {
            int startIndex = row * 3;
            if (board[startIndex] == board[startIndex + 1] && board[startIndex + 1] == board[startIndex + 2])
            {
                // We have a 3-in-a-row!
                totalWin += 50; 
                HighlightWinningRow(startIndex);
            }
        }

        if (totalWin > 0)
        {
            if (WinSFX != null) WinSFX.Play();
        }
        else
        {
        }
    }

    private void HighlightWinningRow(int startIndex)
    {
        // Make the winning row pop, dim the rest
        for (int i = 0; i < _gridSlots.Count; i++)
        {
            if (i >= startIndex && i <= startIndex + 2)
            {
                _gridSlots[i].Modulate = new Color(1.5f, 1.5f, 1.5f); // Brighten
            }
            else if (_gridSlots[i].Modulate == Colors.White) 
            {
                _gridSlots[i].Modulate = new Color(0.4f, 0.4f, 0.4f); // Dim only if not part of another win
            }
        }
    }
}