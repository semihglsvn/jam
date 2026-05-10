using Godot;
using System;

public partial class WinScreen : CanvasLayer
{
    [Export] public Label MessageLabel; // Connect your "YOU ESCAPED" label here
    [Export] public Label NameLabel;    // Connect your "İsimYazisi" label here

    public override void _Ready()
    {
        // Make sure the screen is completely invisible when the game starts
        Visible = false;
    }

    // This is the exact function your Main.cs is looking for!
    public void EkraniGoster(string playerName)
    {
        // 1. Set the texts
        if (MessageLabel != null)
        {
            MessageLabel.Text = "YOU ESCAPED,";
        }

        if (NameLabel != null)
        {
            // We make the name ALL CAPS so it looks dramatic and glitchy
            NameLabel.Text = playerName.ToUpper(); 
        }

        // 2. Make the screen visible
        Visible = true;

        // 3. Pause the game so the player can't fall or die after winning!
        GetTree().Paused = true; 
    }
}