using Godot;

public partial class Player : CharacterBody2D
{
    // --- Tweaks ---
    public const float Speed = 300.0f;
    public const float JumpVelocity = -400.0f;
    public const float DashSpeed = 900.0f;
    public const float DashDuration = 0.15f; 
    public const float DashCooldown = 0.5f;

    // --- State ---
    private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private bool _isDashing = false;
    private float _dashTimeLeft = 0.0f;
    private float _dashCooldownLeft = 0.0f;
    
    // NEW: We track a 2D arrow instead of just left/right
    private Vector2 _lastAimDirection = new Vector2(1, 0); 
    private Vector2 _dashDirection = Vector2.Zero;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // 1. Cooldown Timers
        if (_dashCooldownLeft > 0) _dashCooldownLeft -= (float)delta;

        // 2. Track Aim Direction (We do this first so we always know where you are looking)
        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (inputDir != Vector2.Zero)
        {
            // .Normalized() ensures diagonal dashes aren't faster than straight ones!
            _lastAimDirection = inputDir.Normalized(); 
        }

        // 3. Dash Execution
        if (_isDashing)
        {
            _dashTimeLeft -= (float)delta;
            if (_dashTimeLeft <= 0)
            {
                _isDashing = false;
                velocity = Vector2.Zero; // Stop dead instantly at the end of the dash
            }
            else
            {
                // Lock velocity in ALL directions! Ignore gravity completely.
                Velocity = _dashDirection * DashSpeed;
                MoveAndSlide();
                return; // Skip normal physics entirely!
            }
        }

        // 4. Normal Gravity (Only applies if we are NOT dashing)
        if (!IsOnFloor())
            velocity.Y += _gravity * (float)delta;

        // 5. Jump
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            velocity.Y = JumpVelocity;

        // 6. Normal Left/Right Movement
        if (inputDir.X != 0)
        {
            velocity.X = inputDir.X * Speed;
        }
        else
        {
            // Instantly stop moving when letting go (arcade friction)
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed * 10f * (float)delta);
        }

        // 7. Start a Dash
        if (Input.IsActionJustPressed("dash") && _dashCooldownLeft <= 0 && !_isDashing)
        {
            _isDashing = true;
            _dashTimeLeft = DashDuration;
            _dashCooldownLeft = DashCooldown;
            
            // Snapshot the exact angle you were holding when you pressed Dash!
            _dashDirection = _lastAimDirection; 
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}