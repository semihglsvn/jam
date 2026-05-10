using Godot;
using System;

public partial class Main : Node2D 
{
    [ExportCategory("Audio")]
    [Export] public AudioStreamPlayer BGMPlayer;
    [Export] public AudioStreamPlayer TransitionSFXPlayer; // The megaphone for glitches
    [Export] public AudioStream TransitionSound;          // The glitch sound file

    [ExportCategory("Background Music")]
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
    [Export] public float PlatformDuration = 50.0f;
    [Export] public float BlackjackDuration = 5.0f;
    [Export] public float SlotsDuration = 5.0f;
    [Export] public float ArabaDuration = 40.0f;
    [Export] public float KoridorDuration = 5.0f;

    [ExportCategory("Narrative Messages (Melancholic)")]
    [Export] public string[] BlackjackMessages = { 
        "The house always wins...", 
        "Numbers are the only truth left.", 
        "Bet your soul. It’s worth nothing anyway." 
    };
    [Export] public string[] SlotsMessages = { 
        "Give up and just spin.", 
        "Drown in the flashing lights.", 
        "Luck is a lie told to the desperate." 
    };
    [Export] public string[] ArabaMessages = { 
        "The road leads nowhere.", 
        "Fuel is temporary. The dark is forever.", 
        "No one is waiting at the end." 
    };
    [Export] public string[] KoridorMessages = { 
        "Just stop walking. It’s easier.", 
        "This hallway is the only world left.", 
        "Why keep going? There is no exit." 
    };

    [ExportCategory("Narrative Messages (Hopeful)")]
    [Export] public string[] PlatformerMessages = { 
        "Keep climbing, the light is still there!", 
        "Almost home...", 
        "Don't let them take your sky." 
    };

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
        
        TumKameralariKapat(this);
        AktifKamerayiBulVeAc(PlatformScene);
    }

    private string GetRandomMessage(string targetGame)
    {
        string[] pool = targetGame switch {
            "Blackjack" => BlackjackMessages, "Slots" => SlotsMessages,
            "Araba" => ArabaMessages, "Koridor" => KoridorMessages,
            "Platform" => PlatformerMessages, _ => null
        };
        return (pool == null || pool.Length == 0) ? "..." : pool[_rng.RandiRange(0, pool.Length - 1)];
    }

    private void OyunDurumunuAyarla(Node oyunDugumu, bool gorunurMu, bool calissinMi)
    {
        if (oyunDugumu == null) return;
        
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

        if (oyunDugumu is Node2D n2d)
        {
            n2d.Position = gorunurMu ? Vector2.Zero : new Vector2(-50000, -50000);
        }

        oyunDugumu.ProcessMode = calissinMi ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
    }

    private void TumKameralariKapat(Node root)
    {
        if (root == null) return;
        if (root is Camera2D cam2D) cam2D.Enabled = false;
        else if (root is Camera3D cam3D) cam3D.Current = false;

        foreach (Node cocuk in root.GetChildren())
        {
            TumKameralariKapat(cocuk);
        }
    }

    private bool AktifKamerayiBulVeAc(Node dugum)
    {
        if (dugum == null) return false;

        if (dugum is Camera2D cam2D)
        {
            cam2D.Enabled = true;
            cam2D.MakeCurrent();
            cam2D.ResetSmoothing(); 
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
        float beklemeSuresi = _aktifOyun switch {
            "Platform" => PlatformDuration, "Blackjack" => BlackjackDuration,
            "Slots" => SlotsDuration, "Araba" => ArabaDuration,
            "Koridor" => KoridorDuration, _ => 5.0f
        };
        _autoSwitchTimer.Start(beklemeSuresi);
    }

    private void OnAutoSwitchTimeout()
    {
        if (_isTransitioning) return;
        if (_aktifOyun == "Platform") {
            string next = _minioyunListesi[_rng.RandiRange(0, _minioyunListesi.Length - 1)];
            SwitchGame(next, GetRandomMessage(next));
        } else SwitchGame("Platform", GetRandomMessage("Platform"));
    }

    public void MinioyunKazanildi()
    {
        if (!_isTransitioning && _aktifOyun != "Platform") 
            SwitchGame("Platform", GetRandomMessage("Platform"));
    }

    public void SwitchGame(string targetGame, string message)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        if (TransitionLabel != null) TransitionLabel.Text = message;
        
        // --- TRANSITION SOUND LOGIC ---
        if (TransitionSFXPlayer != null && TransitionSound != null)
        {
            TransitionSFXPlayer.Stream = TransitionSound;
            TransitionSFXPlayer.Play();
        }

        Tween tween = GetTree().CreateTween();
        ShaderMaterial mat = TransitionRect?.Material as ShaderMaterial;

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

            if (BGMPlayer != null)
            {
                BGMPlayer.Stream = targetGame switch {
                    "Platform" => PlatformMusic, "Blackjack" => BlackjackMusic,
                    "Slots" => SlotsMusic, "Araba" => ArabaMusic,
                    "Koridor" => KoridorMusic, _ => BGMPlayer.Stream
                };
                BGMPlayer.Play();
            }

            Node yeniDugum = GetOyunDugumu(targetGame);
            if (yeniDugum != null) 
            {
                OyunDurumunuAyarla(yeniDugum, true, false); 
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

        tween.TweenCallback(Callable.From(() => 
        { 
            Node yeniDugum = GetOyunDugumu(targetGame);
            if (yeniDugum != null) yeniDugum.ProcessMode = ProcessModeEnum.Inherit;
            _isTransitioning = false; 
            StartNextTimer();
        }));
    }

    private Node GetOyunDugumu(string isim) => isim switch {
        "Platform" => PlatformScene, "Blackjack" => BlackjackScene,
        "Slots" => SlotScene, "Araba" => ArabaScene,
        "Koridor" => KoridorScene, _ => null
    };
}