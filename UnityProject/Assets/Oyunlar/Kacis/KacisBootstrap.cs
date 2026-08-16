using System.Collections.Generic;
using UnityEngine;
using Ulu;

/// <summary>
/// Kaçış — 3 şeritli sonsuz kaçış. Sahnedeki tek nesnenin üzerindeki tek script;
/// kamera, şeritler, oyuncu, engeller ve arayüz koddan kurulur.
/// </summary>
public class KacisBootstrap : OyunTabani
{
    protected override string OyunAdi => "Kaçış";
    protected override string NasilOynanir => "Şerit değiştir, engellere çarpma.  A/D veya ← →  ·  telefonda ekranın soluna/sağına dokun";
    protected override Color ArkaPlan => Renk.Gece;
    protected override float KameraBoyu => 5f;

    static readonly float[] SeritX = { -2f, 0f, 2f };
    const float OyuncuY = -3.2f;
    const float UretimY = 6.2f;
    const float SilmeY = -6.5f;

    // Çarpışma kutuları (yarı genişlik/yükseklik, dünya birimi)
    static readonly Vector2 OyuncuKutu = new Vector2(0.38f, 0.38f);
    static readonly Vector2 EngelKutu = new Vector2(0.72f, 0.34f);
    static readonly Vector2 YildizKutu = new Vector2(0.30f, 0.30f);

    class Dusen
    {
        public Transform gecis;
        public bool yildiz;
        public bool sayildi;
    }

    readonly List<Dusen> dusenler = new List<Dusen>();
    readonly List<Transform> cizgiler = new List<Transform>();

    SpriteRenderer oyuncu;
    int serit;
    float gecenSure;
    float sonrakiUretim;

    protected override void Kur()
    {
        // Yol zemini ve kenarlar — turlar arasında kalır.
        Nesne("Yol", Cizim.Dikdortgen(new Color(0.11f, 0.13f, 0.20f), 64, 64),
              Vector3.zero, 1f, -20, Dekor).transform.localScale = new Vector3(6.4f, 12f, 1f);

        foreach (float x in new[] { -3.2f, 3.2f })
        {
            var kenar = Nesne("Kenar", Cizim.Dikdortgen(Renk.Duman, 64, 64), new Vector3(x, 0f, 0f), 1f, -19, Dekor);
            kenar.transform.localScale = new Vector3(0.18f, 12f, 1f);
        }

        // Şerit ayırıcı kesikli çizgiler — oynarken aşağı kayar, hız hissini verir.
        for (int i = 0; i < 2; i++)
        {
            float x = (SeritX[i] + SeritX[i + 1]) * 0.5f;
            for (int j = 0; j < 12; j++)
            {
                var c = Nesne("Cizgi", Cizim.Dikdortgen(new Color(0.30f, 0.34f, 0.45f), 64, 64),
                              new Vector3(x, -6f + j * 1.2f, 0f), 1f, -18, Dekor);
                c.transform.localScale = new Vector3(0.06f, 0.6f, 1f);
                cizgiler.Add(c.transform);
            }
        }
    }

    protected override void Basla()
    {
        dusenler.Clear();
        gecenSure = 0f;
        serit = 1;
        sonrakiUretim = 0.6f;

        oyuncu = Nesne("Oyuncu", Cizim.YuvarlakKare(Renk.Turkuaz, 64, 64, 0.35f),
                       new Vector3(SeritX[serit], OyuncuY, 0f), 0.8f, 5);
    }

    protected override void Oyna(float dt)
    {
        gecenSure += dt;
        float hiz = KacisAyar.Hiz(gecenSure);

        SeritDegistir();
        OyuncuyuKaydir(dt);
        CizgileriKaydir(hiz, dt);
        DusenleriIsle(hiz, dt);

        sonrakiUretim -= dt;
        if (sonrakiUretim <= 0f)
        {
            Uret();
            sonrakiUretim = KacisAyar.Aralik(gecenSure);
        }
    }

    void SeritDegistir()
    {
        int onceki = serit;
        if (Girdi.SolaBasildi) serit = Mathf.Max(0, serit - 1);
        else if (Girdi.SagaBasildi) serit = Mathf.Min(SeritX.Length - 1, serit + 1);
        if (serit != onceki) Ses.Tik();
    }

    void OyuncuyuKaydir(float dt)
    {
        var p = oyuncu.transform.position;
        p.x = Mathf.MoveTowards(p.x, SeritX[serit], 18f * dt);
        // Şerit değiştirirken hafif yatma — mekaniği değiştirmez, his katar.
        oyuncu.transform.position = p;
        float egim = (SeritX[serit] - p.x) * 12f;
        oyuncu.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(egim, -18f, 18f));
    }

    void CizgileriKaydir(float hiz, float dt)
    {
        foreach (var c in cizgiler)
        {
            var p = c.position;
            p.y -= hiz * dt;
            if (p.y < -6f) p.y += 14.4f;
            c.position = p;
        }
    }

    void DusenleriIsle(float hiz, float dt)
    {
        var oyuncuP = oyuncu.transform.position;

        for (int i = dusenler.Count - 1; i >= 0; i--)
        {
            var d = dusenler[i];
            if (d.gecis == null) { dusenler.RemoveAt(i); continue; }

            var p = d.gecis.position;
            p.y -= hiz * dt;
            d.gecis.position = p;

            if (d.yildiz) d.gecis.Rotate(0f, 0f, 90f * dt);

            var kutu = d.yildiz ? YildizKutu : EngelKutu;
            if (Carpisti(oyuncuP, OyuncuKutu, p, kutu))
            {
                if (d.yildiz)
                {
                    PuanEkle(5);
                    Ses.Puan();
                    Destroy(d.gecis.gameObject);
                    dusenler.RemoveAt(i);
                    continue;
                }

                Ses.Carpma();
                Bitir("Çarptın!");
                return;
            }

            if (!d.sayildi && !d.yildiz && p.y < OyuncuY - 0.6f)
            {
                d.sayildi = true;
                PuanEkle(1);
            }

            if (p.y < SilmeY)
            {
                Destroy(d.gecis.gameObject);
                dusenler.RemoveAt(i);
            }
        }
    }

    void Uret()
    {
        int bos = Random.Range(0, SeritX.Length);
        bool cift = Random.value < KacisAyar.CiftEngelSansi(gecenSure);
        int tekSerit = TekEngelSeridi(bos);

        for (int s = 0; s < SeritX.Length; s++)
        {
            if (s == bos) continue;
            if (!cift && s != tekSerit) continue;

            var e = Nesne("Engel", Cizim.YuvarlakKare(Renk.Turuncu, 96, 48, 0.4f),
                          new Vector3(SeritX[s], UretimY, 0f), 1f, 3);
            dusenler.Add(new Dusen { gecis = e.transform });
        }

        // Boş şeride ara sıra yıldız koy: oyuncuyu şerit değiştirmeye teşvik eder.
        if (Random.value < 0.28f)
        {
            var y = Nesne("Yildiz", Cizim.Yildiz(Renk.Sari, 48), new Vector3(SeritX[bos], UretimY + 0.4f, 0f), 0.7f, 4);
            dusenler.Add(new Dusen { gecis = y.transform, yildiz = true });
        }
    }

    /// <summary>Tek engelli turda hangi şeridin dolacağı — boş şeridin komşusu.</summary>
    int TekEngelSeridi(int bos)
    {
        int aday = bos == 0 ? 1 : bos == 2 ? 1 : (Random.value < 0.5f ? 0 : 2);
        return aday;
    }

    static bool Carpisti(Vector3 aMerkez, Vector2 aYari, Vector3 bMerkez, Vector2 bYari)
    {
        return Mathf.Abs(aMerkez.x - bMerkez.x) < aYari.x + bYari.x &&
               Mathf.Abs(aMerkez.y - bMerkez.y) < aYari.y + bYari.y;
    }
}
