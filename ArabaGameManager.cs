using Godot;

public partial class ArabaGameManager : Node
{
	public static float GlobalEkstraHiz = 0f;

	[Export] public PackedScene EnemyScene;
	[Export] public PackedScene PropScene;         
	
	[Export] public Timer SpawnTimerDugumu;    
	[Export] public Timer PropTimerDugumu;         
	[Export] public Label SkorYazisiDugumu;    
	
	[Export] public float PropSpawnAraligi = 0.8f; 

	// --- Zorluk ve Hızlanma Ayarları ---
	[Export] public float HizlanmaMiktari = 4f; 
	[Export] public float DusmanZorlasmaMiktari = 0.02f; 
	// -------------------------------------------------

	[Export] public float MaksimumEkstraHiz = 350f; 
	[Export] public float CiftArabaIhtimali = 0.3f; 

	private int _skor = 0;
	private float _ekstraHiz = 0f;
	private bool _oyunBitti = false;

	public override void _Ready()
	{
		GlobalEkstraHiz = 0f;

		if (SpawnTimerDugumu != null)
			SpawnTimerDugumu.Timeout += DusmanSpawnla;

		if (PropTimerDugumu != null)
		{
			PropTimerDugumu.WaitTime = PropSpawnAraligi;
			PropTimerDugumu.Timeout += PropSpawnla;
		}
	}

	private void DusmanSpawnla()
	{
		if (_oyunBitti || EnemyScene == null) return;

		int kacArabaGelecek = 1;
		if (_ekstraHiz > 50f && GD.Randf() < CiftArabaIhtimali)
			kacArabaGelecek = 2;

		for (int i = 0; i < kacArabaGelecek; i++)
		{
			Node2D yeniDusman = (Node2D)EnemyScene.Instantiate();
			GetParent().AddChild(yeniDusman); 

			// EnemyCar sınıfını kullanıyoruz
			if (yeniDusman is EnemyCar dusmanAraci)
			{
				int seritSecimi = (kacArabaGelecek == 2) ? (i == 0 ? 1 : 2) : 0;
				dusmanAraci.Baslat(_ekstraHiz, seritSecimi);
				dusmanAraci.DusmanGecildi += SkorEkle;
			}
		}
	}

	private void PropSpawnla()
	{
		if (_oyunBitti || PropScene == null) return;

		Node2D yeniProp = (Node2D)PropScene.Instantiate();
		GetParent().AddChild(yeniProp);

		if (yeniProp is YolKenariObjesi propObjesi)
		{
			float rastgeleYon = GD.Randf() > 0.5f ? 1f : -1f;
			propObjesi.Baslat(rastgeleYon);
		}
	}

	public void SkorEkle()
	{
		if (_oyunBitti) return;
		_skor += 10;
		SkoruGuncelle();
		OyunuZorlastir();
	}

	private void SkoruGuncelle()
	{
		if (SkorYazisiDugumu != null)
			SkorYazisiDugumu.Text = "Skor: " + _skor;
	}

	private void OyunuZorlastir()
	{
		if (_oyunBitti) return;
		
		_ekstraHiz += HizlanmaMiktari; 
		if (_ekstraHiz > MaksimumEkstraHiz) _ekstraHiz = MaksimumEkstraHiz;
		GlobalEkstraHiz = _ekstraHiz;

		if (SpawnTimerDugumu != null && SpawnTimerDugumu.WaitTime > 0.6f)
			SpawnTimerDugumu.WaitTime -= DusmanZorlasmaMiktari;

		if (PropTimerDugumu != null && PropTimerDugumu.WaitTime > 0.2f)
			PropTimerDugumu.WaitTime -= (DusmanZorlasmaMiktari / 2f);
	}

	public void OyunuBitir()
	{
		_oyunBitti = true;
		GlobalEkstraHiz = 0f;
		if (SpawnTimerDugumu != null) SpawnTimerDugumu.Stop();
		if (PropTimerDugumu != null) PropTimerDugumu.Stop();
	}
}
