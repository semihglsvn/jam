using Godot;
using System;

public partial class GameManager : Node2D
{
    // This allows you to drag and drop your 6 .tscn files into the Inspector
    [Export] public PackedScene[] MicroGames; 
    
    private Node2D _currentGameInstance;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        _rng.Randomize();
        LoadNextGame();
    }

    public void LoadNextGame()
    {
        // 1. Destroy the old game if it exists
        if (_currentGameInstance != null)
        {
            _currentGameInstance.QueueFree();
        }

        // 2. Pick a random game from your array
        int randomIndex = _rng.RandiRange(0, MicroGames.Length - 1);
        PackedScene nextGame = MicroGames[randomIndex];

        // 3. Instantiate and add it to the tree
        _currentGameInstance = nextGame.Instantiate<Node2D>();
        AddChild(_currentGameInstance);

        // 4. Fire your glitch audio/visual effects here!
    }
}