using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[ExportCategory("Hareket Ayarları")]
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -400.0f;

	[ExportCategory("Double Jump Ayarları")]
	[Export] public int MaxJumps = 2; // 2 = Double Jump
	private int _jumpCount = 0;

	[ExportCategory("Dash Ayarları")]
	[Export] public float DashSpeed = 800.0f;
	[Export] public float DashDuration = 0.2f; 
	[Export] public float DashCooldown = 1.0f; 

	private bool _isDashing = false;
	private bool _canDash = true;
	private Timer _dashTimer;
	private Timer _dashCooldownTimer;

	// Animasyon Düğümü
	private AnimatedSprite2D _animatedSprite;

	public override void _Ready()
	{
		// Animasyon düğümünü bul
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// Dash zamanlayıcılarını oluştur
		_dashTimer = new Timer();
		_dashTimer.OneShot = true;
		_dashTimer.WaitTime = DashDuration;
		_dashTimer.Timeout += OnDashTimeout;
		AddChild(_dashTimer);

		_dashCooldownTimer = new Timer();
		_dashCooldownTimer.OneShot = true;
		_dashCooldownTimer.WaitTime = DashCooldown;
		_dashCooldownTimer.Timeout += OnDashCooldownTimeout;
		AddChild(_dashCooldownTimer);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// --- YERÇEKİMİ ---
		if (!IsOnFloor())
		{
			if (!_isDashing) velocity += GetGravity() * (float)delta;
		}
		else
		{
			_jumpCount = 0; // Yere değince zıplama hakkı yenilenir
		}

		// --- ZIPLAMA (W veya Boşluk) ---
		if (Input.IsActionJustPressed("jump"))
		{
			if (IsOnFloor() || _jumpCount < MaxJumps)
			{
				velocity.Y = JumpVelocity;
				_jumpCount++;
			}
		}

		// --- A ve D İLE YÖN ALMA ---
		float direction = Input.GetAxis("move_left", "move_right");

		// YÖNÜ DÖNDÜRME MANTIĞI (Havadayken veya dash atarken de çalışması için buraya aldık)
		if (direction != 0)
		{
			_animatedSprite.FlipH = direction < 0; 
		}

		// --- DASH ---
		if (Input.IsActionJustPressed("dash") && _canDash && direction != 0)
		{
			StartDash();
			// Eğer sfxjump adında bir ses düğümün yoksa oyun çöker, emin ol eklediğinden!
			if (HasNode("sfxjump")) 
			{
				AudioStreamPlayer _sfxjump = GetNode<AudioStreamPlayer>("sfxjump");
				_sfxjump.Play();
			}
		}

		if (_isDashing)
		{
			velocity.X = direction * DashSpeed;
			velocity.Y = 0; 
		}
		else
		{
			// Normal Yürüme
			if (direction != 0)
			{
				velocity.X = direction * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			}
		}

		Velocity = velocity;
		MoveAndSlide();

		// --- ANİMASYONLARI OYNATMA ---
		// Artık sadece hangi animasyonun oynayacağına karar veriyor, dönüşe karışmıyor.
		if (!IsOnFloor())
		{
			_animatedSprite.Play("jump"); // Havadayken zıplama animasyonu
		}
		else if (direction != 0)
		{
			_animatedSprite.Play("run"); // Yürürken koşma animasyonu
		}
		else
		{
			_animatedSprite.Play("idle"); // Dururken nefes alma animasyonu
		}
	}

	// --- DASH YARDIMCI FONKSİYONLARI ---
	private void StartDash()
	{
		_isDashing = true;
		_canDash = false;
		_dashTimer.Start();
	}

	private void OnDashTimeout()
	{
		_isDashing = false;
		_dashCooldownTimer.Start();
	}

	private void OnDashCooldownTimeout()
	{
		_canDash = true;
	}
}
