using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [ExportCategory("Movement")]
    [Export] public float Speed = 280.0f;          
    [Export] public float JumpVelocity = -500.0f;  
    [Export] public float Acceleration = 2000.0f;  
    [Export] public float Friction = 3000.0f;      
    [Export] public int MaxJumps = 1;              

    [ExportCategory("Game Feel")]
    [Export] public float CoyoteTime = 0.15f;      // Ledge jump window
    [Export] public float JumpBufferTime = 0.1f;   // Early jump window
    private float _coyoteTimer = 0f;
    private float _jumpBufferTimer = 0f;

    [ExportCategory("Dash Settings")]
    [Export] public float DashSpeed = 800.0f;      
    [Export] public float DashDuration = 0.15f;    
    [Export] public float DashCooldown = 0.5f;     

    private int _jumpCount = 0;
    private bool _isDashing = false;
    private bool _canDash = true;
    private bool _hasWon = false; 
    
    private Timer _dashTimer;
    private Timer _dashCooldownTimer;
    private Vector2 _dashDirection = Vector2.Zero; 
    private AnimatedSprite2D _animatedSprite;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        _dashTimer = new Timer();
        _dashTimer.OneShot = true;
        _dashTimer.Timeout += () => _isDashing = false;
        AddChild(_dashTimer);

        _dashCooldownTimer = new Timer();
        _dashCooldownTimer.OneShot = true;
        _dashCooldownTimer.Timeout += () => _canDash = true;
        AddChild(_dashCooldownTimer);
    }

    public override void _PhysicsProcess(double delta)
    {
        float fDelta = (float)delta;

        // 1. OMNIDIRECTIONAL DASH LOGIC
        if (_isDashing)
        {
            Velocity = _dashDirection * DashSpeed;
            MoveAndSlide();
            return; 
        }

        Vector2 velocity = Velocity;

        // 2. COYOTE TIME & GRAVITY
        if (IsOnFloor())
        {
            _coyoteTimer = CoyoteTime; // Reset window while on floor
            _jumpCount = 0; 
        }
        else
        {
            _coyoteTimer -= fDelta;
            velocity += GetGravity() * fDelta;
        }

        // 3. JUMP BUFFERING
        if (Input.IsActionJustPressed("jump"))
        {
            _jumpBufferTimer = JumpBufferTime;
        }
        else
        {
            _jumpBufferTimer -= fDelta;
        }

        // 4. SMART JUMPING (Coyote + Buffer)
        if (_jumpBufferTimer > 0)
        {
            // If we have coyote time OR we have air jumps left
            if (_coyoteTimer > 0 || _jumpCount < MaxJumps)
            {
                velocity.Y = JumpVelocity;
                _jumpCount++;
                _jumpBufferTimer = 0; // Use the buffer
                _coyoteTimer = 0;     // Use the coyote time
            }
        }

        // 5. OMNIDIRECTIONAL INPUT
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (Input.IsActionJustPressed("dash") && _canDash)
        {
            // If holding directions, dash that way. Otherwise dash where facing.
            _dashDirection = inputDir != Vector2.Zero ? inputDir.Normalized() : new Vector2(_animatedSprite.FlipH ? -1.0f : 1.0f, 0.0f);
            
            _isDashing = true;
            _canDash = false;
            _dashTimer.Start(DashDuration);
            _dashCooldownTimer.Start(DashCooldown);

            if (HasNode("sfxjump")) GetNode<AudioStreamPlayer>("sfxjump").Play();
            return;
        }

        // Horizontal Movement
        if (inputDir.X != 0)
        {
            velocity.X = Mathf.MoveToward(velocity.X, inputDir.X * Speed, Acceleration * fDelta);
            _animatedSprite.FlipH = inputDir.X < 0;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * fDelta);
        }

        Velocity = velocity;
        MoveAndSlide();

        // Animations
        if (!IsOnFloor()) _animatedSprite.Play("jump"); 
        else if (inputDir.X != 0) _animatedSprite.Play("run"); 
        else _animatedSprite.Play("idle"); 
    }
}