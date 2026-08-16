using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ulu.Duzenleyici
{
    /// <summary>
    /// Yeni mini oyunun klasörünü, iskelet kodunu ve sahnesini üretir — hepsi komut satırından.
    /// İki aşamalıdır: önce kod yazılır (Unity derlesin diye), sonra ikinci çağrıda
    /// derlenmiş tipe göre sahne oluşturulur. tools/yeni-oyun.ps1 ikisini sırayla çağırır.
    /// </summary>
    public static class OyunIskelesi
    {
        // --- 1. aşama: dosyalar ---
        public static void YeniOyunDosyalari()
        {
            string oyun = YapiCLI.Arg("-oyun");
            if (string.IsNullOrEmpty(oyun)) { Cik("-oyun <OyunAdi> verilmedi.", 1); return; }

            string klasor = Path.Combine(Application.dataPath, "Oyunlar", oyun);
            Directory.CreateDirectory(klasor);

            string kodYolu = Path.Combine(klasor, oyun + "Bootstrap.cs");
            if (File.Exists(kodYolu))
            {
                Debug.Log($"[ULU] {oyun} zaten var, kod korundu.");
            }
            else
            {
                File.WriteAllText(kodYolu, Sablon(oyun), new System.Text.UTF8Encoding(false));
                Debug.Log($"[ULU] Kod yazıldı: {kodYolu}");
            }

            string notYolu = Path.Combine(klasor, "OYUN.md");
            if (!File.Exists(notYolu))
                File.WriteAllText(notYolu, NotSablonu(oyun), new System.Text.UTF8Encoding(false));

            AssetDatabase.Refresh();
            Cik($"{oyun} dosyaları hazır.", 0);
        }

        // --- 2. aşama: sahne ---
        public static void SahneUret()
        {
            string oyun = YapiCLI.Arg("-oyun");
            if (string.IsNullOrEmpty(oyun)) { Cik("-oyun <OyunAdi> verilmedi.", 1); return; }

            if (EditorUtility.scriptCompilationFailed) { Cik("Derleme hatası var, sahne üretilmedi.", 1); return; }

            string tipAdi = oyun + "Bootstrap";
            var tip = TypeCache.GetTypesDerivedFrom<OyunTabani>().FirstOrDefault(t => t.Name == tipAdi);
            if (tip == null) { Cik($"{tipAdi} sınıfı bulunamadı (OyunTabani'ndan türemeli).", 1); return; }

            string klasor = $"Assets/Oyunlar/{oyun}";
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Oyunlar", oyun));
            string sahneYolu = $"{klasor}/{oyun}.unity";

            var sahne = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject(oyun);
            go.AddComponent(tip);
            EditorSceneManager.MarkSceneDirty(sahne);
            EditorSceneManager.SaveScene(sahne, sahneYolu);

            SahneListesiniTazele();
            AssetDatabase.SaveAssets();
            Cik($"Sahne hazır: {sahneYolu}", 0);
        }

        [MenuItem("Ulu/Sahne Listesini Tazele")]
        public static void SahneListesiniTazele()
        {
            var sahneler = YapiCLI.OyunlariBul()
                .Select(ad => new EditorBuildSettingsScene($"Assets/Oyunlar/{ad}/{ad}.unity", true))
                .ToArray();
            EditorBuildSettings.scenes = sahneler;
            Debug.Log($"[ULU] Derleme listesinde {sahneler.Length} sahne var.");
        }

        static void Cik(string mesaj, int kod)
        {
            if (kod == 0) Debug.Log("[ULU] " + mesaj); else Debug.LogError("[ULU] " + mesaj);
            EditorApplication.Exit(kod);
        }

        static string Sablon(string oyun) => @"using UnityEngine;
using Ulu;

/// <summary>
/// @AD@ — sahnedeki tek nesnenin üzerindeki tek script. Her şeyi kod kurar.
/// </summary>
public class @AD@Bootstrap : OyunTabani
{
    protected override string OyunAdi => ""@AD@"";
    protected override string NasilOynanir => ""Nasıl oynanacağını buraya yaz"";
    protected override Color ArkaPlan => Renk.Gece;
    protected override float KameraBoyu => 5f;

    SpriteRenderer oyuncu;

    // Bir kez çalışır: kalıcı dekor (ebeveyn olarak Dekor kullan).
    protected override void Kur()
    {
    }

    // Her turun başında çalışır: Alan temizlenmiş olarak gelir.
    protected override void Basla()
    {
        oyuncu = Nesne(""Oyuncu"", Cizim.Daire(Renk.Turkuaz, 64), new Vector3(0f, -3f, 0f));
    }

    // Oyun oynanırken her karede çalışır.
    protected override void Oyna(float dt)
    {
        oyuncu.transform.position += new Vector3(Girdi.Yatay, 0f, 0f) * 6f * dt;
    }
}
".Replace("@AD@", oyun);

        static string NotSablonu(string oyun) => $@"# {oyun}

**Mekanik:** (tek cümle)

**Kontroller:** (klavye / dokunma)

**Durum:** iskelet

**Açık işler:**
- [ ] oynanışı kur
";
    }
}
