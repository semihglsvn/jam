using Godot;

public partial class Main : Node
{
    [Export] public Control BlackjackScene; 
    [Export] public Control SlotScene;
    [Export] public ColorRect TransitionRect; 
    [Export] public ColorRect TextBackground; 
    [Export] public Label TransitionLabel;

    // --- NEW: Auto-Switch Variables ---
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private Timer _autoSwitchTimer;
    private bool _isTransitioning = false; 

    public override void _Ready()
    {
        BlackjackScene.Visible = true;
        BlackjackScene.ProcessMode = ProcessModeEnum.Inherit;
        
        SlotScene.Visible = false;
        SlotScene.ProcessMode = ProcessModeEnum.Disabled;

        TransitionRect.Material.Set("shader_parameter/progress", 0.0f);
        TextBackground.Modulate = new Color(0, 0, 0, 0); 
        TransitionLabel.Modulate = new Color(1, 1, 1, 0); 

        // --- NEW: Setup the invisible timer ---
        _rng.Randomize();
        _autoSwitchTimer = new Timer();
        _autoSwitchTimer.OneShot = true;
        _autoSwitchTimer.Timeout += OnAutoSwitchTimeout;
        AddChild(_autoSwitchTimer); // Add it to the game invisibly

        // Start the first countdown!
        StartNextRandomTimer();
    }

    // --- NEW: The Timer Methods ---
    private void StartNextRandomTimer()
    {
        float randomTime = _rng.RandfRange(15.0f, 45.0f);
        _autoSwitchTimer.Start(randomTime);
    }

    private void OnAutoSwitchTimeout()
    {
        // Don't auto-switch if the player just clicked the manual button
        if (_isTransitioning) return;

        // Figure out which game they are playing, and violently switch to the other one!
        if (BlackjackScene.Visible)
        {
            SwitchGame("Slots", "SYSTEM OVERRIDE...\nFORCED SHIFT TO SLOTS.");
        }
        else
        {
            SwitchGame("Blackjack", "TIME IS UP...\nBACK TO THE TABLE.");
        }
    }

    public void SwitchGame(string targetGame, string message)
    {
        // Safety lock so transitions don't overlap
        if (_isTransitioning) return;
        _isTransitioning = true;

        // Reset the timer! If they manually clicked the button, this gives them a fresh 15-45 seconds.
        if (_autoSwitchTimer != null) StartNextRandomTimer();

        TransitionLabel.Text = message;
        
        Tween tween = GetTree().CreateTween();
        ShaderMaterial mat = TransitionRect.Material as ShaderMaterial;

        // 1. GLITCH IN
        tween.TweenProperty(mat, "shader_parameter/progress", 1.0f, 1.0f)
             .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);

        // 2. READABILITY
        tween.TweenProperty(TextBackground, "modulate:a", 0.95f, 0.4f);
        tween.Parallel().TweenProperty(TransitionLabel, "modulate:a", 1.0f, 0.4f);

        // 3. THE PAUSE & SWAP
        tween.TweenInterval(2.5f);
        tween.TweenCallback(Callable.From(() =>
        {
            if (targetGame == "Slots")
            {
                BlackjackScene.Visible = false;
                BlackjackScene.ProcessMode = ProcessModeEnum.Disabled;
                SlotScene.Visible = true;
                SlotScene.ProcessMode = ProcessModeEnum.Inherit;       
            }
            else if (targetGame == "Blackjack")
            {
                SlotScene.Visible = false;
                SlotScene.ProcessMode = ProcessModeEnum.Disabled;
                BlackjackScene.Visible = true;
                BlackjackScene.ProcessMode = ProcessModeEnum.Inherit;
            }
        }));

        // 4. FADE OUT TEXT
        tween.TweenProperty(TextBackground, "modulate:a", 0.0f, 0.4f);
        tween.Parallel().TweenProperty(TransitionLabel, "modulate:a", 0.0f, 0.4f);

        // 5. GLITCH OUT
        tween.TweenProperty(mat, "shader_parameter/progress", 0.0f, 1.0f)
             .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);

        // 6. UNLOCK THE SAFETY
        tween.TweenCallback(Callable.From(() =>
        {
            _isTransitioning = false;
        }));
    }
}