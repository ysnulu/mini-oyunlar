using UnityEngine;

namespace Ulu
{
    /// <summary>
    /// Klavye + fare + dokunmatiği tek arayüzde toplar; oyunlar platformu bilmez.
    /// WebGL'de telefon tarayıcısı dokunuşu fare olayına çevirir, bu yüzden fare API'si yeterlidir.
    /// Tuket(): durum geçişlerinde (menüden oyuna geçiş gibi) aynı karedeki girdinin
    /// ikinci kez işlenmesini engeller.
    /// </summary>
    public static class Girdi
    {
        static int tuketilenKare = -1;

        static bool Tuketildi => tuketilenKare == Time.frameCount;

        public static void Tuket() { tuketilenKare = Time.frameCount; }

        // --- anlık basışlar ---
        public static bool Onayla =>
            !Tuketildi && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
                           Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0));

        public static bool Yeniden =>
            !Tuketildi && (Input.GetKeyDown(KeyCode.R) || Onayla);

        public static bool DuraklatBasildi =>
            !Tuketildi && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P));

        public static bool SolaBasildi =>
            !Tuketildi && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ||
                           (Input.GetMouseButtonDown(0) && SolYarim));

        public static bool SagaBasildi =>
            !Tuketildi && (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ||
                           (Input.GetMouseButtonDown(0) && !SolYarim));

        public static bool YukariBasildi =>
            !Tuketildi && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow));

        public static bool AsagiBasildi =>
            !Tuketildi && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow));

        // --- sürekli girdi ---
        public static float Yatay
        {
            get
            {
                float d = 0f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) d -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) d += 1f;
                return d;
            }
        }

        public static float Dikey
        {
            get
            {
                float d = 0f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) d -= 1f;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) d += 1f;
                return d;
            }
        }

        public static bool BasiliMi => Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);

        // --- konum ---
        public static Vector2 EkranNoktasi => Input.mousePosition;

        public static bool SolYarim => Input.mousePosition.x < Screen.width * 0.5f;

        public static Vector3 DunyaNoktasi(Camera kamera)
        {
            if (kamera == null) return Vector3.zero;
            var p = kamera.ScreenToWorldPoint(Input.mousePosition);
            p.z = 0f;
            return p;
        }
    }
}
