using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Örüntü'nün beyni: karo özellikleri, kural üretimi, bölüm zorlukları ve çeldiriciler.
/// Görsel iş yok — oynanışın matematiği burada, ayar buradan yapılır.
/// </summary>
public static class OruntuAyar
{
    public const int ToplamBolum = 12;
    public const int SoruSayisi = 5;      // her bölümde
    public const int Can = 3;             // bölüm başına hata hakkı

    public enum Ozellik { Sekil, Renk, Boyut, Donme, Adet }

    public const int SekilAdet = 5;   // daire, kare, üçgen, yıldız, halka
    public const int RenkAdet = 5;
    public const int BoyutAdet = 3;
    public const int DonmeAdet = 8;   // 45'er derece
    public const int AdetAdet = 3;    // 1..3 küçük şekil

    public static readonly string[] SekilAdi = { "daire", "kare", "üçgen", "yıldız", "halka" };
    public static readonly string[] RenkAdi = { "turkuaz", "mavi", "mor", "pembe", "sarı" };
    public static readonly string[] BoyutAdi = { "küçük", "orta", "büyük" };

    /// <summary>Karo renkleri. Ekran ve çalışma kâğıdı aynı paleti kullansın diye burada.</summary>
    public static readonly Color[] Palet =
        { Ulu.Renk.Turkuaz, Ulu.Renk.Mavi, Ulu.Renk.Mor, Ulu.Renk.Pembe, Ulu.Renk.Sari };

    /// <summary>Boyut basamaklarının ölçek karşılığı (küçük → büyük).</summary>
    public static readonly float[] BoyutOlcek = { 0.52f, 0.76f, 1.0f };

    public struct Oge
    {
        public int sekil, renk, boyut, donme, adet;

        public int this[Ozellik o] => o switch
        {
            Ozellik.Sekil => sekil,
            Ozellik.Renk => renk,
            Ozellik.Boyut => boyut,
            Ozellik.Donme => donme,
            _ => adet
        };

        public Oge Degistir(Ozellik o, int deger)
        {
            var y = this;
            switch (o)
            {
                case Ozellik.Sekil: y.sekil = deger; break;
                case Ozellik.Renk: y.renk = deger; break;
                case Ozellik.Boyut: y.boyut = deger; break;
                case Ozellik.Donme: y.donme = deger; break;
                default: y.adet = deger; break;
            }
            return y;
        }

        public bool Ayni(Oge b) =>
            sekil == b.sekil && renk == b.renk && boyut == b.boyut && donme == b.donme && adet == b.adet;
    }

    public static int Mod(Ozellik o) => o switch
    {
        Ozellik.Sekil => SekilAdet,
        Ozellik.Renk => RenkAdet,
        Ozellik.Boyut => BoyutAdet,
        Ozellik.Donme => DonmeAdet,
        _ => AdetAdet
    };

    // --- kurallar ---

    public enum KuralTipi { Sabit, Ilerle, Alternatif }

    public struct Kural
    {
        public KuralTipi tip;
        public int baslangic;   // ilk değer (harita varsa harita içindeki sıra)
        public int adim;        // Ilerle için (+/-)
        public int ikinci;      // Alternatif için
        public int mod;

        /// <summary>
        /// Değer evrenini daraltmak için: kural 0..harita.Length-1 arasında döner, dışarıya
        /// harita[deger] verilir. Dönme aktifken şekli yalnızca asimetrik şekillerle sınırlamak
        /// gibi durumlar için — yoksa dönen bir halka ile dönmeyen halka aynı görünür.
        /// </summary>
        public int[] harita;

        public int Deger(int sira)
        {
            int ham = tip switch
            {
                KuralTipi.Sabit => baslangic,
                KuralTipi.Ilerle => Sar(baslangic + sira * adim, mod),
                _ => sira % 2 == 0 ? baslangic : ikinci
            };
            return harita == null ? ham : harita[Sar(ham, harita.Length)];
        }
    }

    /// <summary>Dönme görünür olsun diye izin verilen şekiller: üçgen ve yıldız.</summary>
    public static readonly int[] AsimetrikSekiller = { 2, 3 };

    /// <summary>Döndürüldüğünde farkı görünmeyen şekiller (daire, halka).</summary>
    public static bool DonmeGorunmez(int sekil) => sekil == 0 || sekil == 4;

    /// <summary>
    /// Bir özelliğin ekranda görünen değeri. Dönme, şeklin simetrisine göre sadeleştirilir:
    /// daire/halka her açıda aynıdır, kare 90°'de kendine eşittir. Ham değerle karşılaştırma
    /// yapmak "farklı ama aynı görünen" karolar üretir.
    /// </summary>
    public static int GorunenDeger(Ozellik o, Oge k)
    {
        if (o != Ozellik.Donme) return k[o];
        if (DonmeGorunmez(k.sekil)) return 0;
        return k.sekil == 1 ? k.donme % 2 : k.donme;
    }

    /// <summary>
    /// Karonun ekranda göründüğü hâlin kimliği. İki karo aynı anahtara sahipse öğrenci için
    /// ayırt edilemezler. Soru üretimi bu anahtarla doğrulanır.
    /// </summary>
    public static string GorselAnahtar(Oge o) =>
        $"{o.sekil}-{o.renk}-{o.boyut}-{o.adet}-{GorunenDeger(Ozellik.Donme, o)}";

    /// <summary>Listedeki karolardan ikisi ekranda aynı görünüyor mu?</summary>
    public static bool AyirtEdilemezVar(IList<Oge> ogeler)
    {
        var gorulen = new HashSet<string>();
        foreach (var o in ogeler)
            if (!gorulen.Add(GorselAnahtar(o))) return true;
        return false;
    }

    static int Sar(int deger, int mod) => ((deger % mod) + mod) % mod;

    // --- bölüm zorluğu ---

    public struct BolumAyar
    {
        public int ozellikSayisi;   // kaç özellik aynı anda değişiyor
        public int diziUzunlugu;    // ekrandaki karo sayısı (gizli dahil)
        public bool ortadanSor;     // eksik karo sonda değil ortada
        public bool inceCeldirici;  // çeldiriciler bir önceki karonun değerini taşısın
    }

    public static BolumAyar Bolum(int no)
    {
        no = Mathf.Clamp(no, 1, ToplamBolum);
        return new BolumAyar
        {
            ozellikSayisi = no <= 2 ? 1 : no <= 6 ? 2 : no <= 10 ? 3 : 4,
            diziUzunlugu = no <= 4 ? 5 : 6,
            ortadanSor = no >= 9,
            inceCeldirici = no >= 5
        };
    }

    // --- uyarlanır zorluk ---

    /// <summary>Üst üste bu kadar doğrudan sonra bir özellik daha devreye girer.</summary>
    public const int UyumEsigi = 2;

    /// <summary>Kaydırmanın sınırları: bölüm tabanının bir üstü, bir altı.</summary>
    public const int UyumEnCok = 1;
    public const int UyumEnAz = -1;

    /// <summary>Bölüm tabanı + oyun içi kaydırma. Soru üretimi bunu kullanır.</summary>
    public static int OzellikSayisi(int bolum, int kaydirma) =>
        Mathf.Clamp(Bolum(bolum).ozellikSayisi + Mathf.Clamp(kaydirma, UyumEnAz, UyumEnCok), 1, 5);

    /// <summary>Doğru/yanlış sonrası yeni kaydırma: üst üste doğruda artar, her hatada düşer.</summary>
    public static int KaydirmayiGuncelle(int kaydirma, ref int ustusteDogru, bool dogru)
    {
        if (!dogru)
        {
            ustusteDogru = 0;
            return Mathf.Max(kaydirma - 1, UyumEnAz);
        }

        ustusteDogru++;
        if (ustusteDogru < UyumEsigi) return kaydirma;
        ustusteDogru = 0;
        return Mathf.Min(kaydirma + 1, UyumEnCok);
    }

    // --- üretim ---

    /// <summary>Bölüme uygun aktif özellikleri seçer. Dönme aktifse şekil sabit üçgene çekilir
    /// (dönmenin görülebilmesi için), böylece soru okunur kalır.</summary>
    public static Ozellik[] AktifOzellikler(int bolum, int kaydirma = 0)
    {
        var havuz = new List<Ozellik> { Ozellik.Renk, Ozellik.Sekil, Ozellik.Boyut, Ozellik.Adet, Ozellik.Donme };

        // Karıştır
        for (int i = havuz.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (havuz[i], havuz[j]) = (havuz[j], havuz[i]);
        }

        var secili = havuz.GetRange(0, Mathf.Min(OzellikSayisi(bolum, kaydirma), havuz.Count));
        return secili.ToArray();
    }

    public static Dictionary<Ozellik, Kural> KurallariUret(Ozellik[] aktif)
    {
        var kurallar = new Dictionary<Ozellik, Kural>();
        foreach (Ozellik o in System.Enum.GetValues(typeof(Ozellik)))
        {
            int mod = Mod(o);
            bool acik = System.Array.IndexOf(aktif, o) >= 0;

            if (!acik)
            {
                int sabit = Random.Range(0, mod);
                // Dönme aktif değilse karoları düz tut; okunurluk için.
                if (o == Ozellik.Donme) sabit = 0;
                if (o == Ozellik.Adet) sabit = 0;
                kurallar[o] = new Kural { tip = KuralTipi.Sabit, baslangic = sabit, mod = mod };
                continue;
            }

            bool alternatif = Random.value < 0.3f && mod > 2;
            if (alternatif)
            {
                int a = Random.Range(0, mod);
                int b = (a + Random.Range(1, mod)) % mod;
                kurallar[o] = new Kural { tip = KuralTipi.Alternatif, baslangic = a, ikinci = b, mod = mod };
            }
            else
            {
                int adim = Random.value < 0.5f ? 1 : -1;
                if (o == Ozellik.Donme && Random.value < 0.4f) adim *= 2;   // 90 derece
                kurallar[o] = new Kural
                {
                    tip = KuralTipi.Ilerle,
                    baslangic = Random.Range(0, mod),
                    adim = adim,
                    mod = mod
                };
            }
        }

        // Dönme aktifse şekli asimetriklerle sınırla: dönen bir daire/halka dönmeyeninden
        // ayırt edilemez, o zaman soru çözülemez hale gelir.
        if (System.Array.IndexOf(aktif, Ozellik.Donme) >= 0)
        {
            var k = kurallar[Ozellik.Sekil];
            if (System.Array.IndexOf(aktif, Ozellik.Sekil) >= 0)
            {
                // Şekil de değişiyor: yalnızca üçgen ↔ yıldız arasında gidip gelsin.
                k.harita = AsimetrikSekiller;
                k.mod = AsimetrikSekiller.Length;
                k.baslangic = Random.Range(0, AsimetrikSekiller.Length);
                k.ikinci = (k.baslangic + 1) % AsimetrikSekiller.Length;
                if (k.tip == KuralTipi.Ilerle) k.adim = 1;
            }
            else
            {
                k.tip = KuralTipi.Sabit;
                k.harita = null;
                k.baslangic = 2;   // üçgen
            }
            kurallar[Ozellik.Sekil] = k;
        }

        return kurallar;
    }

    public static Oge OgeUret(Dictionary<Ozellik, Kural> kurallar, int sira) => new Oge
    {
        sekil = kurallar[Ozellik.Sekil].Deger(sira),
        renk = kurallar[Ozellik.Renk].Deger(sira),
        boyut = kurallar[Ozellik.Boyut].Deger(sira),
        donme = kurallar[Ozellik.Donme].Deger(sira),
        adet = kurallar[Ozellik.Adet].Deger(sira)
    };

    // --- tam soru ---

    /// <summary>Ekrana ya da kâğıda basılmaya hazır tek soru.</summary>
    public struct Soru
    {
        public Oge[] dizi;
        public int gizli;             // dizide eksik olan karonun sırası
        public Oge[] secenekler;
        public int dogru;             // seçenekler içinde doğrunun sırası
        public Ozellik[] aktif;
        public Dictionary<Ozellik, Kural> kurallar;

        public string Aciklama => KuralAciklamasi(kurallar, aktif);
    }

    /// <summary>
    /// Bölüme uygun bir soru üretir. Emniyet: seçeneklerden ikisi ekranda aynı görünüyorsa ya da
    /// dizi hiç değişmiyorsa soru çözülemez demektir — yeniden denenir. Oyun, testler ve
    /// çalışma kâğıdı aynı yoldan geçsin diye tek yerde.
    /// </summary>
    public static Soru SoruUret(int bolum, int kaydirma = 0, Ozellik? odak = null)
    {
        var ayar = Bolum(bolum);
        var s = new Soru();
        List<Oge> liste = null;
        Oge dogru = default;

        for (int deneme = 0; deneme < 12; deneme++)
        {
            s.aktif = odak.HasValue ? new[] { odak.Value } : AktifOzellikler(bolum, kaydirma);
            s.kurallar = KurallariUret(s.aktif);

            s.dizi = new Oge[ayar.diziUzunlugu];
            for (int i = 0; i < s.dizi.Length; i++) s.dizi[i] = OgeUret(s.kurallar, i);

            s.gizli = ayar.ortadanSor && !odak.HasValue
                ? Random.Range(1, s.dizi.Length - 1)
                : s.dizi.Length - 1;

            dogru = s.dizi[s.gizli];
            var onceki = s.dizi[Mathf.Max(0, s.gizli - 1)];
            liste = new List<Oge>(Celdiriciler(dogru, onceki, s.aktif, ayar.inceCeldirici, s.kurallar)) { dogru };

            bool diziDegisiyor = !AyirtEdilemezVar(new[] { s.dizi[0], s.dizi[1] });
            if (!AyirtEdilemezVar(liste) && diziDegisiyor) break;
        }

        for (int i = liste.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (liste[i], liste[j]) = (liste[j], liste[i]);
        }

        s.secenekler = liste.ToArray();
        s.dogru = System.Array.FindIndex(s.secenekler, x => x.Ayni(dogru));
        return s;
    }

    /// <summary>
    /// Çeldiriciler: rastgele değil, "az kalsın doğru" olacak şekilde üretilir —
    /// bir özelliği bir önceki karodan alan ya da tek adım kayan seçenekler.
    /// Öğrenciyi kuralı gerçekten okumaya zorlayan kısım burası.
    /// </summary>
    public static List<Oge> Celdiriciler(Oge dogru, Oge onceki, Ozellik[] aktif, bool ince,
                                         Dictionary<Ozellik, Kural> kurallar = null, int adet = 3)
    {
        var liste = new List<Oge>();
        // Karşılaştırmalar ham değerle değil GÖRÜNEN hâlle yapılır: kare 90° dönünce
        // kendine eşittir, dolayısıyla "farklı" bir çeldirici ekranda aynı görünebilir.
        var anahtarlar = new HashSet<string> { GorselAnahtar(dogru) };
        int guvenlik = 0;

        while (liste.Count < adet && guvenlik++ < 200)
        {
            Ozellik o = aktif[Random.Range(0, aktif.Length)];

            // Daire/halka döndürülünce fark görünmez; böyle bir çeldirici doğrudan
            // ayırt edilemeyen bir seçenek üretir, atla.
            if (o == Ozellik.Donme && DonmeGorunmez(dogru.sekil)) continue;

            Oge aday;

            if (ince && Random.value < 0.5f)
            {
                aday = dogru.Degistir(o, onceki[o]);          // bir önceki karonun değeri
            }
            else
            {
                aday = dogru.Degistir(o, KomsuDeger(o, dogru[o], kurallar));
            }

            if (!anahtarlar.Add(GorselAnahtar(aday))) continue;   // doğruyla ya da başka bir seçenekle aynı görünüyor
            liste.Add(aday);
        }

        // Kilitlenirse (çok dar özellik uzayı) renk/boyut üzerinden tamamla —
        // bu ikisi her şekilde görünür fark yaratır.
        for (int renk = 0; renk < RenkAdet && liste.Count < adet; renk++)
        {
            for (int boyut = 0; boyut < BoyutAdet && liste.Count < adet; boyut++)
            {
                var aday = dogru.Degistir(Ozellik.Renk, renk).Degistir(Ozellik.Boyut, boyut);
                if (anahtarlar.Add(GorselAnahtar(aday))) liste.Add(aday);
            }
        }

        return liste;
    }

    /// <summary>
    /// Bir özelliğin komşu değeri. Kuralın haritası varsa (ör. dönme aktifken şekil yalnızca
    /// üçgen/yıldız) çeldirici de o evrenin içinde kalır — dizide hiç görülmeyen bir şekil
    /// atmak çeldiriciyi bedavaya elenir hale getirir.
    /// </summary>
    static int KomsuDeger(Ozellik o, int mevcut, Dictionary<Ozellik, Kural> kurallar)
    {
        int[] harita = null;
        if (kurallar != null && kurallar.TryGetValue(o, out var k)) harita = k.harita;

        if (harita != null && harita.Length > 1)
        {
            int sira = System.Array.IndexOf(harita, mevcut);
            if (sira < 0) sira = 0;
            int kaymaH = Random.value < 0.5f ? 1 : harita.Length - 1;
            return harita[(sira + kaymaH) % harita.Length];
        }

        int mod = Mod(o);
        int kayma = Random.value < 0.5f ? 1 : mod - 1;
        return (mevcut + kayma) % mod;
    }

    // --- açıklama (yanlış cevaptan sonra öğretim anı) ---

    public static string KuralAciklamasi(Dictionary<Ozellik, Kural> kurallar, Ozellik[] aktif)
    {
        var parcalar = new List<string>();
        foreach (var o in aktif) parcalar.Add(Anlat(o, kurallar[o]));
        return string.Join("   ·   ", parcalar);
    }

    public static string Ad(Ozellik o) => o switch
    {
        Ozellik.Sekil => "şekil",
        Ozellik.Renk => "renk",
        Ozellik.Boyut => "boyut",
        Ozellik.Donme => "dönme",
        _ => "adet"
    };

    static string Anlat(Ozellik o, Kural k)
    {
        string ad = Ad(o);

        // Not: harita varsa baslangic/ikinci ham sıra numarasıdır, gerçek değeri Deger(sira) verir.
        if (k.tip == KuralTipi.Alternatif)
            return $"{ad}: {Deger(o, k.Deger(0))} ile {Deger(o, k.Deger(1))} dönüşümlü";

        if (k.tip == KuralTipi.Sabit)
            return $"{ad}: değişmiyor";

        if (o == Ozellik.Donme)
            return $"dönme: her karoda {Mathf.Abs(k.adim) * 45}° {(k.adim > 0 ? "sağa" : "sola")}";

        string yon = k.adim > 0 ? "ileri" : "geri";
        return $"{ad}: sırayla bir {yon} ({Deger(o, k.Deger(0))} → {Deger(o, k.Deger(1))} → {Deger(o, k.Deger(2))})";
    }

    public static string Deger(Ozellik o, int v) => o switch
    {
        Ozellik.Sekil => SekilAdi[Mathf.Clamp(v, 0, SekilAdet - 1)],
        Ozellik.Renk => RenkAdi[Mathf.Clamp(v, 0, RenkAdet - 1)],
        Ozellik.Boyut => BoyutAdi[Mathf.Clamp(v, 0, BoyutAdet - 1)],
        Ozellik.Adet => (v + 1).ToString(),
        _ => (v * 45) + "°"
    };

    // --- "neden yanlış?" ---

    /// <summary>İki karonun ekranda ayrıldığı özellikler (ham değil, görünen değere göre).</summary>
    public static List<Ozellik> Farklar(Oge secilen, Oge dogru)
    {
        var liste = new List<Ozellik>();
        foreach (Ozellik o in System.Enum.GetValues(typeof(Ozellik)))
            if (GorunenDeger(o, secilen) != GorunenDeger(o, dogru)) liste.Add(o);
        return liste;
    }

    /// <summary>
    /// "renk: sarı yerine mor" — yanlış seçimin nerede saptığı. Kuralın tamamını tekrar
    /// anlatmak yerine öğrencinin kendi hatasını gösterir; yanlıştan öğrenme kısmı burası.
    /// </summary>
    public static string FarkAciklamasi(Oge secilen, Oge dogru)
    {
        var parcalar = new List<string>();
        foreach (var o in Farklar(secilen, dogru))
            parcalar.Add($"{Ad(o)}: {Deger(o, secilen[o])} yerine {Deger(o, dogru[o])}");
        return string.Join("   ·   ", parcalar);
    }
}
