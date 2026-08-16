using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Ulu.Duzenleyici
{
    /// <summary>
    /// Komut satırından (batchmode) çağrılan işler: derleme kontrolü ve WebGL derlemesi.
    /// Editor arayüzünde hiçbir düğmeye basmadan çalışır; tools/*.ps1 bunları çağırır.
    /// </summary>
    public static class YapiCLI
    {
        // --- tools/derle.ps1 ---
        public static void DerlemeKontrol()
        {
            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError("[ULU] Derleme HATALI — yukarıdaki 'error CS' satırlarına bak.");
                EditorApplication.Exit(1);
                return;
            }

            int oyunSayisi = OyunlariBul().Length;
            Debug.Log($"[ULU] Derleme tamam. Bulunan oyun: {oyunSayisi}");
            EditorApplication.Exit(0);
        }

        // --- tools/yayinla.ps1 ve tools/dene.ps1 ---
        public static void WebGLYap()
        {
            string oyun = Arg("-oyun");
            if (string.IsNullOrEmpty(oyun)) { Hata("-oyun <OyunAdi> verilmedi."); return; }

            string sahne = $"Assets/Oyunlar/{oyun}/{oyun}.unity";
            if (!File.Exists(sahne)) { Hata($"Sahne yok: {sahne} — önce tools/yeni-oyun.ps1 çalıştır."); return; }

            string cikti = Arg("-cikti");
            if (string.IsNullOrEmpty(cikti))
                cikti = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "docs", Slug(oyun)));

            if (Directory.Exists(cikti)) Directory.Delete(cikti, true);
            Directory.CreateDirectory(cikti);

            WebAyarlari(oyun);

            var secenekler = new BuildPlayerOptions
            {
                scenes = new[] { sahne },
                locationPathName = cikti,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None
            };

            Debug.Log($"[ULU] WebGL derlemesi başlıyor: {oyun} → {cikti}");
            BuildReport rapor = BuildPipeline.BuildPlayer(secenekler);
            var ozet = rapor.summary;

            if (ozet.result == BuildResult.Succeeded)
            {
                Debug.Log($"[ULU] Derleme başarılı: {ozet.totalSize / (1024 * 1024)} MB, süre {ozet.totalTime}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[ULU] Derleme başarısız: {ozet.result}, hata sayısı {ozet.totalErrors}");
                EditorApplication.Exit(1);
            }
        }

        static void WebAyarlari(string oyun)
        {
            PlayerSettings.productName = oyun;
            PlayerSettings.companyName = "Yasin Ulu";
            PlayerSettings.runInBackground = true;
            PlayerSettings.defaultWebScreenWidth = 960;
            PlayerSettings.defaultWebScreenHeight = 600;

            // GitHub Pages özel HTTP başlığı veremez; sıkıştırılmış dosyayı tarayıcıya
            // açtırmak için "decompression fallback" şart, yoksa oyun boş ekran açar.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            PlayerSettings.stripEngineCode = true;
        }

        // --- yardımcılar ---

        /// <summary>Assets/Oyunlar altındaki sahnesi olan oyunların adları.</summary>
        public static string[] OyunlariBul()
        {
            string kok = Path.Combine(Application.dataPath, "Oyunlar");
            if (!Directory.Exists(kok)) return Array.Empty<string>();
            return Directory.GetDirectories(kok)
                .Select(Path.GetFileName)
                .Where(ad => File.Exists(Path.Combine(kok, ad, ad + ".unity")))
                .OrderBy(ad => ad)
                .ToArray();
        }

        public static string Slug(string ad)
        {
            var eslesme = new (char, string)[]
            {
                ('ç', "c"), ('Ç', "c"), ('ğ', "g"), ('Ğ', "g"), ('ı', "i"), ('İ', "i"),
                ('ö', "o"), ('Ö', "o"), ('ş', "s"), ('Ş', "s"), ('ü', "u"), ('Ü', "u")
            };
            string s = ad;
            foreach (var (k, v) in eslesme) s = s.Replace(k.ToString(), v);
            s = s.ToLowerInvariant();
            var temiz = new System.Text.StringBuilder();
            foreach (char c in s)
            {
                if (char.IsLetterOrDigit(c)) temiz.Append(c);
                else if (temiz.Length > 0 && temiz[temiz.Length - 1] != '-') temiz.Append('-');
            }
            return temiz.ToString().Trim('-');
        }

        public static string Arg(string ad)
        {
            var argumanlar = Environment.GetCommandLineArgs();
            for (int i = 0; i < argumanlar.Length - 1; i++)
                if (argumanlar[i] == ad) return argumanlar[i + 1];
            return null;
        }

        static void Hata(string mesaj)
        {
            Debug.LogError("[ULU] " + mesaj);
            EditorApplication.Exit(1);
        }
    }
}
