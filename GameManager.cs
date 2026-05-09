using Godot;
using System;

public partial class GameManager : Node
{
	// This allows you to drag and drop your .tscn files into the Inspector
	[Export] public PackedScene[] Games; 
	
	private Node _currentGameInstance;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		_rng.Randomize();
		LoadNextGame();
	}

	public void LoadNextGame()
	{
		// 1. DEFENSIVE CHECK: Prevent crashes if the Inspector array is empty
		if (Games == null || Games.Length == 0)
		{
			GD.PrintErr("CRITICAL: 'Games' array is empty! Click the GameManager node and add your .tscn files in the Inspector.");
			return; 
		}

		// 2. Destroy the old game if it exists
		if (_currentGameInstance != null)
		{
			_currentGameInstance.QueueFree();
		}

		// 3. Pick a random game from your array
		int randomIndex = _rng.RandiRange(0, Games.Length - 1);
		PackedScene nextGame = Games[randomIndex];

		// 4. DEFENSIVE CHECK: Prevent crashes if an array slot was left blank
		if (nextGame == null)
		{
			GD.PrintErr($"CRITICAL: The scene at index {randomIndex} in the Inspector is missing!");
			return;
		}

		// 5. Instantiate and add it to the tree
		_currentGameInstance = nextGame.Instantiate();
		AddChild(_currentGameInstance);

		// 6. Fire your glitch audio/visual effects here!
	}
}
