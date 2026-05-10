using Godot;

public partial class Main : Node2D 
{
    [ExportCategory("Background Music")]
    [Export] public AudioStreamPlayer BGMPlayer;
    [Export] public AudioStream PlatformMusic;
    [Export] public AudioStream BlackjackMusic;
    [Export] public AudioStream SlotsMusic;
    [Export] public AudioStream ArabaMusic;
    [Export] public AudioStream KoridorMusic;

    [ExportCategory("Game Scenes")]
    [Export] public Node PlatformScene; 
    [Export] public Node BlackjackScene; 
    [Export] public Node SlotScene;
    [Export] public Node ArabaScene;
    [Export] public Node KoridorScene;

    [ExportCategory("Transition UI")]
    [Export] public ColorRect TransitionRect; 
    [Export] public ColorRect TextBackground; 
    [Export] public Label TransitionLabel;

    [ExportCategory("Game Durations (Seconds)")]
    [Export] public float PlatformDuration = 5.0f;
    [Export] public float BlackjackDuration = 5.0f;
    [Export] public float SlotsDuration = 5.0f;
    [Export] public float ArabaDuration = 40.0f;
    [Export] public float KoridorDuration = 5.0f;

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

        if (BGMPlayer != null && PlatformMusic != null)
        {
            BGMPlayer.Stream = PlatformMusic;
            BGMPlayer.Play();
        }

        _rng.Randomize();
        _autoSwitchTimer = new Timer();
        _autoSwitchTimer.OneShot = true;
        _autoSwitchTimer.Timeout += OnAutoSwitchTimeout;
        AddChild(_autoSwitchTimer); 

        StartNextTimer();
        
        // --- YENİ BAŞLANGIÇ: Önce tüm kameraları kapat, sonra sadece platformu aç ---
        TumKameralariKapat(this);
        AktifKamerayiBulVeAc(PlatformScene);
    }

		private void OyunDurumunuAyarla(Node oyunDugumu, bool gorunurMu, bool calissinMi)
    {
        if (oyunDugumu == null) return;
        
        // 1. Görsel Olarak Kapat/Aç
        if (oyunDugumu is CanvasItem canvasItem) canvasItem.Visible = gorunurMu;
        else if (oyunDugumu is Node3D node3d) node3d.Visible = gorunurMu;
        else
        {
            foreach (Node cocuk in oyunDugumu.GetChildren())
            {
                if (cocuk is CanvasItem c) c.Visible = gorunurMu;
                else if (cocuk is Node3D n) n.Visible = gorunurMu;
            }
        }

        // --- YENİ: UZAYA FIRLATMA (The Banishment Fix) ---
        // Görünmez olan oyunları fiziksel olarak -50.000 koordinatına ışınla.
        // Böylece diğer oyunların görünmez duvarlarına çarpıp arabayı sıkıştıramazlar!
        if (oyunDugumu is Node2D n2d)
        {
            n2d.Position = gorunurMu ? Vector2.Zero : new Vector2(-50000, -50000);
        }

        // 3. İşlemleri ve Zamanlayıcıları Dondur
        oyunDugumu.ProcessMode = calissinMi ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
    }

    // --- KUSURSUZ ÇÖZÜM: Bütün sahnedeki kameraları devre dışı bırakır ---
    private void TumKameralariKapat(Node root)
    {
        if (root == null) return;

        // 2D ve 3D kameraları bulup kapatıyoruz (Default Viewport'a dönmek için)
        if (root is Camera2D cam2D) cam2D.Enabled = false;
        else if (root is Camera3D cam3D) cam3D.Current = false;

        foreach (Node cocuk in root.GetChildren())
        {
            TumKameralariKapat(cocuk);
        }
    }

    // --- Sadece gereken kamerayı aktif eder ---
private bool AktifKamerayiBulVeAc(Node dugum)
    {
        if (dugum == null) return false;

        if (dugum is Camera2D cam2D)
        {
            cam2D.Enabled = true;
            cam2D.MakeCurrent();
            cam2D.ResetSmoothing(); 
            
            // --- SİHİRLİ DOKUNUŞ ---
            // Godot'ya kameranın yerini hesaplamak için bir sonraki frame'i 
            // beklememesini, ANINDA hesaplamasını söyler!
            cam2D.ForceUpdateScroll(); 
            
            return true;
        }
        else if (dugum is Camera3D cam3D)
        {
            cam3D.Current = true;
            return true;
        }

        foreach (Node cocuk in dugum.GetChildren())
        {
            if (AktifKamerayiBulVeAc(cocuk)) return true;
        }

        return false;
    }
    private void StartNextTimer()
    {
        float beklemeSuresi = 20.0f; 

        if (_aktifOyun == "Platform") beklemeSuresi = PlatformDuration;
        else if (_aktifOyun == "Blackjack") beklemeSuresi = BlackjackDuration;
        else if (_aktifOyun == "Slots") beklemeSuresi = SlotsDuration;
        else if (_aktifOyun == "Araba") beklemeSuresi = ArabaDuration;
        else if (_aktifOyun == "Koridor") beklemeSuresi = KoridorDuration;

        _autoSwitchTimer.Start(beklemeSuresi);
    }

    private void OnAutoSwitchTimeout()
    {
        if (_isTransitioning) return;

        if (_aktifOyun == "Platform")
        {
            string yeniOyun = _minioyunListesi[_rng.RandiRange(0, _minioyunListesi.Length - 1)];

            string mesaj = "";
            if (yeniOyun == "Blackjack") mesaj = "MASAYA OTUR...";
            else if (yeniOyun == "Slots") mesaj = "ŞANSINI DENE...";
            else if (yeniOyun == "Araba") mesaj = "KAÇAMAZSIN... SÜR!";
            else if (yeniOyun == "Koridor") mesaj = "YÜRÜ... SADECE YÜRÜ.";

            SwitchGame(yeniOyun, mesaj);
        }
        else
        {
            SwitchGame("Platform", "ZAMAN DOLDU... GERİ DÖN!");
        }
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
        // --- KARANLIK EVRE: Ekranın tamamen glitch ile kaplı olduğu an ---
        tween.TweenCallback(Callable.From(() =>
        {
            OyunDurumunuAyarla(PlatformScene, false, false);
            OyunDurumunuAyarla(BlackjackScene, false, false);
            OyunDurumunuAyarla(SlotScene, false, false);
            OyunDurumunuAyarla(ArabaScene, false, false);
            OyunDurumunuAyarla(KoridorScene, false, false);

            _aktifOyun = targetGame;

            if (BGMPlayer != null)
            {
                if (targetGame == "Platform") BGMPlayer.Stream = PlatformMusic;
                else if (targetGame == "Blackjack") BGMPlayer.Stream = BlackjackMusic;
                else if (targetGame == "Slots") BGMPlayer.Stream = SlotsMusic;
                else if (targetGame == "Araba") BGMPlayer.Stream = ArabaMusic;
                else if (targetGame == "Koridor") BGMPlayer.Stream = KoridorMusic;

                BGMPlayer.Play();
            }

            Node yeniDugum = GetOyunDugumu(targetGame);
            if (yeniDugum != null) 
            {
                // Oyunu görünür yap ve (-50.000)'den geri (0,0)'a ışınla
                OyunDurumunuAyarla(yeniDugum, true, false); 
                
                // --- KAMERA DEĞİŞİMİNİ BURAYA, KARANLIĞA ALDIK ---
                TumKameralariKapat(this);
                AktifKamerayiBulVeAc(yeniDugum);
            }
        }));

        if (TextBackground != null) tween.TweenProperty(TextBackground, "modulate:a", 0.0f, 0.4f);
        if (TransitionLabel != null) tween.Parallel().TweenProperty(TransitionLabel, "modulate:a", 0.0f, 0.4f);

        if (mat != null)
        {
            tween.TweenProperty(mat, "shader_parameter/progress", 0.0f, 1.0f)
                 .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
        }

        // --- AYDINLIK EVRE: Ekran açılınca sadece zamanı / hareketi başlat ---
        tween.TweenCallback(Callable.From(() => 
        { 
            Node yeniDugum = GetOyunDugumu(targetGame);
            if (yeniDugum != null) 
            {
                yeniDugum.ProcessMode = ProcessModeEnum.Inherit;
            }
            
            _isTransitioning = false; 
            StartNextTimer();
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