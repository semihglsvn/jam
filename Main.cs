using Godot;

public partial class Main : Node2D 
{
	[Export] public Node PlatformScene; 
	[Export] public Node BlackjackScene; 
	[Export] public Node SlotScene;
	[Export] public Node ArabaScene;
	[Export] public Node KoridorScene;

	[Export] public ColorRect TransitionRect; 
	[Export] public ColorRect TextBackground; 
	[Export] public Label TransitionLabel;

	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private Timer _autoSwitchTimer;
	private bool _isTransitioning = false; 

	private string[] _minioyunListesi = { "Blackjack", "Slots", "Araba", "Koridor" };
	private string _aktifOyun = "Platform"; 

	public override void _Ready()
	{
		OyunDurumunuAyarla(PlatformScene, true, true);
		OyunDurumunuAyarla(BlackjackScene, false, false);
		OyunDurumunuAyarla(SlotScene, false, false);
		OyunDurumunuAyarla(ArabaScene, false, false);
		OyunDurumunuAyarla(KoridorScene, false, false);

		if (TransitionRect != null && TransitionRect.Material != null)
			TransitionRect.Material.Set("shader_parameter/progress", 0.0f);
		if (TextBackground != null) TextBackground.Modulate = new Color(0, 0, 0, 0); 
		if (TransitionLabel != null) TransitionLabel.Modulate = new Color(1, 1, 1, 0); 

		_rng.Randomize();
		_autoSwitchTimer = new Timer();
		_autoSwitchTimer.OneShot = true;
		_autoSwitchTimer.Timeout += OnAutoSwitchTimeout;
		AddChild(_autoSwitchTimer); 

		StartNextRandomTimer();

		// OYUN BAŞLADIĞINDA PLATFORMUN KAMERASINI AKTİF ET
		AktifKamerayiBulVeAyarla(PlatformScene);
	}

	private void OyunDurumunuAyarla(Node oyunDugumu, bool gorunurMu, bool calissinMi)
	{
		if (oyunDugumu == null) return;
		
		if (oyunDugumu is CanvasItem canvasItem) 
		{
			canvasItem.Visible = gorunurMu;
		}
		else if (oyunDugumu is Node3D node3d) 
		{
			node3d.Visible = gorunurMu;
		}
		else
		{
			foreach (Node cocuk in oyunDugumu.GetChildren())
			{
				if (cocuk is CanvasItem c) c.Visible = gorunurMu;
				else if (cocuk is Node3D n) n.Visible = gorunurMu;
			}
		}

		oyunDugumu.ProcessMode = calissinMi ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
	}

	// YENİ EKLENEN SİHİRLİ FONKSİYON: Sahnedeki kamerayı bulup ona odaklanır
	private void AktifKamerayiBulVeAyarla(Node dugum)
	{
		if (dugum == null) return;

		// Eğer aradığımız düğüm bir kameraysa, onu ana kamera yap
		if (dugum is Camera2D cam2D)
		{
			cam2D.MakeCurrent();
			return;
		}

		// Değilse altındaki çocuk düğümleri kontrol et
		foreach (Node cocuk in dugum.GetChildren())
		{
			AktifKamerayiBulVeAyarla(cocuk);
		}
	}

	private void StartNextRandomTimer()
	{
		if (_aktifOyun == "Platform")
		{
			float randomTime = _rng.RandfRange(15.0f, 35.0f);
			_autoSwitchTimer.Start(randomTime);
		}
	}

	private void OnAutoSwitchTimeout()
	{
		if (_isTransitioning || _aktifOyun != "Platform") return;

		string yeniOyun = _minioyunListesi[_rng.RandiRange(0, _minioyunListesi.Length - 1)];

		string mesaj = "";
		if (yeniOyun == "Blackjack") mesaj = "MASAYA OTUR...";
		else if (yeniOyun == "Slots") mesaj = "ŞANSINI DENE...";
		else if (yeniOyun == "Araba") mesaj = "KAÇAMAZSIN... SÜR!";
		else if (yeniOyun == "Koridor") mesaj = "YÜRÜ... SADECE YÜRÜ.";

		SwitchGame(yeniOyun, mesaj);
	}

	public void MinioyunKazanildi()
	{
		if (_isTransitioning || _aktifOyun == "Platform") return;
		SwitchGame("Platform", "GERİ DÖNDÜN...");
	}

	public void SwitchGame(string targetGame, string message)
	{
		if (_isTransitioning) return;
		_isTransitioning = true;

		if (TransitionLabel != null) TransitionLabel.Text = message;
		
		Tween tween = GetTree().CreateTween();
		ShaderMaterial mat = TransitionRect != null ? TransitionRect.Material as ShaderMaterial : null;

		Node aktifDugum = GetOyunDugumu(_aktifOyun);
		if (aktifDugum != null) aktifDugum.ProcessMode = ProcessModeEnum.Disabled;

		if (mat != null)
		{
			tween.TweenProperty(mat, "shader_parameter/progress", 1.0f, 1.0f)
				 .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
		}

		if (TextBackground != null) tween.Parallel().TweenProperty(TextBackground, "modulate:a", 0.95f, 0.4f);
		if (TransitionLabel != null) tween.Parallel().TweenProperty(TransitionLabel, "modulate:a", 1.0f, 0.4f);

		tween.TweenInterval(2.5f);
		tween.TweenCallback(Callable.From(() =>
		{
			OyunDurumunuAyarla(PlatformScene, false, false);
			OyunDurumunuAyarla(BlackjackScene, false, false);
			OyunDurumunuAyarla(SlotScene, false, false);
			OyunDurumunuAyarla(ArabaScene, false, false);
			OyunDurumunuAyarla(KoridorScene, false, false);

			_aktifOyun = targetGame;

			Node yeniDugum = GetOyunDugumu(targetGame);
			if (yeniDugum != null) OyunDurumunuAyarla(yeniDugum, true, false); 
		}));

		if (TextBackground != null) tween.TweenProperty(TextBackground, "modulate:a", 0.0f, 0.4f);
		if (TransitionLabel != null) tween.Parallel().TweenProperty(TransitionLabel, "modulate:a", 0.0f, 0.4f);

		if (mat != null)
		{
			tween.TweenProperty(mat, "shader_parameter/progress", 0.0f, 1.0f)
				 .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
		}

		tween.TweenCallback(Callable.From(() => 
		{ 
			Node yeniDugum = GetOyunDugumu(targetGame);
			if (yeniDugum != null) 
			{
				yeniDugum.ProcessMode = ProcessModeEnum.Inherit;
				
				// YENİ EKLENEN KISIM: Geçiş bitince o oyunun kamerasını ele al!
				AktifKamerayiBulVeAyarla(yeniDugum);
			}
			
			_isTransitioning = false; 

			if (_aktifOyun == "Platform")
			{
				StartNextRandomTimer();
			}
		}));
	}

	private Node GetOyunDugumu(string isim)
	{
		if (isim == "Platform") return PlatformScene;
		if (isim == "Blackjack") return BlackjackScene;
		if (isim == "Slots") return SlotScene;
		if (isim == "Araba") return ArabaScene;
		if (isim == "Koridor") return KoridorScene;
		return null;
	}
}
