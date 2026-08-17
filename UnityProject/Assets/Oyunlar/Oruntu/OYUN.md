# Örüntü

**Mekanik:** Ekrandaki karo dizisinde bir karo eksiktir. Öğrenci diziyi yöneten kuralı çözüp
dört seçenekten doğrusunu seçer. Karoların beş özelliği var: **şekil, renk, boyut, dönme, adet**.
Her soruda bunların bir kısmı bir kurala göre değişir (sırayla ilerleme, dönüşümlü, sabit).

**Kontroller:** 1-4 tuşları veya seçeneğe dokunma · seçim tahtasında 1-9 bölüm seçer ·
Esc duraklat · R oturumu yeniden başlatır

## Sınıf sürümü

Oyun **bölüm seçme tahtası** ile açılır:
- 12 bölüm karosu; açılanlar parlak, kilitliler soluk, altlarında kazanılan yıldızlar.
- Alt sırada **öğretmen modu**: `şekil · renk · boyut · dönme · adet`. Biri seçilince yalnızca
  o özellik değişen 5 soruluk alıştırma gelir — örüntü öğretiminin doğal sırası bu.
  Öğretmen modu yıldız vermez ve bölüm açmaz, sırf çalıştırma amaçlı.
- Üst satırda **oturum dökümü**: "Bu oturum: dönme 4/7 renk 5/5 …"
- Bölüm sonunda **özellik bazlı döküm**: hangi boyutta kaç doğru. Öğrencinin nerede zorlandığını
  görmek için tasarlanan kısım burası; 2.8 sn ekranda kalır, sonra tahtaya döner.

**Bölümler:** 12 bölüm × 5 soru, bölüm başına 3 can. Hatasız bölüm 3 yıldız, 1 hata 2, 2 hata 1.
Açılan bölüm ve yıldızlar kaydedilir; bölüm bitince (başarılı ya da değil) seçim tahtasına dönülür.

**Zorluk basamakları** (`OruntuAyar.Bolum`):
| Bölüm | Aynı anda değişen özellik | Dizi | Eksik karo | Çeldirici |
|---|---|---|---|---|
| 1-2 | 1 | 5 karo | sonda | kaba |
| 3-4 | 2 | 5 karo | sonda | kaba |
| 5-6 | 2 | 6 karo | sonda | ince |
| 7-8 | 3 | 6 karo | sonda | ince |
| 9-10 | 3 | 6 karo | **ortada** | ince |
| 11-12 | 4 | 6 karo | **ortada** | ince |

## Eğitsel tasarım notları

- Çeldiriciler rastgele değil: biri genellikle **bir önceki karonun** değerini taşır (ileri
  seviyede), diğerleri tek adım kayar. "Gözüne hoş geleni seç" işe yaramaz.
- Her cevaptan sonra kural yazıyla açıklanır ("renk: sırayla bir ileri (mavi → mor → pembe)").
  Yanlışta 2.6 sn, doğruda 1.1 sn ekranda kalır.
- **Çözülebilirlik güvencesi:** bir karonun ekranda göründüğü hâli özetleyen "görsel anahtar"
  var (dönme, şeklin simetrisine göre sadeleşir: kare 90°'de kendine eşit, daire/halka her açıda).
  Çeldiriciler bu anahtarla karşılaştırılır, yani ekranda aynı görünen iki seçenek üretilemez.
  Dönme aktifken şekil yalnızca üçgen ↔ yıldız arasında hareket eder.
- Bu güvenceler `Assets/Testler/OruntuTestleri.cs` içinde 12 bölüm × 80 rastgele soru üzerinde
  test edilir — bu hata sınıfını elle yakalamak zor.

**Durum:** sınıf sürümü

**Açık işler:**
- [ ] "Neden yanlış?" — seçilen karonun hangi özelliğinin uymadığını ekranda vurgulama
- [ ] Uyarlanır zorluk: üst üste doğruda özellik sayısını artır, hatada düşür
- [ ] Aynı kural motorundan yazdırılabilir çalışma kâğıdı (PNG/PDF)
- [ ] Bölüm ortasında tahtaya dönme tuşu (şu an bölüm bitene kadar çıkılmıyor)
