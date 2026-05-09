using Godot;

public partial class Main : Node2D 
{
    [ExportCategory("Background Music")]
    [Export] public AudioStreamPlayer BGMPlayer;
    [Export] public AudioStream BlackjackMusic;
    [Export] public AudioStream SlotsMusic;
    [Export] public AudioStream ArabaMusic;   // Eklendi
    [Export] public AudioStream KoridorMusic; // Eklendi

    [ExportCategory("Game Scenes")]
    [Export] public Node BlackjackScene; 
    [Export] public Node SlotScene;
    [Export] public Node ArabaScene;
    [Export] public Node KoridorScene;

    [ExportCategory("Transition UI")]
    [Export] public ColorRect TransitionRect; 
    [Export] public ColorRect TextBackground; 
    [Export] public Label TransitionLabel;

    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private Timer _autoSwitchTimer;
    private bool _isTransitioning = false; 

    private string[] _oyunListesi = { "Blackjack", "Slots", "Araba", "Koridor" };
    private string _aktifOyun = "Blackjack";

    public override void _Ready()
    {
        OyunDurumunuAyarla(BlackjackScene, true, true);
        OyunDurumunuAyarla(SlotScene, false, false);
        OyunDurumunuAyarla(ArabaScene, false, false);
        OyunDurumunuAyarla(KoridorScene, false, false);

        if (TransitionRect != null && TransitionRect.Material != null)
            TransitionRect.Material.Set("shader_parameter/progress", 0.0f);
        if (TextBackground != null) TextBackground.Modulate = new Color(0, 0, 0, 0); 
        if (TransitionLabel != null) TransitionLabel.Modulate = new Color(1, 1, 1, 0); 

        // --- YENİ: Başlangıç müziğini çal! ---
        if (BGMPlayer != null && BlackjackMusic != null)
        {
            BGMPlayer.Stream = BlackjackMusic;
            BGMPlayer.Play();
        }

        _rng.Randomize();
        _autoSwitchTimer = new Timer();
        _autoSwitchTimer.OneShot = true;
        _autoSwitchTimer.Timeout += OnAutoSwitchTimeout;
        AddChild(_autoSwitchTimer); 

        StartNextRandomTimer();
    }

    private void OyunDurumunuAyarla(Node oyunDugumu, bool gorunurMu, bool calissinMi)
    {
        if (oyunDugumu == null) return;
        
        GorunurlukAyarlaRecursive(oyunDugumu, gorunurMu);
        oyunDugumu.ProcessMode = calissinMi ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
    }

    private void GorunurlukAyarlaRecursive(Node dugum, bool gorunurMu)
    {
        if (dugum is CanvasItem canvasItem) 
        {
            canvasItem.Visible = gorunurMu;
        }
        else if (dugum is Node3D node3d) 
        {
            node3d.Visible = gorunurMu;
        }
        else
        {
            foreach (Node cocuk in dugum.GetChildren())
            {
                GorunurlukAyarlaRecursive(cocuk, gorunurMu);
            }
        }
    }

    private void StartNextRandomTimer()
    {
        float randomTime = _rng.RandfRange(15.0f, 35.0f);
        _autoSwitchTimer.Start(randomTime);
    }

    private void OnAutoSwitchTimeout()
    {
        if (_isTransitioning) return;

        string yeniOyun;
        do {
            yeniOyun = _oyunListesi[_rng.RandiRange(0, _oyunListesi.Length - 1)];
        } while (yeniOyun == _aktifOyun);

        string mesaj = "";
        if (yeniOyun == "Blackjack") mesaj = "MASAYA GERİ DÖN...";
        else if (yeniOyun == "Slots") mesaj = "ŞANSINI DENE...";
        else if (yeniOyun == "Araba") mesaj = "KAÇAMAZSIN... SÜR!";
        else if (yeniOyun == "Koridor") mesaj = "YÜRÜ... SADECE YÜRÜ.";

        SwitchGame(yeniOyun, mesaj);
    }

    public void SwitchGame(string targetGame, string message)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        if (_autoSwitchTimer != null) StartNextRandomTimer();
        if (TransitionLabel != null) TransitionLabel.Text = message;
        
        Tween tween = GetTree().CreateTween();
        ShaderMaterial mat = TransitionRect != null ? TransitionRect.Material as ShaderMaterial : null;

        // --- SİHİRLİ DOKUNUŞ 1: Geçiş başladığı salise şu anki oyunu DONDUR ---
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
            OyunDurumunuAyarla(BlackjackScene, false, false);
            OyunDurumunuAyarla(SlotScene, false, false);
            OyunDurumunuAyarla(ArabaScene, false, false);
            OyunDurumunuAyarla(KoridorScene, false, false);

            _aktifOyun = targetGame;

            // --- YENİ: MÜZİK DEĞİŞİMİ (Ekran karanlıkken müzik anında değişir) ---
            if (BGMPlayer != null)
            {
                if (targetGame == "Blackjack") BGMPlayer.Stream = BlackjackMusic;
                else if (targetGame == "Slots") BGMPlayer.Stream = SlotsMusic;
                else if (targetGame == "Araba") BGMPlayer.Stream = ArabaMusic;
                else if (targetGame == "Koridor") BGMPlayer.Stream = KoridorMusic;

                BGMPlayer.Play();
            }

            // --- SİHİRLİ DOKUNUŞ 2: Yeni oyunu GÖRÜNÜR yap ama BAŞLATMA (Hala donuk) ---
            Node yeniDugum = GetOyunDugumu(targetGame);
            if (yeniDugum != null) GorunurlukAyarlaRecursive(yeniDugum, true);
        }));

        if (TextBackground != null) tween.TweenProperty(TextBackground, "modulate:a", 0.0f, 0.4f);
        if (TransitionLabel != null) tween.Parallel().TweenProperty(TransitionLabel, "modulate:a", 0.0f, 0.4f);

        if (mat != null)
        {
            tween.TweenProperty(mat, "shader_parameter/progress", 0.0f, 1.0f)
                 .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
        }

        // --- SİHİRLİ DOKUNUŞ 3: Glitch tamamen bitip ekran aydınlanınca zamanı AKIT ---
        tween.TweenCallback(Callable.From(() => 
        { 
            Node yeniDugum = GetOyunDugumu(targetGame);
            if (yeniDugum != null) yeniDugum.ProcessMode = ProcessModeEnum.Inherit;
            
            _isTransitioning = false; 
        }));
    }

    private Node GetOyunDugumu(string isim)
    {
        if (isim == "Blackjack") return BlackjackScene;
        if (isim == "Slots") return SlotScene;
        if (isim == "Araba") return ArabaScene;
        if (isim == "Koridor") return KoridorScene;
        return null;
    }
}