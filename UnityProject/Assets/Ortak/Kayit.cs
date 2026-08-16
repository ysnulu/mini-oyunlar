using UnityEngine;

namespace Ulu
{
    /// <summary>
    /// Kalıcı kayıt (en iyi skor, açılan bölüm). WebGL'de tarayıcının IndexedDB'sine yazılır,
    /// yani öğrenci sekmeyi kapatsa da skoru durur.
    /// </summary>
    public static class Kayit
    {
        static string Anahtar(string oyun, string alan) => $"ulu.{oyun}.{alan}";

        public static int EnIyi(string oyun) => PlayerPrefs.GetInt(Anahtar(oyun, "eniyi"), 0);

        /// <summary>Skor rekoru kırdıysa kaydeder ve true döner.</summary>
        public static bool EnIyiDene(string oyun, int skor)
        {
            if (skor <= EnIyi(oyun)) return false;
            PlayerPrefs.SetInt(Anahtar(oyun, "eniyi"), skor);
            PlayerPrefs.Save();
            return true;
        }

        public static int Bolum(string oyun) => PlayerPrefs.GetInt(Anahtar(oyun, "bolum"), 1);

        public static void BolumYaz(string oyun, int bolum)
        {
            if (bolum <= Bolum(oyun)) return;
            PlayerPrefs.SetInt(Anahtar(oyun, "bolum"), bolum);
            PlayerPrefs.Save();
        }

        public static int Sayi(string oyun, string alan, int varsayilan = 0)
            => PlayerPrefs.GetInt(Anahtar(oyun, alan), varsayilan);

        public static void SayiYaz(string oyun, string alan, int deger)
        {
            PlayerPrefs.SetInt(Anahtar(oyun, alan), deger);
            PlayerPrefs.Save();
        }

        public static void Sil(string oyun)
        {
            PlayerPrefs.DeleteKey(Anahtar(oyun, "eniyi"));
            PlayerPrefs.DeleteKey(Anahtar(oyun, "bolum"));
            PlayerPrefs.Save();
        }
    }
}
