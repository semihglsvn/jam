using Godot;

public partial class LinesScroller : Node2D
{
	[Export] public Texture2D CizgiDokusu; 
	
	[Export] public int CizgiSayisi = 8;      
	[Export] public float TemelHiz = 8f;     
	
	[Export] public float UfukY = 260f;       
	[Export] public float EkranAltY = 648f;   
	[Export] public float MerkezX = 576f;     
	
	[Export] public float MaxZ = 100f;        
	[Export] public float ScaleCarpaniX = 2f; 
	[Export] public float ScaleCarpaniY = 8f; 

	private class Cizgi
	{
		public Sprite2D Sprite;
		public float Z; 
	}

	private Cizgi[] _cizgiler;
	private float _k; 

	public override void _Ready()
	{
		_k = EkranAltY - UfukY; 
		_cizgiler = new Cizgi[CizgiSayisi];
		
		float zAralik = MaxZ / CizgiSayisi;

		for (int i = 0; i < CizgiSayisi; i++)
		{
			Sprite2D spr = new Sprite2D();
			spr.Texture = CizgiDokusu;
			AddChild(spr); 

			_cizgiler[i] = new Cizgi
			{
				Sprite = spr,
				Z = MaxZ - (i * zAralik)
			};
		}
	}

	public override void _Process(double delta)
	{
		if (CizgiDokusu == null) return;

		// Global hız sınırı olduğu için bu hız zaten belirli bir yerde sabitlenecek
		float guncelHiz = TemelHiz + (ArabaGameManager.GlobalEkstraHiz / 15f);

		foreach (var c in _cizgiler)
		{
			c.Z -= guncelHiz * (float)delta; 

			if (c.Z <= 1f)
			{
				c.Z += MaxZ; 
			}
			
			float y = UfukY + (_k / c.Z);
			c.Sprite.Position = new Vector2(MerkezX, y);

			float scale = 1f / c.Z;
			
			// Çizgilerin uzamasına %250 (3.5f) sınırı koyduk, daha fazla uzamayacaklar
			float uzamaEfekti = 1f + (ArabaGameManager.GlobalEkstraHiz / 100f); 
			uzamaEfekti = Mathf.Min(uzamaEfekti, 3.5f); 
			
			c.Sprite.Scale = new Vector2(scale * ScaleCarpaniX, scale * ScaleCarpaniY * uzamaEfekti);
		}
	}
}
