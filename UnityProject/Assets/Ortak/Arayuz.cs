using UnityEngine;
using UnityEngine.UI;

namespace Ulu
{
    public enum Kose { SolUst, Ust, SagUst, Sol, Orta, Sag, SolAlt, Alt, SagAlt }

    /// <summary>Kamera ve ekran arayüzünü kodla kurar; sahnede elle kurulacak hiçbir şey kalmaz.</summary>
    public static class Arayuz
    {
        static Font yaziTipi;

        public static Font YaziTipi
        {
            get
            {
                if (yaziTipi == null)
                {
                    yaziTipi = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (yaziTipi == null) yaziTipi = Font.CreateDynamicFontFromOSFont("Arial", 32);
                }
                return yaziTipi;
            }
        }

        public static Camera Kamera(Color arkaPlan, float ortografikBoy = 5f)
        {
            var go = new GameObject("Kamera") { tag = "MainCamera" };
            var kam = go.AddComponent<Camera>();
            kam.orthographic = true;
            kam.orthographicSize = ortografikBoy;
            kam.clearFlags = CameraClearFlags.SolidColor;
            kam.backgroundColor = arkaPlan;
            kam.transform.position = new Vector3(0f, 0f, -10f);
            go.AddComponent<AudioListener>();
            return kam;
        }

        public static Canvas Kanvas(string ad = "Kanvas", Vector2? referansCozunurluk = null)
        {
            var go = new GameObject(ad);
            var kanvas = go.AddComponent<Canvas>();
            kanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var olcek = go.AddComponent<CanvasScaler>();
            olcek.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            olcek.referenceResolution = referansCozunurluk ?? new Vector2(1280f, 720f);
            olcek.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            olcek.matchWidthOrHeight = 0.5f;
            return kanvas;
        }

        public static Text Yazi(Transform ebeveyn, string metin, int punto = 36, Kose kose = Kose.Orta,
                                Vector2 kenarBosluk = default, Color? renk = null, Vector2? kutu = null)
        {
            var go = new GameObject("Yazi");
            go.transform.SetParent(ebeveyn, false);

            var yazi = go.AddComponent<Text>();
            yazi.text = metin;
            yazi.font = YaziTipi;
            yazi.fontSize = punto;
            yazi.color = renk ?? Renk.Beyaz;
            yazi.alignment = Hizalama(kose);
            yazi.horizontalOverflow = HorizontalWrapMode.Overflow;
            yazi.verticalOverflow = VerticalWrapMode.Overflow;
            yazi.raycastTarget = false;

            var rt = go.GetComponent<RectTransform>();
            var nokta = Nokta(kose);
            rt.anchorMin = rt.anchorMax = rt.pivot = nokta;
            rt.sizeDelta = kutu ?? new Vector2(1000f, punto * 1.6f);
            rt.anchoredPosition = new Vector2(
                kenarBosluk.x * (nokta.x < 0.5f ? 1f : nokta.x > 0.5f ? -1f : 1f),
                kenarBosluk.y * (nokta.y < 0.5f ? 1f : nokta.y > 0.5f ? -1f : 1f));
            return yazi;
        }

        /// <summary>Tüm ekranı kaplayan renk perdesi (oyun sonu / menü fonu).</summary>
        public static Image Perde(Transform ebeveyn, Color renk)
        {
            var go = new GameObject("Perde");
            go.transform.SetParent(ebeveyn, false);
            var img = go.AddComponent<Image>();
            img.color = renk;
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.transform.SetAsFirstSibling();
            return img;
        }

        static Vector2 Nokta(Kose k) => k switch
        {
            Kose.SolUst => new Vector2(0f, 1f),
            Kose.Ust => new Vector2(0.5f, 1f),
            Kose.SagUst => new Vector2(1f, 1f),
            Kose.Sol => new Vector2(0f, 0.5f),
            Kose.Sag => new Vector2(1f, 0.5f),
            Kose.SolAlt => new Vector2(0f, 0f),
            Kose.Alt => new Vector2(0.5f, 0f),
            Kose.SagAlt => new Vector2(1f, 0f),
            _ => new Vector2(0.5f, 0.5f)
        };

        static TextAnchor Hizalama(Kose k) => k switch
        {
            Kose.SolUst => TextAnchor.UpperLeft,
            Kose.Ust => TextAnchor.UpperCenter,
            Kose.SagUst => TextAnchor.UpperRight,
            Kose.Sol => TextAnchor.MiddleLeft,
            Kose.Sag => TextAnchor.MiddleRight,
            Kose.SolAlt => TextAnchor.LowerLeft,
            Kose.Alt => TextAnchor.LowerCenter,
            Kose.SagAlt => TextAnchor.LowerRight,
            _ => TextAnchor.MiddleCenter
        };
    }
}
