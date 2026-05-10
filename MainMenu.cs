using Godot;

public partial class MainMenu : Control
{
    [Export] public Button EnterButton;

    public override void _Ready()
    {
        // Düğmeye basıldığında anında Main.tscn (Ana Oyun) sahnesine geçer
        if (EnterButton != null)
        {
            EnterButton.Pressed += () => GetTree().ChangeSceneToFile("res://Main.tscn");
        }
    }
}