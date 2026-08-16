using UnityEngine;
using UnityEngine.UI;

namespace Ulu
{
    /// <summary>
    /// Her mini oyunun ortak omurgası: kamera, ekran arayüzü, menü → oyna → bitti döngüsü,
    /// skor ve rekor kaydı, duraklatma, yeniden başlatma.
    /// Sahnede tek bir boş nesnenin üstünde durur; geri kalan her şeyi kod kurar.
    /// Türeyen sınıf yalnızca Kur / Basla / Oyna yazar.
    /// </summary>
    public abstract class OyunTabani : MonoBehaviour
    {
        public enum Durum { Menu, Oynaniyor, Duraklatildi, Bitti }

        public Durum durum { get; private set; } = Durum.Menu;
        public int Skor { get; private set; }
        public int EnIyi { get; private set; }

        // --- türeyen oyunun dolduracakları ---
        protected abstract string OyunAdi { get; }
        protected abstract string NasilOynanir { get; }
        protected virtual Color ArkaPlan => Renk.Gece;
        protected virtual float KameraBoyu => 5f;
        protected virtual string SkorEtiketi => "Skor";

        /// <summary>Bir kez çalışır: kalıcı sahne öğeleri (zemin, şerit çizgileri, dekor).</summary>
        protected abstract void Kur();

        /// <summary>Her turun başında çalışır: oyuncuyu ve dünyayı sıfırla.</summary>
        protected abstract void Basla();

        /// <summary>Oyun oynanırken her karede çalışır.</summary>
        protected abstract void Oyna(float dt);

        /// <summary>Tur bittiğinde çalışır (isteğe bağlı).</summary>
        protected virtual void Bitti() { }

        // --- ortak alanlar ---
        protected Camera Kam { get; private set; }

        /// <summary>Tur boyunca yaşayan nesnelerin kökü; her turda temizlenir.</summary>
        protected Transform Alan { get; private set; }

        /// <summary>Kalıcı sahne öğelerinin kökü (Kur içinde kurulanlar); turlar arasında silinmez.</summary>
        protected Transform Dekor { get; private set; }

        protected float EkranYariEn => Kam.orthographicSize * Kam.aspect;
        protected float EkranYariBoy => Kam.orthographicSize;

        Canvas kanvas;
        Text skorYazi, enIyiYazi, durumYazi, baslikYazi, altYazi;
        Image perde;
        float bitisZamani;

        void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            Kam = Arayuz.Kamera(ArkaPlan, KameraBoyu);
            Alan = new GameObject("Alan").transform;
            Dekor = new GameObject("Dekor").transform;

            kanvas = Arayuz.Kanvas();
            perde = Arayuz.Perde(kanvas.transform, new Color(0f, 0f, 0f, 0.55f));

            skorYazi = Arayuz.Yazi(kanvas.transform, "", 34, Kose.SolUst, new Vector2(24f, 20f), Renk.Beyaz);
            enIyiYazi = Arayuz.Yazi(kanvas.transform, "", 26, Kose.SagUst, new Vector2(24f, 24f), Renk.Gri);
            baslikYazi = Arayuz.Yazi(kanvas.transform, "", 72, Kose.Orta, new Vector2(0f, 90f), Renk.Beyaz);
            durumYazi = Arayuz.Yazi(kanvas.transform, "", 34, Kose.Orta, new Vector2(0f, -10f), Renk.Turkuaz);
            altYazi = Arayuz.Yazi(kanvas.transform, "", 26, Kose.Orta, new Vector2(0f, -100f), Renk.Gri);

            EnIyi = Kayit.EnIyi(OyunAdi);
            Kur();
            MenuyeDon();
        }

        void Update()
        {
            switch (durum)
            {
                case Durum.Menu:
                    if (Girdi.Onayla) { Girdi.Tuket(); TuruBaslat(); }
                    break;

                case Durum.Oynaniyor:
                    if (Girdi.DuraklatBasildi) { Girdi.Tuket(); Duraklat(true); break; }
                    Oyna(Time.deltaTime);
                    break;

                case Durum.Duraklatildi:
                    if (Girdi.Onayla || Girdi.DuraklatBasildi) { Girdi.Tuket(); Duraklat(false); }
                    break;

                case Durum.Bitti:
                    // Kazara anında yeniden başlamayı önlemek için kısa bekleme.
                    if (Time.unscaledTime - bitisZamani > 0.6f && Girdi.Yeniden) { Girdi.Tuket(); TuruBaslat(); }
                    break;
            }
        }

        // --- türeyen oyunun çağıracakları ---

        protected SpriteRenderer Nesne(string ad, Sprite sprite, Vector3 konum = default,
                                       float olcek = 1f, int siralama = 0, Transform ebeveyn = null)
        {
            var go = new GameObject(ad);
            go.transform.SetParent(ebeveyn != null ? ebeveyn : Alan, false);
            go.transform.position = konum;
            go.transform.localScale = Vector3.one * olcek;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = siralama;
            return sr;
        }

        protected void PuanEkle(int puan)
        {
            Skor = Mathf.Max(0, Skor + puan);
            skorYazi.text = $"{SkorEtiketi}: {Skor}";
        }

        protected void SkoruAyarla(int skor)
        {
            Skor = Mathf.Max(0, skor);
            skorYazi.text = $"{SkorEtiketi}: {Skor}";
        }

        /// <summary>Ekranın ortasına kısa bir bilgi yazar (boş metin gizler).</summary>
        protected void Yaz(string metin)
        {
            durumYazi.text = metin ?? "";
        }

        protected void Bitir(string mesaj = "Oyun Bitti")
        {
            if (durum == Durum.Bitti) return;
            durum = Durum.Bitti;
            bitisZamani = Time.unscaledTime;
            Time.timeScale = 0f;

            bool rekor = Kayit.EnIyiDene(OyunAdi, Skor);
            EnIyi = Kayit.EnIyi(OyunAdi);
            enIyiYazi.text = $"Rekor: {EnIyi}";

            perde.enabled = true;
            baslikYazi.text = mesaj;
            durumYazi.text = rekor ? $"Yeni rekor: {Skor}" : $"{SkorEtiketi}: {Skor}   (Rekor: {EnIyi})";
            altYazi.text = "Yeniden oynamak için tıkla veya R";

            Ses.Bitis();
            Bitti();
        }

        /// <summary>Rastgele ama tekrar edilebilir sayı üretmek isteyen oyunlar için ortak kolaylık.</summary>
        protected static float Rastgele(float en, float boy) => Random.Range(en, boy);

        // --- iç akış ---

        void MenuyeDon()
        {
            durum = Durum.Menu;
            Time.timeScale = 1f;
            SkoruAyarla(0);
            enIyiYazi.text = EnIyi > 0 ? $"Rekor: {EnIyi}" : "";
            perde.enabled = true;
            baslikYazi.text = OyunAdi;
            durumYazi.text = NasilOynanir;
            altYazi.text = "Başlamak için tıkla veya Boşluk";
        }

        void TuruBaslat()
        {
            Temizle();
            SkoruAyarla(0);
            Yaz("");
            perde.enabled = false;
            baslikYazi.text = "";
            altYazi.text = "";
            Time.timeScale = 1f;
            durum = Durum.Oynaniyor;
            Ses.Basla();
            Basla();
        }

        void Duraklat(bool duraklat)
        {
            durum = duraklat ? Durum.Duraklatildi : Durum.Oynaniyor;
            Time.timeScale = duraklat ? 0f : 1f;
            perde.enabled = duraklat;
            baslikYazi.text = duraklat ? "Duraklatıldı" : "";
            altYazi.text = duraklat ? "Devam etmek için tıkla veya Esc" : "";
        }

        void Temizle()
        {
            for (int i = Alan.childCount - 1; i >= 0; i--)
                Destroy(Alan.GetChild(i).gameObject);
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
