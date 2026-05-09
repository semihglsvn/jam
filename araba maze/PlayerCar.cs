using Godot;

public partial class PlayerCar : CharacterBody2D
{
	[Export] public float Hiz        = 400f;
	[Export] public float SolSinir   = 250f;   
	[Export] public float SagSinir   = 900f;   
	[Export] public float BaslangicX = 576f;   

	[Export] public float AracYPos   = 560f;   
	[Export] public float AracScale  = 1.0f;   

	private bool _oldu = false;
	
	// YENİ EKLENEN: Animasyon düğümümüzü kontrol etmek için
	private AnimatedSprite2D _animSprite;

	[Signal]
	public delegate void OyuncuCarptiEventHandler();

	public override void _Ready()
	{
		Position = new Vector2(BaslangicX, AracYPos);
		Scale    = new Vector2(AracScale, AracScale);

		// AnimatedSprite2D düğümünü bul
		_animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		
		// Oyun başlarken düz gitme animasyonunu başlat
		if (_animSprite != null)
			_animSprite.Play("duz");

		var hitarea = GetNodeOrNull<Area2D>("hitarea");
		if (hitarea != null)
			hitarea.AreaEntered += CarpismayiAlgiladi;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_oldu) return;

		float yon = 0f;
		if (Input.IsActionPressed("ui_left"))  yon = -1f;
		if (Input.IsActionPressed("ui_right")) yon =  1f;

		// =========================================================
		// SİHİRLİ ANİMASYON KONTROL KISMI
		// =========================================================
		if (_animSprite != null)
		{
			if (yon == -1f)
				_animSprite.Play("sol");   // Sola basılıyorsa "sol" animasyonunu oynat
			else if (yon == 1f)
				_animSprite.Play("sag");   // Sağa basılıyorsa "sag" animasyonunu oynat
			else
				_animSprite.Play("duz");   // Hiçbir tuşa basılmıyorsa "duz" animasyona dön
		}

		float yeniX = Position.X + yon * Hiz * (float)delta;
		yeniX = Mathf.Clamp(yeniX, SolSinir, SagSinir);

		Position = new Vector2(yeniX, AracYPos);

		Velocity = Vector2.Zero;
		MoveAndSlide();
	}

	private void CarpismayiAlgiladi(Area2D alan)
	{
		if (_oldu) return;
		if (alan.IsInGroup("dusman_carpma"))
		{
			_oldu = true;
			EmitSignal(SignalName.OyuncuCarpti);
		}
	}

	public void Sifirla()
	{
		_oldu    = false;
		Position = new Vector2(BaslangicX, AracYPos);
		
		// Ölürse veya sıfırlanırsa tekrar düz animasyona geç
		if (_animSprite != null) 
			_animSprite.Play("duz");
	}
}
