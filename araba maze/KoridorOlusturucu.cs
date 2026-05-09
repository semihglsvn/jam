using Godot;

public partial class KoridorOlusturucu : Node3D
{
	[Export] public float Uzunluk = 300f; 
	[Export] public float Genislik = 3f;  
	[Export] public float Yukseklik = 4f; 

	public override void _Ready()
	{
		// --- 1. ZEMİN (Daha aşağıya çektik, artık betona gömülmek İMKANSIZ) ---
		StandardMaterial3D zeminMateryali = new StandardMaterial3D();
		zeminMateryali.AlbedoColor = new Color(0.1f, 0.1f, 0.1f); // Koyu gri

		CsgBox3D zemin = new CsgBox3D();
		zemin.Size = new Vector3(Genislik, 0.1f, Uzunluk);
		// Zemin Y pozisyonunu -1.1 yaptık ki karakter üstünde kalsın
		zemin.Position = new Vector3(0, -1.1f, -Uzunluk / 2f); 
		zemin.MaterialOverride = zeminMateryali;
		zemin.UseCollision = true; 
		AddChild(zemin);

		// --- 2. RENGARENK DUVAR PARÇALARI (Hız Hissi İçin) ---
		float parcaUzunlugu = 10f; // Her 10 metrede bir renk değişecek
		int parcaSayisi = (int)(Uzunluk / parcaUzunlugu);

		for (int i = 0; i < parcaSayisi; i++)
		{
			StandardMaterial3D renkliMateryal = new StandardMaterial3D();
			// Tamamen rastgele pavyon renkleri üretiyoruz :)
			renkliMateryal.AlbedoColor = new Color((float)GD.Randf(), (float)GD.Randf(), (float)GD.Randf());

			float zPozisyonu = -(i * parcaUzunlugu) - (parcaUzunlugu / 2f);

			// Sol Parça
			CsgBox3D solParca = new CsgBox3D();
			solParca.Size = new Vector3(0.5f, Yukseklik, parcaUzunlugu);
			solParca.Position = new Vector3(-Genislik / 2f, Yukseklik / 2f - 1f, zPozisyonu);
			solParca.MaterialOverride = renkliMateryal;
			solParca.UseCollision = true; 
			AddChild(solParca);

			// Sağ Parça
			CsgBox3D sagParca = new CsgBox3D();
			sagParca.Size = new Vector3(0.5f, Yukseklik, parcaUzunlugu);
			sagParca.Position = new Vector3(Genislik / 2f, Yukseklik / 2f - 1f, zPozisyonu);
			sagParca.MaterialOverride = renkliMateryal;
			sagParca.UseCollision = true;
			AddChild(sagParca);
		}

		// --- 3. ARKA DUVAR ---
		CsgBox3D arkaDuvar = new CsgBox3D();
		arkaDuvar.Size = new Vector3(Genislik + 1f, Yukseklik, 0.5f);
		arkaDuvar.Position = new Vector3(0, Yukseklik / 2f - 1f, 1f); 
		StandardMaterial3D arkaMateryal = new StandardMaterial3D();
		arkaMateryal.AlbedoColor = new Color(0, 0, 0); // Simsiyah
		arkaDuvar.MaterialOverride = arkaMateryal;
		arkaDuvar.UseCollision = true;
		AddChild(arkaDuvar);
	}
}
