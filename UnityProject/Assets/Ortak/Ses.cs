using UnityEngine;

namespace Ulu
{
    public enum Dalga { Sinus, Kare, Testere, Ucgen }

    /// <summary>
    /// Sesleri kodla üretir; projede hiç ses dosyası tutulmaz.
    /// Kısa efektler için (bip, zıplama, çarpma, puan). Üretilen klipler önbelleklenir.
    /// </summary>
    public static class Ses
    {
        const int OrneklemeHizi = 44100;

        public static float Seviye = 0.5f;
        public static bool Acik = true;

        static AudioSource kaynak;
        static AudioClip zipla, puan, carpma, basla, bitis, tik;

        static AudioSource Kaynak()
        {
            if (kaynak != null) return kaynak;
            var go = new GameObject("SesKaynagi") { hideFlags = HideFlags.HideAndDontSave };
            Object.DontDestroyOnLoad(go);
            kaynak = go.AddComponent<AudioSource>();
            kaynak.playOnAwake = false;
            kaynak.spatialBlend = 0f;
            return kaynak;
        }

        /// <summary>Tek tonluk klip. bitisHz verilirse ton kayar (yükselen/alçalan efekt).</summary>
        public static AudioClip Ton(float hz, float sure, Dalga tip = Dalga.Kare, float bitisHz = -1f, float gurultu = 0f)
        {
            int adet = Mathf.Max(1, Mathf.RoundToInt(OrneklemeHizi * sure));
            var veri = new float[adet];
            float faz = 0f;
            for (int i = 0; i < adet; i++)
            {
                float t = (float)i / adet;
                float f = bitisHz > 0f ? Mathf.Lerp(hz, bitisHz, t) : hz;
                faz += f / OrneklemeHizi;
                faz -= Mathf.Floor(faz);

                float ornek = tip switch
                {
                    Dalga.Sinus => Mathf.Sin(faz * Mathf.PI * 2f),
                    Dalga.Kare => faz < 0.5f ? 1f : -1f,
                    Dalga.Testere => faz * 2f - 1f,
                    _ => 1f - 4f * Mathf.Abs(faz - 0.5f)
                };
                if (gurultu > 0f) ornek = Mathf.Lerp(ornek, Random.Range(-1f, 1f), gurultu);

                // Zarf: hızlı yükseliş, üstel iniş — tıkırtıyı önler.
                float acilis = Mathf.Clamp01(t / 0.02f);
                float kapanis = Mathf.Exp(-4f * t);
                veri[i] = ornek * acilis * kapanis * 0.6f;
            }

            var klip = AudioClip.Create("ton", adet, 1, OrneklemeHizi, false);
            klip.SetData(veri, 0);
            return klip;
        }

        public static void Cal(AudioClip klip, float ses = 1f)
        {
            if (!Acik || klip == null) return;
            Kaynak().PlayOneShot(klip, Mathf.Clamp01(ses * Seviye));
        }

        public static void Zipla() { Cal(zipla ??= Ton(320f, 0.12f, Dalga.Kare, 620f)); }
        public static void Puan() { Cal(puan ??= Ton(660f, 0.14f, Dalga.Ucgen, 990f)); }
        public static void Tik() { Cal(tik ??= Ton(880f, 0.05f, Dalga.Kare), 0.6f); }
        public static void Carpma() { Cal(carpma ??= Ton(180f, 0.35f, Dalga.Testere, 60f, 0.5f)); }
        public static void Basla() { Cal(basla ??= Ton(440f, 0.18f, Dalga.Ucgen, 880f)); }
        public static void Bitis() { Cal(bitis ??= Ton(400f, 0.5f, Dalga.Testere, 120f)); }
    }
}
