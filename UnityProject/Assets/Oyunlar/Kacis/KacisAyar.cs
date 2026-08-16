using UnityEngine;

/// <summary>
/// Kaçış'ın zorluk eğrisi. Saf matematik — oyun hissini ayarlamak için tek yer burası.
/// </summary>
public static class KacisAyar
{
    public const float BaslangicHizi = 5.5f;
    public const float EnYuksekHiz = 15f;
    public const float HizArtisi = 0.30f;      // saniyede

    public const float IlkAralikMesafesi = 5.0f;   // dünya birimi
    public const float SonAralikMesafesi = 2.9f;
    public const float MesafeSikismaSuresi = 60f;  // bu sürede en sık hâle gelir

    public static float Hiz(float gecenSure)
        => Mathf.Min(EnYuksekHiz, BaslangicHizi + gecenSure * HizArtisi);

    /// <summary>İki engel arasındaki dikey mesafe (dünya birimi).</summary>
    public static float Mesafe(float gecenSure)
        => Mathf.Lerp(IlkAralikMesafesi, SonAralikMesafesi,
                      Mathf.Clamp01(gecenSure / MesafeSikismaSuresi));

    /// <summary>İki engel arasındaki süre; hız arttıkça kısalır ama tepki süresi korunur.</summary>
    public static float Aralik(float gecenSure)
        => Mesafe(gecenSure) / Hiz(gecenSure);

    /// <summary>Aynı anda iki şeridi kapatma olasılığı (0..0.55).</summary>
    public static float CiftEngelSansi(float gecenSure)
        => Mathf.Clamp(  (gecenSure - 15f) / 60f, 0f, 0.55f);
}
