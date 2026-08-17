using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Ozellik = OruntuAyar.Ozellik;
using Oge = OruntuAyar.Oge;

namespace Ulu.Duzenleyici
{
    /// <summary>
    /// Örüntü'nün kural motorundan yazdırılabilir çalışma kâğıdı üretir: HTML + SVG.
    /// Tarayıcıda açılır, Ctrl+P ile A4'e ya da PDF'e basılır. Sorular oyundakiyle
    /// aynı üretimden geçer (OruntuAyar.SoruUret), yani kâğıt ile ekran aynı örüntüleri
    /// kullanır — kural motorunu ikinci kez yazmak yok.
    /// </summary>
    public static class OruntuKagit
    {
        const int KaroPx = 64;
        const float KaroBirim = 1.6f;   // oyundaki karo genişliği (dünya birimi)

        // --- giriş noktaları ---

        [MenuItem("Ulu/Örüntü — Çalışma Kâğıdı")]
        static void Menuden()
        {
            string yol = Yaz(3, 12, Environment.TickCount, false, null);
            Debug.Log("[ULU] Çalışma kâğıdı: " + yol);
            Application.OpenURL("file:///" + yol.Replace('\\', '/'));
        }

        /// <summary>tools/oruntu-kagit.ps1 — batchmode giriş noktası.</summary>
        public static void Uret()
        {
            try
            {
                int bolum = SayiArg("-bolum", 3);
                int adet = SayiArg("-soru", 12);
                int tohum = SayiArg("-tohum", Environment.TickCount);
                bool renksiz = Array.IndexOf(Environment.GetCommandLineArgs(), "-renksiz") >= 0;

                string yol = Yaz(bolum, adet, tohum, renksiz, YapiCLI.Arg("-cikti"));
                Debug.Log("[ULU] Çalışma kâğıdı: " + yol);
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("[ULU] Çalışma kâğıdı üretilemedi: " + e);
                EditorApplication.Exit(1);
            }
        }

        public static string Yaz(int bolum, int adet, int tohum, bool renksiz, string cikti)
        {
            bolum = Mathf.Clamp(bolum, 1, OruntuAyar.ToplamBolum);
            adet = Mathf.Clamp(adet, 1, 40);
            UnityEngine.Random.InitState(tohum);

            var sorular = new List<OruntuAyar.Soru>();
            for (int i = 0; i < adet; i++) sorular.Add(SoruSec(bolum, renksiz));

            if (string.IsNullOrEmpty(cikti))
            {
                string ad = $"oruntu-b{bolum}{(renksiz ? "-sb" : "")}-{tohum}.html";
                cikti = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "kagitlar", ad));
            }

            Directory.CreateDirectory(Path.GetDirectoryName(cikti));
            File.WriteAllText(cikti, Html(bolum, tohum, renksiz, sorular), new UTF8Encoding(false));
            return cikti;
        }

        /// <summary>
        /// Siyah-beyaz baskıda mor/mavi/pembe aynı griye düşer; -renksiz verildiyse rengin
        /// değişmediği bir soru aranır. Bulunamazsa (dar bölümlerde olabilir) elde ne varsa o.
        /// </summary>
        static OruntuAyar.Soru SoruSec(int bolum, bool renksiz)
        {
            var soru = OruntuAyar.SoruUret(bolum);
            if (!renksiz) return soru;

            for (int deneme = 0; deneme < 200 && Array.IndexOf(soru.aktif, Ozellik.Renk) >= 0; deneme++)
                soru = OruntuAyar.SoruUret(bolum);
            return soru;
        }

        // --- HTML ---

        static string Html(int bolum, int tohum, bool renksiz, List<OruntuAyar.Soru> sorular)
        {
            var s = new StringBuilder();
            s.AppendLine("<!doctype html>");
            s.AppendLine("<html lang=\"tr\"><head><meta charset=\"utf-8\">");
            s.AppendLine("<title>Örüntü — Çalışma Kâğıdı</title>");
            s.AppendLine("<style>" + Stil + "</style>");
            s.AppendLine("</head><body>");

            s.AppendLine("<header><h1>Örüntü — Çalışma Kâğıdı</h1>");
            s.AppendLine("<p class=\"kunye\">Ad soyad: ________________________________   Sınıf: __________   Tarih: ______________</p>");
            s.AppendLine("<p class=\"yonerge\">Her satırdaki karolar bir kurala göre değişiyor. " +
                         "Eksik karonun yerine gelmesi gereken seçeneği işaretle.</p></header>");

            s.AppendLine("<ol class=\"sorular\">");
            foreach (var soru in sorular) s.AppendLine(SoruBlok(soru));
            s.AppendLine("</ol>");

            s.AppendLine("<section class=\"anahtar\"><h2>Cevap anahtarı</h2><ol>");
            foreach (var soru in sorular)
                s.AppendLine($"<li><b>{Harf(soru.dogru)}</b> — {Kacis(soru.Aciklama)}</li>");
            s.AppendLine("</ol></section>");

            s.AppendLine($"<footer>Örüntü · bölüm {bolum} · {sorular.Count} soru{(renksiz ? " · siyah-beyaz baskı için" : "")}" +
                         $" · tohum {tohum} (aynı tohum aynı kâğıdı üretir)</footer>");
            s.AppendLine("</body></html>");
            return s.ToString();
        }

        static string SoruBlok(OruntuAyar.Soru soru)
        {
            var s = new StringBuilder("<li class=\"soru\"><div class=\"dizi\">");

            for (int i = 0; i < soru.dizi.Length; i++)
                s.Append(i == soru.gizli ? BosKaro() : Karo(soru.dizi[i]));

            s.Append("</div><div class=\"secenekler\">");
            for (int i = 0; i < soru.secenekler.Length; i++)
                s.Append($"<div class=\"secenek\"><span class=\"harf\">{Harf(i)}</span>{Karo(soru.secenekler[i])}</div>");

            s.Append("</div></li>");
            return s.ToString();
        }

        static string Harf(int i) => ((char)('A' + i)).ToString();

        // --- SVG karo ---

        static string Karo(Oge o)
        {
            var s = new StringBuilder(SvgBasi());
            s.Append($"<rect x=\"0.5\" y=\"0.5\" width=\"{KaroPx - 1}\" height=\"{KaroPx - 1}\" rx=\"9\" fill=\"#ffffff\" stroke=\"#b9c0d4\"/>");

            // Oyundaki KaroCiz ile aynı yerleşim: karo 1.6 birim, şekiller adede göre küçülür.
            float birim = KaroPx / KaroBirim;
            int adet = o.adet + 1;
            float icOlcek = adet == 1 ? 1f : adet == 2 ? 0.62f : 0.44f;
            float cap = OruntuAyar.BoyutOlcek[Mathf.Clamp(o.boyut, 0, OruntuAyar.BoyutOlcek.Length - 1)] * icOlcek * birim;
            float aralik = (adet == 1 ? 0f : adet == 2 ? 0.68f : 0.47f) * birim;

            for (int i = 0; i < adet; i++)
            {
                float kayma = adet == 1 ? 0f : (i - (adet - 1) * 0.5f) * aralik;
                s.Append(Sekil(o, KaroPx * 0.5f + kayma, KaroPx * 0.5f, cap));
            }

            s.Append("</svg>");
            return s.ToString();
        }

        static string BosKaro()
        {
            return SvgBasi()
                 + $"<rect x=\"0.5\" y=\"0.5\" width=\"{KaroPx - 1}\" height=\"{KaroPx - 1}\" rx=\"9\" fill=\"#ffffff\" "
                 + "stroke=\"#5b647d\" stroke-dasharray=\"5 4\"/>"
                 + $"<text x=\"{KaroPx / 2}\" y=\"{KaroPx / 2 + 9}\" text-anchor=\"middle\" font-size=\"26\" fill=\"#5b647d\">?</text></svg>";
        }

        static string SvgBasi() =>
            $"<svg class=\"karo\" width=\"{KaroPx}\" height=\"{KaroPx}\" viewBox=\"0 0 {KaroPx} {KaroPx}\" xmlns=\"http://www.w3.org/2000/svg\">";

        static string Sekil(Oge o, float cx, float cy, float cap)
        {
            string renk = "#" + ColorUtility.ToHtmlStringRGB(
                OruntuAyar.Palet[Mathf.Clamp(o.renk, 0, OruntuAyar.Palet.Length - 1)]);

            // Unity'de dönme saat yönünün tersi; SVG'de y aşağı baktığı için işaret ters çevrilir.
            float aci = -o.donme * 45f;

            string ic = o.sekil switch
            {
                0 => $"<circle r=\"1\" fill=\"{renk}\"/>",
                1 => $"<rect x=\"-1\" y=\"-1\" width=\"2\" height=\"2\" rx=\"0.18\" fill=\"{renk}\"/>",
                // Cizim.Ucgen ile aynı yön: tepe aşağıda.
                2 => $"<polygon points=\"-1,-1 1,-1 0,1\" fill=\"{renk}\"/>",
                3 => $"<polygon points=\"{YildizNoktalari()}\" fill=\"{renk}\"/>",
                _ => $"<circle r=\"0.83\" fill=\"none\" stroke=\"{renk}\" stroke-width=\"0.34\"/>"
            };

            return $"<g transform=\"translate({S(cx)} {S(cy)}) rotate({S(aci)}) scale({S(cap * 0.5f)})\">{ic}</g>";
        }

        static string YildizNoktalari()
        {
            var s = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                float aci = -Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                float r = i % 2 == 0 ? 1f : 0.45f;
                if (i > 0) s.Append(' ');
                s.Append($"{S(Mathf.Cos(aci) * r)},{S(Mathf.Sin(aci) * r)}");
            }
            return s.ToString();
        }

        /// <summary>SVG sayıları noktalı yazılmalı; Türkçe kültürde virgül çıkarsa dosya bozulur.</summary>
        static string S(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);

        static string Kacis(string metin) =>
            metin.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        static int SayiArg(string ad, int varsayilan)
        {
            string ham = YapiCLI.Arg(ad);
            return int.TryParse(ham, NumberStyles.Integer, CultureInfo.InvariantCulture, out int deger)
                ? deger : varsayilan;
        }

        const string Stil = @"
@page { size: A4; margin: 14mm; }
* { box-sizing: border-box; }
body { font-family: 'Segoe UI', Arial, sans-serif; color: #1b2030; margin: 0; }
h1 { font-size: 20px; margin: 0 0 6px; }
h2 { font-size: 15px; margin: 0 0 8px; }
.kunye { font-size: 12px; color: #4a5268; margin: 0 0 6px; }
.yonerge { font-size: 12px; margin: 0 0 16px; }
ol.sorular { padding-left: 24px; margin: 0; }
li.soru { break-inside: avoid; page-break-inside: avoid; margin: 0 0 14px; padding-bottom: 12px;
          border-bottom: 1px dashed #d5d9e6; }
.dizi { display: flex; gap: 6px; align-items: center; }
.secenekler { display: flex; gap: 18px; margin-top: 8px; padding-left: 8px; }
.secenek { display: flex; align-items: center; gap: 6px; }
.harf { font-size: 12px; font-weight: 600; color: #4a5268; border: 1px solid #b9c0d4;
        border-radius: 50%; width: 19px; height: 19px; display: inline-flex;
        align-items: center; justify-content: center; }
.anahtar { break-before: page; page-break-before: always; font-size: 12px; }
.anahtar ol { padding-left: 22px; }
.anahtar li { margin-bottom: 3px; }
footer { margin-top: 14px; font-size: 10px; color: #7b8299; }
";
    }
}
