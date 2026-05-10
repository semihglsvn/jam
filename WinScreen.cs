// --- OYUN KAZANILDIĞINDA ÇAĞIRILACAK FONKSİYON ---
	public void GameWin(string oyuncuAdi)
	{
		if (_isTransitioning) return;
		_isTransitioning = true;

		GD.Print($">>> ZİRVEYE ULAŞILDI! OYUNCU: {oyuncuAdi} <<<");

		if (KazanmaEkrani != null)
		{
			// WinScreen.cs içindeki EkraniGoster fonksiyonunu çağır ve içine oyuncu adını gönder!
			KazanmaEkrani.Call("EkraniGoster", oyuncuAdi);
		}
		else
		{
			GD.PrintErr("DİKKAT: KazanmaEkrani Main'e atanmamış!");
		}
	}
