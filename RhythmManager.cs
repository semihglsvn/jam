using Godot;

public partial class RhythmManager : Node
{
    [Export] public TileMapLayer LayerStandard; // Always Solid
    [Export] public TileMapLayer LayerB;        // Swaps
    [Export] public TileMapLayer LayerC;        // Swaps
    [Export] public float BeatInterval = 2.0f;

    private Timer _beatTimer;
    private bool _stateToggle = true;

    public override void _Ready()
    {
        // Safety: Make sure the layers are actually at (0,0) so they don't shift
        if (LayerB != null) LayerB.Position = Vector2.Zero;
        if (LayerC != null) LayerC.Position = Vector2.Zero;

        _beatTimer = new Timer();
        _beatTimer.WaitTime = BeatInterval;
        _beatTimer.OneShot = false;
        _beatTimer.Timeout += () => {
            _stateToggle = !_stateToggle;
            UpdateWorld();
        };
        AddChild(_beatTimer);
        _beatTimer.Start();

        UpdateWorld();
    }

    private void UpdateWorld()
    {
        // Dimension A
        if (_stateToggle)
        {
            SetLayerState(LayerB, true);  // B: Glitch/Ghost
            SetLayerState(LayerC, false); // C: Normal/Solid
        }
        // Dimension B
        else
        {
            SetLayerState(LayerB, false); // B: Normal/Solid
            SetLayerState(LayerC, true);  // C: Glitch/Ghost
        }
    }

    private void SetLayerState(TileMapLayer layer, bool shouldGlitch)
    {
        // FIXED NULL CHECK: We don't use layer.Name if layer is null!
        if (layer == null) return;

        // 1. Physics: Ghost blocks don't have collisions
        layer.CollisionEnabled = !shouldGlitch;

        // 2. Visuals: Toggle the "is_active" parameter in your shader
        if (layer.Material is ShaderMaterial mat)
        {
            mat.SetShaderParameter("is_active", shouldGlitch ? 1.0f : 0.0f);
        }
        else
        {
            GD.PrintErr($"{layer.Name} is missing a ShaderMaterial! Check the Inspector.");
        }
    }
}