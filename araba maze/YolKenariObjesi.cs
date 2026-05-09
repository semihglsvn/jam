using Godot;

public partial class YolKenariObjesi : Node2D
{
	[Export] public Texture2D[] PropDokulari; 

	[Export] public float UfukY       = 260f;
	[Export] public float ZeminY      = 648f;
	[Export] public float UfuktaScale = 0.05f; 
	[Export] public float AlttaScale  = 3.0f;  

	[Export] public float TemelHiz    = 280f;  
	[Export] public float MerkezX     = 576f;

	// --- BURAYI DEĞİŞTİRDİK (Daha sağa ve sola açılmaları için değerleri büyüttük) ---
	[Export] public float UfuktaXAcikligi = 100f;  // Eskiden 30'du. (Ufukta yoldan ne kadar uzak)
	[Export] public float AlttaXAcikligi  = 1500f; // Eskiden 950'ydi. (Aşağı indikçe ne kadar dışarı taşacak)
	// ---------------------------------------------------------------------------------

	private float _yon = 1f; 

	public void Baslat(float yon)
	{
		_yon = yon; 

		Sprite2D sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (sprite != null && PropDokulari != null && PropDokulari.Length > 0)
		{
			int rastgeleIndex = (int)GD.RandRange(0, PropDokulari.Length - 1);
			sprite.Texture = PropDokulari[rastgeleIndex];
		}

		float baslangicX = MerkezX + (UfuktaXAcikligi * _yon);
		Position = new Vector2(baslangicX, UfukY);
		Scale = new Vector2(UfuktaScale, UfuktaScale);
	}

	public override void _Process(double delta)
	{
		float toplamHiz = TemelHiz + GameManager.GlobalEkstraHiz;

		float yeni_y = Position.Y + toplamHiz * (float)delta;
		float t = Mathf.Clamp(Mathf.InverseLerp(UfukY, ZeminY, yeni_y), 0f, 1f);
		float s = Mathf.Lerp(UfuktaScale, AlttaScale, t);

		float baslangicX = MerkezX + (UfuktaXAcikligi * _yon);
		float hedefX     = MerkezX + (AlttaXAcikligi * _yon);
		float yeni_x     = Mathf.Lerp(baslangicX, hedefX, t);

		Position = new Vector2(yeni_x, yeni_y);
		Scale = new Vector2(s, s);

		// Z-Index: Çimenlerin üstünde kalmasını garantilemek için
		ZIndex = 500 + (int)(t * 100);

		if (yeni_y > ZeminY + 200f)
		{
			QueueFree();
		}
	}
}
