using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ulu;
using Ozellik = OruntuAyar.Ozellik;
using Oge = OruntuAyar.Oge;

/// <summary>
/// Örüntü — dizideki kuralı çözüp eksik karoyu bulma oyunu.
/// Sınıf sürümü: bölüm seçme tahtası, öğretmen modu (tek özelliği izole eden alıştırma)
/// ve bölüm sonunda özellik bazlı doğruluk dökümü.
/// Sahnedeki tek nesnenin üstündeki tek script; her şeyi kod kurar.
/// </summary>
public class OruntuBootstrap : OyunTabani
{
    protected override string OyunAdi => "Örüntü";
    protected override string NasilOynanir => "Dizideki kuralı çöz, eksik karoyu bul.\nBaşlamak için tıkla, sonra bölüm seç.";
    protected override string SkorEtiketi => "Puan";
    protected override Color ArkaPlan => new Color(0.055f, 0.065f, 0.105f);
    protected override float KameraBoyu => 5f;

    static readonly Color[] Palet = { Renk.Turkuaz, Renk.Mavi, Renk.Mor, Renk.Pembe, Renk.Sari };
    static readonly float[] BoyutOlcek = { 0.52f, 0.76f, 1.0f };
    static readonly Ozellik[] TumOzellikler =
        { Ozellik.Sekil, Ozellik.Renk, Ozellik.Boyut, Ozellik.Donme, Ozellik.Adet };
    static readonly string[] OzellikAdi = { "şekil", "renk", "boyut", "dönme", "adet" };

    const float DiziY = 1.7f;
    const float SecenekY = -2.5f;
    const float KaroAralik = 1.85f;
    const float SecenekAralik = 2.4f;
    const float KaroYari = 0.85f;

    enum Asama { BolumSec, Soru, Bekle }

    // --- durum ---
    Asama asama;
    int bolum, soruNo, can, bolumHatasi;
    Ozellik? odak;              // öğretmen modu: yalnızca bu özellik değişir
    float bekleme;
    bool sonrakiSecime;         // bekleme bitince seçim tahtasına dön

    Dictionary<Ozellik, OruntuAyar.Kural> kurallar;
    Ozellik[] aktif;
    Oge[] dizi;
    int gizliIndex;
    Oge[] secenekler;
    int dogruSecenek;
    SpriteRenderer[] secenekArka;

    // --- ölçme ---
    readonly int[] bolumDogru = new int[5];
    readonly int[] bolumSoru = new int[5];
    readonly int[] oturumDogru = new int[5];
    readonly int[] oturumSoru = new int[5];

    Transform kok;
    Text ustBilgi, altBilgi;
    readonly List<(Text yazi, Vector3 dunya)> dunyaYazilari = new List<(Text, Vector3)>();
    readonly List<(Vector3 merkez, Vector2 yari, int tur, int deger)> hedefler =
        new List<(Vector3, Vector2, int, int)>();

    protected override void Kur()
    {
        ustBilgi = Arayuz.Yazi(Ekran.transform, "", 26, Kose.Ust, new Vector2(0f, 56f), Renk.Gri, new Vector2(1240f, 70f));
        altBilgi = Arayuz.Yazi(Ekran.transform, "", 24, Kose.Alt, new Vector2(0f, 30f), Renk.Gri, new Vector2(1240f, 60f));
    }

    protected override void Basla()
    {
        System.Array.Clear(oturumDogru, 0, oturumDogru.Length);
        System.Array.Clear(oturumSoru, 0, oturumSoru.Length);
        odak = null;
        SecimeDon();
    }

    protected override void Oyna(float dt)
    {
        KonumlariTazele();

        switch (asama)
        {
            case Asama.BolumSec:
                SecimGirdisi();
                break;

            case Asama.Soru:
                CevapGirdisi();
                break;

            case Asama.Bekle:
                bekleme -= dt;
                if (bekleme <= 0f) Devam();
                break;
        }
    }

    // ================= BÖLÜM SEÇME TAHTASI =================

    void SecimeDon()
    {
        asama = Asama.BolumSec;
        odak = null;
        TahtaCiz();
    }

    void TahtaCiz()
    {
        KokuYenile();
        int acilan = Mathf.Clamp(Kayit.Bolum(OyunAdi), 1, OruntuAyar.ToplamBolum);

        for (int i = 0; i < OruntuAyar.ToplamBolum; i++)
        {
            int no = i + 1;
            int satir = i / 6, kolon = i % 6;
            var merkez = new Vector3(-4.5f + kolon * 1.8f, satir == 0 ? 1.9f : 0.1f, 0f);
            bool acik = no <= acilan;
            int yildiz = Kayit.Sayi(OyunAdi, "yildiz" + no);

            var arkaRenk = acik
                ? (no == acilan ? new Color(0.18f, 0.30f, 0.34f) : new Color(0.16f, 0.19f, 0.27f))
                : new Color(0.10f, 0.11f, 0.15f);
            Nesne("BolumKaro", Cizim.YuvarlakKare(arkaRenk, 64, 64, 0.26f), merkez, 1.55f, 0, kok);

            YaziEkle(no.ToString(), merkez + new Vector3(0f, 0.18f, 0f), 30,
                     acik ? Renk.Beyaz : new Color(0.35f, 0.37f, 0.45f));

            for (int y = 0; y < 3; y++)
            {
                var renk = y < yildiz ? Renk.Sari : new Color(0.24f, 0.26f, 0.33f);
                Nesne("Yildiz", Cizim.Yildiz(renk, 24), merkez + new Vector3((y - 1) * 0.28f, -0.42f, 0f), 0.2f, 1, kok);
            }

            if (acik) hedefler.Add((merkez, new Vector2(0.78f, 0.78f), 0, no));
        }

        // Öğretmen modu: tek özelliği izole eden alıştırma düğmeleri.
        for (int i = 0; i < TumOzellikler.Length; i++)
        {
            var merkez = new Vector3(-4.0f + i * 2.0f, -2.4f, 0f);
            Nesne("ModKaro", Cizim.YuvarlakKare(new Color(0.14f, 0.16f, 0.23f), 96, 48, 0.4f), merkez, 1.15f, 0, kok);
            YaziEkle(OzellikAdi[(int)TumOzellikler[i]], merkez, 22, Renk.Turkuaz);
            hedefler.Add((merkez, new Vector2(0.86f, 0.44f), 1, (int)TumOzellikler[i]));
        }

        ustBilgi.color = Renk.Gri;
        ustBilgi.text = $"Bölüm seç   ·   açılan bölüm: {acilan}/{OruntuAyar.ToplamBolum}   ·   {Dokum(oturumDogru, oturumSoru, "Bu oturum")}";
        altBilgi.color = Renk.Gri;
        altBilgi.text = "Alt sıra: öğretmen modu — yalnızca o özellik değişir (tek başına çalıştırmak için)";
    }

    void SecimGirdisi()
    {
        for (int i = 1; i <= 9; i++)
            if (Girdi.SayiBasildi(i) && i <= Kayit.Bolum(OyunAdi)) { BolumBaslat(i, null); return; }

        if (!Girdi.Onayla) return;

        var d = Girdi.DunyaNoktasi(Kam);
        foreach (var h in hedefler)
        {
            if (Mathf.Abs(d.x - h.merkez.x) >= h.yari.x || Mathf.Abs(d.y - h.merkez.y) >= h.yari.y) continue;

            if (h.tur == 0) BolumBaslat(h.deger, null);
            else BolumBaslat(Mathf.Clamp(Kayit.Bolum(OyunAdi), 1, OruntuAyar.ToplamBolum), (Ozellik)h.deger);
            return;
        }
    }

    // ================= BÖLÜM AKIŞI =================

    void BolumBaslat(int no, Ozellik? odakOzellik)
    {
        Girdi.Tuket();
        bolum = Mathf.Clamp(no, 1, OruntuAyar.ToplamBolum);
        odak = odakOzellik;
        can = OruntuAyar.Can;
        soruNo = 0;
        bolumHatasi = 0;
        System.Array.Clear(bolumDogru, 0, bolumDogru.Length);
        System.Array.Clear(bolumSoru, 0, bolumSoru.Length);
        Ses.Basla();
        SoruUret();
    }

    void SoruUret()
    {
        asama = Asama.Soru;
        var ayar = OruntuAyar.Bolum(bolum);
        List<Oge> liste = null;
        Oge dogru = default;

        // Emniyet: seçeneklerden ikisi ekranda aynı görünüyorsa ya da dizi hiç değişmiyorsa
        // soru çözülemez demektir — yeniden üret.
        for (int deneme = 0; deneme < 12; deneme++)
        {
            aktif = odak.HasValue ? new[] { odak.Value } : OruntuAyar.AktifOzellikler(bolum);
            kurallar = OruntuAyar.KurallariUret(aktif);

            dizi = new Oge[ayar.diziUzunlugu];
            for (int i = 0; i < dizi.Length; i++) dizi[i] = OruntuAyar.OgeUret(kurallar, i);

            gizliIndex = ayar.ortadanSor && !odak.HasValue ? Random.Range(1, dizi.Length - 1) : dizi.Length - 1;

            dogru = dizi[gizliIndex];
            var onceki = dizi[Mathf.Max(0, gizliIndex - 1)];
            var celdirici = OruntuAyar.Celdiriciler(dogru, onceki, aktif, ayar.inceCeldirici, kurallar);

            liste = new List<Oge>(celdirici) { dogru };

            bool diziDegisiyor = !OruntuAyar.AyirtEdilemezVar(new[] { dizi[0], dizi[1] });
            if (!OruntuAyar.AyirtEdilemezVar(liste) && diziDegisiyor) break;
        }

        for (int i = liste.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (liste[i], liste[j]) = (liste[j], liste[i]);
        }
        secenekler = liste.ToArray();
        dogruSecenek = System.Array.FindIndex(secenekler, s => s.Ayni(dogru));

        SoruCiz();
        BilgiTazele();
        altBilgi.color = Renk.Gri;
        altBilgi.text = "Kuralı bul: dizideki değişim nasıl ilerliyor?";
    }

    void CevapGirdisi()
    {
        for (int i = 0; i < secenekler.Length; i++)
            if (Girdi.SayiBasildi(i + 1)) { Cevapla(i); return; }

        if (!Girdi.Onayla) return;

        var d = Girdi.DunyaNoktasi(Kam);
        for (int i = 0; i < secenekler.Length; i++)
        {
            var merkez = SecenekKonum(i);
            if (Mathf.Abs(d.x - merkez.x) < KaroYari && Mathf.Abs(d.y - merkez.y) < KaroYari)
            {
                Cevapla(i);
                return;
            }
        }
    }

    void Cevapla(int secim)
    {
        Girdi.Tuket();
        bool dogru = secim == dogruSecenek;

        // Ölçme: bu soruda çalışan her özellik için doğruluk sayacı.
        foreach (var o in aktif)
        {
            bolumSoru[(int)o]++;
            oturumSoru[(int)o]++;
            if (dogru) { bolumDogru[(int)o]++; oturumDogru[(int)o]++; }
        }

        if (dogru)
        {
            secenekArka[secim].color = new Color(0.25f, 0.65f, 0.40f);
            PuanEkle(10 + bolum * 2);
            Ses.Puan();
            altBilgi.color = Renk.Yesil;
            altBilgi.text = "Doğru — " + OruntuAyar.KuralAciklamasi(kurallar, aktif);
            bekleme = 1.1f;
        }
        else
        {
            secenekArka[secim].color = new Color(0.62f, 0.24f, 0.28f);
            secenekArka[dogruSecenek].color = new Color(0.25f, 0.65f, 0.40f);
            can--;
            bolumHatasi++;
            Ses.Carpma();
            altBilgi.color = Renk.Turuncu;
            altBilgi.text = "Kural: " + OruntuAyar.KuralAciklamasi(kurallar, aktif);
            bekleme = 2.6f;
        }

        // Eksik karonun yerine doğrusunu koy (katman 4: boş çerçevenin üstünde kalsın).
        KaroCiz(dizi[gizliIndex], DiziKonum(gizliIndex), kok,
                dogru ? Renk.Duman : new Color(0.28f, 0.22f, 0.26f), 4);

        BilgiTazele();
        asama = Asama.Bekle;
        sonrakiSecime = false;
        if (can <= 0) bekleme = Mathf.Max(bekleme, 2.6f);
    }

    void Devam()
    {
        if (sonrakiSecime) { SecimeDon(); return; }

        if (can <= 0)
        {
            BolumSonu(false);
            return;
        }

        soruNo++;
        if (soruNo >= OruntuAyar.SoruSayisi)
        {
            BolumSonu(true);
            return;
        }
        SoruUret();
    }

    void BolumSonu(bool basarili)
    {
        int yildiz = bolumHatasi == 0 ? 3 : bolumHatasi == 1 ? 2 : 1;

        if (basarili && !odak.HasValue)
        {
            string anahtar = "yildiz" + bolum;
            if (yildiz > Kayit.Sayi(OyunAdi, anahtar)) Kayit.SayiYaz(OyunAdi, anahtar, yildiz);
            Kayit.BolumYaz(OyunAdi, bolum + 1);
            PuanEkle(40);
        }

        string bas = odak.HasValue
            ? $"{OzellikAdi[(int)odak.Value]} alıştırması bitti"
            : basarili ? $"Bölüm {bolum} tamam — {yildiz} yıldız" : $"Bölüm {bolum} bitti — canlar tükendi";

        ustBilgi.color = basarili ? Renk.Sari : Renk.Turuncu;
        ustBilgi.text = bas + "   ·   " + Dokum(bolumDogru, bolumSoru, "Bu bölüm");
        altBilgi.color = Renk.Gri;
        altBilgi.text = "Bölüm seçme tahtasına dönülüyor…";

        if (basarili) Ses.Puan(); else Ses.Bitis();

        // Son bölüm de bitmişse oyunu kutlamayla kapat, yoksa seçime dön.
        if (basarili && !odak.HasValue && bolum >= OruntuAyar.ToplamBolum)
        {
            Bitir("Tüm bölümler tamam!");
            return;
        }

        asama = Asama.Bekle;
        sonrakiSecime = true;
        bekleme = 2.8f;
    }

    void BilgiTazele()
    {
        int gosterilenSoru = Mathf.Clamp(soruNo + 1, 1, OruntuAyar.SoruSayisi);
        string baslik = odak.HasValue
            ? $"Öğretmen modu: {OzellikAdi[(int)odak.Value]}"
            : $"Bölüm {bolum}/{OruntuAyar.ToplamBolum}";
        ustBilgi.color = Renk.Gri;
        ustBilgi.text = $"{baslik}   ·   Soru {gosterilenSoru}/{OruntuAyar.SoruSayisi}   ·   Can {Mathf.Max(0, can)}";
    }

    /// <summary>"dönme 2/3 · renk 5/5" biçiminde özellik bazlı doğruluk dökümü.</summary>
    string Dokum(int[] dogru, int[] soru, string etiket)
    {
        var parcalar = new List<string>();
        foreach (var o in TumOzellikler)
        {
            int i = (int)o;
            if (soru[i] == 0) continue;
            parcalar.Add($"{OzellikAdi[i]} {dogru[i]}/{soru[i]}");
        }
        return parcalar.Count == 0 ? $"{etiket}: —" : $"{etiket}: " + string.Join("  ", parcalar);
    }

    // ================= ÇİZİM =================

    void KokuYenile()
    {
        if (kok != null) Destroy(kok.gameObject);
        kok = new GameObject("Ekran").transform;
        kok.SetParent(Alan, false);

        foreach (var (yazi, _) in dunyaYazilari)
            if (yazi != null) Destroy(yazi.gameObject);
        dunyaYazilari.Clear();
        hedefler.Clear();
    }

    void YaziEkle(string metin, Vector3 dunya, int punto, Color renk)
    {
        var yazi = Arayuz.Yazi(Ekran.transform, metin, punto, Kose.Orta, Vector2.zero, renk, new Vector2(220f, 90f));
        dunyaYazilari.Add((yazi, dunya));
        yazi.rectTransform.position = Kam.WorldToScreenPoint(dunya);
    }

    /// <summary>Dünya konumuna bağlı yazıları ekran koordinatına taşır (pencere boyu değişirse de doğru kalsın).</summary>
    void KonumlariTazele()
    {
        foreach (var (yazi, dunya) in dunyaYazilari)
            if (yazi != null) yazi.rectTransform.position = Kam.WorldToScreenPoint(dunya);
    }

    void SoruCiz()
    {
        KokuYenile();

        for (int i = 0; i < dizi.Length; i++)
        {
            var konum = DiziKonum(i);
            if (i == gizliIndex)
            {
                Nesne("BosCerceve", Cizim.YuvarlakKare(Renk.Turkuaz, 64, 64, 0.24f), konum, 1.6f, 0, kok);
                Nesne("BosIc", Cizim.YuvarlakKare(ArkaPlan, 64, 64, 0.24f), konum, 1.44f, 1, kok);
                YaziEkle("?", konum, 60, Renk.Turkuaz);
            }
            else
            {
                KaroCiz(dizi[i], konum, kok, Renk.Duman);
            }
        }

        secenekArka = new SpriteRenderer[secenekler.Length];
        for (int i = 0; i < secenekler.Length; i++)
        {
            var merkez = SecenekKonum(i);
            secenekArka[i] = KaroCiz(secenekler[i], merkez, kok, new Color(0.14f, 0.16f, 0.23f));
            YaziEkle((i + 1).ToString(), merkez + new Vector3(0f, -1.05f, 0f), 20, Renk.Gri);
        }
    }

    Vector3 DiziKonum(int i)
    {
        float toplam = (dizi.Length - 1) * KaroAralik;
        return new Vector3(-toplam * 0.5f + i * KaroAralik, DiziY, 0f);
    }

    Vector3 SecenekKonum(int i)
    {
        float toplam = (secenekler.Length - 1) * SecenekAralik;
        return new Vector3(-toplam * 0.5f + i * SecenekAralik, SecenekY, 0f);
    }

    SpriteRenderer KaroCiz(Oge o, Vector3 merkez, Transform ebeveyn, Color arkaRenk, int katman = 0)
    {
        var arka = Nesne("Karo", Cizim.YuvarlakKare(arkaRenk, 64, 64, 0.24f), merkez, 1.6f, katman, ebeveyn);

        // Karo 1.6 birim geniş (yarısı 0.8). En dıştaki şeklin merkezi + yarı genişliği
        // 0.72'yi geçmemeli, yoksa şekiller karonun dışına taşar.
        int adet = o.adet + 1;
        float icOlcek = adet == 1 ? 1f : adet == 2 ? 0.62f : 0.44f;
        float boyut = BoyutOlcek[Mathf.Clamp(o.boyut, 0, BoyutOlcek.Length - 1)] * icOlcek;
        float aralik = adet == 1 ? 0f : adet == 2 ? 0.68f : 0.47f;   // komşu merkezler arası

        for (int i = 0; i < adet; i++)
        {
            float kayma = adet == 1 ? 0f : (i - (adet - 1) * 0.5f) * aralik;
            var sr = Nesne("Sekil", SekilSprite(o.sekil, Palet[Mathf.Clamp(o.renk, 0, Palet.Length - 1)]),
                           merkez + new Vector3(kayma, 0f, 0f), boyut, katman + 2, ebeveyn);
            sr.transform.rotation = Quaternion.Euler(0f, 0f, o.donme * 45f);
        }

        return arka;
    }

    static Sprite SekilSprite(int sekil, Color renk) => sekil switch
    {
        0 => Cizim.Daire(renk, 64),
        1 => Cizim.YuvarlakKare(renk, 64, 64, 0.18f),
        2 => Cizim.Ucgen(renk, 64),
        3 => Cizim.Yildiz(renk, 64),
        _ => Cizim.Halka(renk, 64, 0.34f)
    };
}
