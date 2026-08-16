using System.Collections.Generic;
using UnityEngine;

namespace Ulu
{
    /// <summary>Hazır palet. Oyunlar arası görsel tutarlılık için buradan renk seçilir.</summary>
    public static class Renk
    {
        public static readonly Color Gece = new Color(0.07f, 0.08f, 0.13f);
        public static readonly Color Duman = new Color(0.16f, 0.18f, 0.25f);
        public static readonly Color Turkuaz = new Color(0.25f, 0.86f, 0.82f);
        public static readonly Color Mavi = new Color(0.33f, 0.60f, 0.98f);
        public static readonly Color Mor = new Color(0.68f, 0.45f, 0.98f);
        public static readonly Color Pembe = new Color(0.98f, 0.42f, 0.62f);
        public static readonly Color Kirmizi = new Color(0.95f, 0.33f, 0.33f);
        public static readonly Color Turuncu = new Color(0.98f, 0.60f, 0.25f);
        public static readonly Color Sari = new Color(0.99f, 0.83f, 0.30f);
        public static readonly Color Yesil = new Color(0.40f, 0.85f, 0.47f);
        public static readonly Color Beyaz = new Color(0.96f, 0.97f, 1.00f);
        public static readonly Color Gri = new Color(0.55f, 0.58f, 0.68f);
    }

    /// <summary>
    /// Sprite'ları kodla üretir; projede hiç görsel dosya tutulmaz.
    /// Üretilen sprite'lar imzalarına göre önbelleğe alınır, tekrar üretilmez.
    /// 64 piksel = 1 dünya birimi (pixelsPerUnit).
    /// </summary>
    public static class Cizim
    {
        public const float PikselBirim = 64f;

        static readonly Dictionary<string, Sprite> onbellek = new Dictionary<string, Sprite>();

        static Sprite Uret(string anahtar, int en, int boy, System.Func<float, float, float> kapsama, Color renk)
        {
            if (onbellek.TryGetValue(anahtar, out var hazir) && hazir != null) return hazir;

            var doku = new Texture2D(en, boy, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            var pikseller = new Color32[en * boy];
            for (int y = 0; y < boy; y++)
            {
                for (int x = 0; x < en; x++)
                {
                    // Piksel merkezini -1..1 aralığına taşı.
                    float nx = (x + 0.5f) / en * 2f - 1f;
                    float ny = (y + 0.5f) / boy * 2f - 1f;
                    float a = Mathf.Clamp01(kapsama(nx, ny));
                    var c = renk;
                    c.a *= a;
                    pikseller[y * en + x] = c;
                }
            }
            doku.SetPixels32(pikseller);
            doku.Apply();

            var sprite = Sprite.Create(doku, new Rect(0, 0, en, boy), new Vector2(0.5f, 0.5f), PikselBirim);
            sprite.name = anahtar;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            onbellek[anahtar] = sprite;
            return sprite;
        }

        // Kenarları yumuşatmak için: uzaklık fonksiyonundan bir piksellik geçiş üretir.
        static float Yumusat(float mesafe, float pikselAdimi)
        {
            return Mathf.Clamp01(0.5f - mesafe / Mathf.Max(pikselAdimi, 0.0001f));
        }

        public static Sprite Kare(Color renk, int boy = 64)
        {
            return Uret($"kare_{boy}_{Kod(renk)}", boy, boy, (x, y) => 1f, renk);
        }

        public static Sprite Dikdortgen(Color renk, int en = 64, int boy = 64)
        {
            return Uret($"dik_{en}x{boy}_{Kod(renk)}", en, boy, (x, y) => 1f, renk);
        }

        public static Sprite Daire(Color renk, int cap = 64)
        {
            float adim = 2f / cap;
            return Uret($"daire_{cap}_{Kod(renk)}", cap, cap,
                (x, y) => Yumusat(Mathf.Sqrt(x * x + y * y) - 1f + adim, adim), renk);
        }

        public static Sprite Halka(Color renk, int cap = 64, float kalinlik = 0.25f)
        {
            float adim = 2f / cap;
            float ic = 1f - Mathf.Clamp01(kalinlik);
            return Uret($"halka_{cap}_{kalinlik:F2}_{Kod(renk)}", cap, cap, (x, y) =>
            {
                float r = Mathf.Sqrt(x * x + y * y);
                float dis = Yumusat(r - 1f + adim, adim);
                float icBosluk = Yumusat(ic - r, adim);
                return Mathf.Min(dis, icBosluk);
            }, renk);
        }

        /// <summary>Köşeleri yuvarlatılmış dikdörtgen. yuvarlaklik 0..1 (kısa kenara oranla).</summary>
        public static Sprite YuvarlakKare(Color renk, int en = 64, int boy = 64, float yuvarlaklik = 0.3f)
        {
            float adim = 2f / Mathf.Min(en, boy);
            float r = Mathf.Clamp01(yuvarlaklik);
            return Uret($"ykare_{en}x{boy}_{r:F2}_{Kod(renk)}", en, boy, (x, y) =>
            {
                float ax = Mathf.Max(Mathf.Abs(x) - (1f - r), 0f);
                float ay = Mathf.Max(Mathf.Abs(y) - (1f - r), 0f);
                float mesafe = Mathf.Sqrt(ax * ax + ay * ay) - r;
                return Yumusat(mesafe + adim, adim);
            }, renk);
        }

        public static Sprite Ucgen(Color renk, int boy = 64)
        {
            float adim = 2f / boy;
            return Uret($"ucgen_{boy}_{Kod(renk)}", boy, boy, (x, y) =>
            {
                // Tepesi yukarıda, tabanı altta üçgen.
                float sol = (y + 1f) * 0.5f + x;      // sol kenar
                float sag = (y + 1f) * 0.5f - x;      // sağ kenar
                float alt = y + 1f;                   // taban
                float en = Mathf.Min(Mathf.Min(sol, sag), alt);
                return Mathf.Clamp01(en / adim);
            }, renk);
        }

        public static Sprite Yildiz(Color renk, int boy = 64, int kol = 5, float icOran = 0.45f)
        {
            float adim = 2f / boy;
            return Uret($"yildiz_{boy}_{kol}_{icOran:F2}_{Kod(renk)}", boy, boy, (x, y) =>
            {
                float aci = Mathf.Atan2(y, x);
                float r = Mathf.Sqrt(x * x + y * y);
                float dilim = Mathf.PI * 2f / kol;
                float t = Mathf.Repeat(aci + Mathf.PI / 2f, dilim) / dilim;   // 0..1
                float ucgenDalga = Mathf.Abs(t * 2f - 1f);                    // uçta 1, çukurda 0
                float sinir = Mathf.Lerp(icOran, 1f, ucgenDalga);
                return Yumusat(r - sinir, adim);
            }, renk);
        }

        /// <summary>Ekranı kaplayan düz renk (panel, gölge perde vb. için).</summary>
        public static Sprite Nokta(Color renk)
        {
            return Uret($"nokta_{Kod(renk)}", 4, 4, (x, y) => 1f, renk);
        }

        static string Kod(Color c)
        {
            return ColorUtility.ToHtmlStringRGBA(c);
        }
    }
}
