using Godot;

public partial class EnemyCar : Area2D
{
	// --- YENİ EKLENEN: ARAÇ GÖRSELLERİ LİSTESİ ---
	[Export] public Texture2D[] AracDokulari;

	[Export] public float UfukY       = 260f;   
	[Export] public float ZeminY      = 648f;   
	[Export] public float UfuktaScale = 0.10f;  
	[Export] public float AlttaScale  = 1.20f;  
	[Export] public float TemelHiz    = 120f;   

	[Export] public float YolAltSol   = 250f;   
	[Export] public float YolAltSag   = 900f;   
	[Export] public float MerkezX     = 576f;   

	[Signal]
	public delegate void DusmanGecildiEventHandler();

	private float _ekstraHiz = 0f;
	private float _hedefAltX;               
	private bool  _gecildiSayildi = false;
	
	// Sprite düğümünü kontrol etmek için
	private Sprite2D _sprite;

	public override void _Ready()
	{
		AddToGroup("dusman_carpma");
		
		// Sprite2D düğümünü bul ve hafızaya al
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
	}

	public void Baslat(float ekstraHiz, int seritSecimi = 0)
	{
		_ekstraHiz = ekstraHiz;
		
		// --- SİHİRLİ KISIM: RASTGELE ARABA SEÇİMİ ---
		// Eğer listeye resim eklediysen ve Sprite2D varsa, listeden rastgele birini seç
		if (_sprite != null && AracDokulari != null && AracDokulari.Length > 0)
		{
			int rastgeleIndex = (int)GD.RandRange(0, AracDokulari.Length - 1);
			_sprite.Texture = AracDokulari[rastgeleIndex];
		}

		if (seritSecimi == 1) 
			_hedefAltX = (float)GD.RandRange(YolAltSol, MerkezX - 60f);
		else if (seritSecimi == 2) 
			_hedefAltX = (float)GD.RandRange(MerkezX + 60f, YolAltSag);
		else 
			_hedefAltX = (float)GD.RandRange(YolAltSol, YolAltSag);
		
		Position = new Vector2(MerkezX, UfukY);
		Scale    = new Vector2(UfuktaScale, UfuktaScale);
	}

	public override void _Process(double delta)
	{
		float toplamHiz = TemelHiz + _ekstraHiz;

		float yeni_y = Position.Y + toplamHiz * (float)delta;
		float t = Mathf.Clamp(Mathf.InverseLerp(UfukY, ZeminY, yeni_y), 0f, 1f);
		float s = Mathf.Lerp(UfuktaScale, AlttaScale, t);

		float yeni_x = Mathf.Lerp(MerkezX, _hedefAltX, t);

		Position = new Vector2(yeni_x, yeni_y);
		Scale    = new Vector2(s, s);
		
		ZIndex   = 100 + (int)(t * 100);

		if (!_gecildiSayildi && yeni_y > ZeminY - 30f)
		{
			_gecildiSayildi = true;
			EmitSignal(SignalName.DusmanGecildi);
		}

		if (yeni_y > ZeminY + 150f)
			QueueFree();
	}
}
