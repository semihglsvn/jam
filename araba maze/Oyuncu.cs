using Godot;

public partial class Oyuncu : CharacterBody3D
{
	[Export] public float YurumeHizi = 5.0f;
	
	// UI Label'ını nerede yaratırsan yarat, Inspector'dan buraya sürükleyeceksin
	[Export] public Label SayacLabel; 

	private float _skor = 0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector3 yeniHiz = Velocity;
		bool yuruyorMu = false;

		// Godot 3D dünyasında ileri yön "-Z" eksenidir
		if (Input.IsActionPressed("ui_up"))
		{
			yeniHiz.Z = -YurumeHizi; 
			yuruyorMu = true;
		}
		else if (Input.IsActionPressed("ui_down"))
		{
			yeniHiz.Z = YurumeHizi;
			yuruyorMu = true;
		}
		else
		{
			yeniHiz.Z = 0; // Tuş bırakıldığında anında dursun
		}

		// Hızı karaktere uygula ve duvarlara çarpmayı hesapla
		Velocity = yeniHiz;
		MoveAndSlide();

		// --- SAYAÇ MEKANİĞİ ---
		// Sadece hareket ediyorsak skoru artır
		if (yuruyorMu)
		{
			// Saniyede 10 puan artar
			_skor += (float)delta * 10f; 

			if (SayacLabel != null)
			{
				// Mathf.FloorToInt ile küsuratları atıp tam sayı gösteriyoruz
				SayacLabel.Text = "Skor: " + Mathf.FloorToInt(_skor).ToString();
			}
		}
	}
}
