# Kaçış

**Mekanik:** 3 şeritli sonsuz kaçış. Engeller yukarıdan iner, oyuncu şerit değiştirerek kaçar.
Geçilen her engel 1, toplanan yıldız 5 puan. Hız ve engel sıklığı sürekli artar.

**Kontroller:** A/D veya ← → · telefonda ekranın soluna/sağına dokun · Boşluk başlat · R yeniden · Esc duraklat

**Zorluk:** `KacisAyar.cs` — hız 5.5 → 15, engel mesafesi 5.0 → 2.9 birim (60 saniyede), çift engel
şansı 15. saniyeden sonra 0 → 0.55. Oynanış hissi bu dosyadan ayarlanır.

**Durum:** ilk sürüm

**Açık işler:**
- [ ] Hızlanma anlarında kısa ekran titremesi / iz efekti
- [ ] 30 saniyede bir "kalkan" toplanabilir
- [ ] Renk temasının seviyeye göre değişmesi
