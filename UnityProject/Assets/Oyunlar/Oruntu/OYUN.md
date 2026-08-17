# Örüntü

**Mekanik:** Ekrandaki karo dizisinde bir karo eksiktir. Öğrenci diziyi yöneten kuralı çözüp
dört seçenekten doğrusunu seçer. Karoların beş özelliği var: **şekil, renk, boyut, dönme, adet**.
Her soruda bunların bir kısmı bir kurala göre değişir (sırayla ilerleme, dönüşümlü, sabit).

**Kontroller:** 1-4 tuşları veya seçeneğe dokunma · seçim tahtasında 1-9 bölüm seçer ·
**Q / Backspace** ya da sol üstteki "Tahta" düğmesi bölümü bırakıp seçim tahtasına döner ·
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

Bu tablo **tabandır**; bölüm içinde zorluk öğrenciye uyar (`OruntuAyar.KaydirmayiGuncelle`):
üst üste 2 doğruda değişen özellik sayısı bir artar, her hatada bir azalır. Sapma tabanın
bir üstü/bir altıyla sınırlı, yani bölüm kimliğini kaybetmez. Ekranda yalnızca "zorluk: yüksek /
düşük" yazar — kaç özellik değiştiğini söylemek soruyu ele verirdi. Öğretmen modunda kapalı.

## Çalışma kâğıdı

Aynı kural motorundan yazdırılabilir kâğıt üretilir — sorular oyundakiyle aynı üretimden
(`OruntuAyar.SoruUret`) geçer, ikinci bir kural motoru yok:

```
.\tools\oruntu-kagit.ps1 -Bolum 7 -Soru 16          # HTML üretir ve tarayıcıda açar
.\tools\oruntu-kagit.ps1 -Renksiz                   # siyah-beyaz baskı: renk özelliği kullanılmaz
.\tools\oruntu-kagit.ps1 -Tohum 42                  # aynı tohum aynı kâğıdı verir (sınıfa dağıtılan nüsha)
```

Çıktı `kagitlar\` altına HTML olarak yazılır (şekiller SVG), tarayıcıdan Ctrl+P ile A4'e ya da
PDF'e basılır. Cevap anahtarı ayrı sayfada, her sorunun kuralı yazılı. Unity Editor açıkken
komut yerine `Ulu > Örüntü — Çalışma Kâğıdı` menüsü kullanılır.

Siyah-beyaz baskıda mor/mavi/pembe aynı griye düştüğü için `-Renksiz` bayrağı rengin değişmediği
sorular seçer; renkli baskıda gerek yok.

## Eğitsel tasarım notları

- Çeldiriciler rastgele değil: biri genellikle **bir önceki karonun** değerini taşır (ileri
  seviyede), diğerleri tek adım kayar. "Gözüne hoş geleni seç" işe yaramaz.
- Her cevaptan sonra kural yazıyla açıklanır ("renk: sırayla bir ileri (mavi → mor → pembe)").
  Doğruda 1.1 sn, yanlışta 3.2 sn ekranda kalır.
- **Yanlışta "neden yanlış?":** seçilen karonun altına uymayan özelliklerin adı yazılır
  ("renk uymuyor"), doğrunun altına "doğrusu", alt satırda da sapmanın kendisi
  ("Seçtiğin: renk sarı yerine mor"). Kuralı tekrar okumak yerine öğrenci kendi hatasını görür.
  Karşılaştırma ham değerle değil **görünen** değerle yapılır (`OruntuAyar.Farklar`), yoksa
  "dönme uymuyor" diyip ekranda hiçbir fark göstermeyen bir açıklama çıkabilirdi.
- **Çözülebilirlik güvencesi:** bir karonun ekranda göründüğü hâli özetleyen "görsel anahtar"
  var (dönme, şeklin simetrisine göre sadeleşir: kare 90°'de kendine eşit, daire/halka her açıda).
  Çeldiriciler bu anahtarla karşılaştırılır, yani ekranda aynı görünen iki seçenek üretilemez.
  Dönme aktifken şekil yalnızca üçgen ↔ yıldız arasında hareket eder.
- Bu güvenceler `Assets/Testler/OruntuTestleri.cs` içinde binlerce rastgele soru üzerinde
  test edilir (81 test) — bu hata sınıfını elle yakalamak zor. Zorluk kaydırmalı üretim de
  aynı testten geçer, yani uyarlanır zorluk çözülemez soru üretemez.

**Durum:** sınıf sürümü — açık iş yok

**Sonraki adımlar için fikirler:**
- [ ] Oturum raporu: bölüm sonundaki dökümü öğretmenin kaydedebileceği bir özet ekrana toplama
- [ ] Kâğıt üzerinde iki sütunlu düzen (bir sayfaya daha çok soru)
- [ ] Öğretmen modunda çeldiriciler de tek özellikle sınırlansın (şu an dar özellik uzayında
      renk üzerinden tamamlanıyor)
