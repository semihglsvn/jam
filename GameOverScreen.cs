using Godot;
using System;

public partial class GameOverScreen : CanvasLayer
{
	// Düğüm ağacındaki Button'u buraya sürükleyeceğiz
	[Export] public Button RestartButton;

	public override void _Ready()
	{
		this.Hide(); // Oyun başladığında bu ekran gizli dursun
		
		// ÇOK ÖNEMLİ: Oyun donduğunda bile bu ekranın ve butonun çalışmaya devam etmesi için!
		this.ProcessMode = ProcessModeEnum.Always; 

		if (RestartButton != null)
		{
			RestartButton.Pressed += OnRestartPressed;
		}
	}

	// Main.cs bu fonksiyonu çağıracak!
	public void EkraniGoster()
	{
		this.Show(); // Siyah glitchli ekranı göster
		GetTree().Paused = true; // Arkadaki oyunu tamamen dondur
	}

	private void OnRestartPressed()
	{
		GetTree().Paused = false; // Oyunu donukluktan çıkar
		GetTree().ReloadCurrentScene(); // Her şeyi sıfırla ve baştan başlat
	}
}
