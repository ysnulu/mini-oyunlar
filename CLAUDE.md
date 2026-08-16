# Mini Oyunlar — çalışma kuralları

Unity ile küçük 2D oyunlar üretip GitHub Pages'te yayınlayan bir üretim hattı.
Amaç: **oyunun tamamı koddan kurulsun**, Editor'de tıklanacak hiçbir adım kalmasın.
Yasin fikir verir ve oynar; kod, sahne, derleme ve yayın bu hattan geçer.

## Temel ilkeler

1. **Kod-öncelikli.** Sahne dosyası tek boş nesne + tek `*Bootstrap` scripti içerir.
   Kamera, oyuncu, düşman, arayüz, ses — hepsi `Kur/Basla/Oyna` içinde koddan kurulur.
   Inspector'da alan bağlamak, prefab sürüklemek yok.
2. **Binary asset yok.** Görseller `Cizim`, sesler `Ses` ile üretilir. Bu kural bilinçli:
   depo okunur kalıyor ve her şeyi Claude yapabiliyor. Bir oyuna gerçek sprite gerekiyorsa
   önce Yasin'e sorulur.
3. **Türkçe isimlendirme.** Sınıf, metot, değişken, klasör Türkçe. Klasör ve tip adları
   ASCII (`Kacis`), oyunun görünen adı Türkçe olabilir (`OyunAdi => "Kaçış"`).
4. **Her oyun tek klasör:** `UnityProject/Assets/Oyunlar/<Ad>/` → `<Ad>Bootstrap.cs`,
   varsa `<Ad>Ayar.cs` (zorluk/ayar sabitleri), `<Ad>.unity`, `OYUN.md`.

## Yapı

```
UnityProject/Assets/Ortak/     Cizim, Ses, Girdi, Arayuz, Kayit, OyunTabani  (asmdef: Ulu.Ortak)
UnityProject/Assets/Oyunlar/   her oyun bir klasör (Assembly-CSharp)
UnityProject/Assets/Editor/    YapiCLI (derleme/WebGL), OyunIskelesi (yeni oyun + sahne)
UnityProject/Assets/Testler/   EditMode testleri (yalnızca Ortak'ı görebilir)
tools/                         PowerShell komutları
docs/                          GitHub Pages kökü: galeri + oyun başına WebGL çıktısı
```

Unity **6000.5.2f1**, 2D URP. Editor yolu: `C:\Program Files\unity\6000.5.2f1\Editor\Unity.exe`.

## Komutlar

| Komut | Ne yapar |
|---|---|
| `.\tools\yeni-oyun.ps1 -Oyun Ad` | Klasör + Bootstrap iskeleti + sahne üretir (iki aşamalı) |
| `.\tools\derle.ps1` | Kodun derlendiğini Editor açmadan doğrular |
| `.\tools\test.ps1` | EditMode testleri |
| `.\tools\web-derle.ps1 -Oyun Ad` | WebGL derlemesi → `docs\<slug>\` |
| `.\tools\dene.ps1 -Oyun Ad [-Derle]` | Yerelde tarayıcıda açar (python http.server) |
| `.\tools\yayinla.ps1 -Oyun Ad -Aciklama "..." -Mesaj "..."` | Derle → galeriye ekle → commit → push |

**Editor açıkken batchmode çalışmaz** (proje kilidi). İki mod:
- **Editor açık:** yalnızca `.cs` yaz; Unity kendi derler, Yasin Play'e basar. Hataları
  `%LOCALAPPDATA%\Unity\Editor\Editor.log` dosyasından okuyabilirsin.
- **Editor kapalı:** `derle / test / yeni-oyun / web-derle / yayinla`.

## Bir oyun eklerken

1. `.\tools\yeni-oyun.ps1 -Oyun Ad`
2. `<Ad>Bootstrap.cs` içini yaz; ayarlanabilir sayıları `<Ad>Ayar.cs` içine topla.
3. `.\tools\derle.ps1` — temiz geçmeden ilerleme.
4. Oynanış denemesi: Yasin Editor'de Play, ya da `.\tools\dene.ps1 -Oyun Ad -Derle`.
5. Geri bildirim turları → `<Ad>Ayar.cs` üzerinden ayar.
6. `.\tools\yayinla.ps1 -Oyun Ad -Aciklama "tek cümle"`.
7. `OYUN.md` güncelle (mekanik, kontroller, açık işler).

## Bilinmesi gerekenler

- **WebGL + Pages:** `PlayerSettings.WebGL.decompressionFallback = true` şart (Pages özel HTTP
  başlığı veremez). Boş ekran gelirse `compressionFormat = Disabled` ile yeniden derle.
- **TextMeshPro kullanılmıyor.** Arayüz legacy `UnityEngine.UI.Text` + built-in
  `LegacyRuntime.ttf` ile kuruluyor; böylece "TMP Essentials" içe aktarma adımı hiç çıkmıyor.
- **Girdi** legacy Input üzerinden (`activeInputHandler: 2`). Dokunma, fare olaylarına düşer;
  ekranın sol/sağ yarısına dokunma `Girdi.SolaBasildi/SagaBasildi` içinde hazır.
- **Testler** yalnızca `Ulu.Ortak` içindekileri görebilir (asmdef sınırı). Oyuna özel mantığı
  test etmek gerekirse o oyun klasörüne kendi asmdef'i eklenir.
- **docs/ büyür:** her WebGL çıktısı ~5–15 MB ve git geçmişinde kalır. Bu yüzden `docs/`
  yalnızca yayın anlarında commit edilir, her denemede değil.
- **Lisans:** batchmode "No valid Unity Editor license" derse Unity Hub → Preferences →
  Licenses'tan kişisel lisans yenilenmeli. Kodla çözülemez.
