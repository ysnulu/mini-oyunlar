using System.Collections.Generic;
using NUnit.Framework;

namespace Ulu.Testler
{
    /// <summary>
    /// Örüntü'nün çözülebilirlik güvenceleri. Buradaki testler oyun oynanmadan,
    /// yüzlerce rastgele soru üreterek "ekranda ayırt edilemeyen seçenek" gibi
    /// hataları yakalar — bu sınıf hata elle test edilerek zor bulunuyor.
    /// </summary>
    public class OruntuTestleri
    {
        [Test]
        public void Secenekler_EkrandaAyirtEdilebilir([Range(1, 12)] int bolum, [Values(-1, 0, 1)] int kaydirma)
        {
            for (int deneme = 0; deneme < 60; deneme++)
            {
                var soru = OruntuAyar.SoruUret(bolum, kaydirma);
                Assert.AreEqual(4, soru.secenekler.Length, $"Bölüm {bolum}: seçenek sayısı 4 olmalı.");
                Assert.IsFalse(OruntuAyar.AyirtEdilemezVar(soru.secenekler),
                    $"Bölüm {bolum} (kaydırma {kaydirma}): iki seçenek ekranda aynı görünüyor (deneme {deneme}).");
                Assert.GreaterOrEqual(soru.dogru, 0, "Doğru seçenek listede bulunamadı.");
            }
        }

        [Test]
        public void Dizi_GercektenDegisiyor([Range(1, 12)] int bolum)
        {
            for (int deneme = 0; deneme < 80; deneme++)
            {
                var soru = OruntuAyar.SoruUret(bolum);
                Assert.AreNotEqual(OruntuAyar.GorselAnahtar(soru.dizi[0]), OruntuAyar.GorselAnahtar(soru.dizi[1]),
                    $"Bölüm {bolum}: ilk iki karo aynı görünüyor, örüntü okunamaz (deneme {deneme}).");
            }
        }

        [Test]
        public void Donme_Aktifken_SekilAsimetrikKalir()
        {
            for (int deneme = 0; deneme < 300; deneme++)
            {
                var aktif = new[] { OruntuAyar.Ozellik.Donme, OruntuAyar.Ozellik.Sekil };
                var kurallar = OruntuAyar.KurallariUret(aktif);
                for (int i = 0; i < 6; i++)
                {
                    var o = OruntuAyar.OgeUret(kurallar, i);
                    Assert.IsFalse(OruntuAyar.DonmeGorunmez(o.sekil),
                        "Dönme aktifken daire/halka kullanılamaz: dönme görünmez olur.");
                }
            }
        }

        [Test]
        public void Bolum_ZorlukSirasi_Artiyor()
        {
            var ilk = OruntuAyar.Bolum(1);
            var son = OruntuAyar.Bolum(OruntuAyar.ToplamBolum);
            Assert.Less(ilk.ozellikSayisi, son.ozellikSayisi);
            Assert.IsFalse(ilk.ortadanSor);
            Assert.IsTrue(son.ortadanSor);
            Assert.IsFalse(ilk.inceCeldirici);
            Assert.IsTrue(son.inceCeldirici);
        }

        // --- "neden yanlış?" ---

        [Test]
        public void HerYanlisSecenegin_Farki_Anlatilabiliyor([Range(1, 12)] int bolum)
        {
            for (int deneme = 0; deneme < 40; deneme++)
            {
                var soru = OruntuAyar.SoruUret(bolum);
                var dogru = soru.secenekler[soru.dogru];

                for (int i = 0; i < soru.secenekler.Length; i++)
                {
                    if (i == soru.dogru) continue;

                    var farklar = OruntuAyar.Farklar(soru.secenekler[i], dogru);
                    Assert.IsNotEmpty(farklar,
                        $"Bölüm {bolum}: yanlış seçenek doğrudan görünür bir farkla ayrılmalı (deneme {deneme}).");
                    Assert.IsNotEmpty(OruntuAyar.FarkAciklamasi(soru.secenekler[i], dogru),
                        "Fark açıklaması boş kalmamalı.");
                }

                Assert.IsEmpty(OruntuAyar.Farklar(dogru, dogru), "Doğru seçeneğin kendisiyle farkı olamaz.");
            }
        }

        // --- uyarlanır zorluk ---

        [Test]
        public void OzellikSayisi_SinirlarIcinde([Range(1, 12)] int bolum)
        {
            int taban = OruntuAyar.Bolum(bolum).ozellikSayisi;

            Assert.AreEqual(taban, OruntuAyar.OzellikSayisi(bolum, 0));
            Assert.LessOrEqual(OruntuAyar.OzellikSayisi(bolum, 5), 5, "Beş özelliğin üstüne çıkılamaz.");
            Assert.GreaterOrEqual(OruntuAyar.OzellikSayisi(bolum, -5), 1, "En az bir özellik değişmeli.");
            Assert.LessOrEqual(OruntuAyar.OzellikSayisi(bolum, 5) - taban, OruntuAyar.UyumEnCok,
                "Kaydırma bölüm tabanından fazla uzaklaşmamalı.");
        }

        [Test]
        public void Kaydirma_UstUsteDogruda_Artar_HatadaDuser()
        {
            int kaydirma = 0, ustuste = 0;

            // Eşik dolmadan artmaz.
            for (int i = 0; i < OruntuAyar.UyumEsigi - 1; i++)
                kaydirma = OruntuAyar.KaydirmayiGuncelle(kaydirma, ref ustuste, true);
            Assert.AreEqual(0, kaydirma, "Eşiğe ulaşmadan zorluk artmamalı.");

            kaydirma = OruntuAyar.KaydirmayiGuncelle(kaydirma, ref ustuste, true);
            Assert.AreEqual(OruntuAyar.UyumEnCok, kaydirma, "Eşik dolunca zorluk artmalı.");
            Assert.AreEqual(0, ustuste, "Artıştan sonra seri sıfırlanmalı.");

            // Tavanı aşmaz.
            for (int i = 0; i < 10; i++) kaydirma = OruntuAyar.KaydirmayiGuncelle(kaydirma, ref ustuste, true);
            Assert.AreEqual(OruntuAyar.UyumEnCok, kaydirma);

            // Tek hata bile düşürür, seriyi sıfırlar.
            kaydirma = OruntuAyar.KaydirmayiGuncelle(kaydirma, ref ustuste, false);
            Assert.AreEqual(OruntuAyar.UyumEnCok - 1, kaydirma);
            Assert.AreEqual(0, ustuste);

            // Tabanı aşmaz.
            for (int i = 0; i < 10; i++) kaydirma = OruntuAyar.KaydirmayiGuncelle(kaydirma, ref ustuste, false);
            Assert.AreEqual(OruntuAyar.UyumEnAz, kaydirma);
        }

        // --- öğretmen modu ---

        [Test]
        public void OgretmenModu_YalnizcaSecilenOzellikDegisir()
        {
            foreach (OruntuAyar.Ozellik odak in System.Enum.GetValues(typeof(OruntuAyar.Ozellik)))
            {
                for (int deneme = 0; deneme < 40; deneme++)
                {
                    var soru = OruntuAyar.SoruUret(6, 0, odak);
                    Assert.AreEqual(1, soru.aktif.Length, "Öğretmen modunda tek özellik değişmeli.");
                    Assert.AreEqual(odak, soru.aktif[0]);
                    Assert.AreEqual(soru.dizi.Length - 1, soru.gizli,
                        "Öğretmen modunda eksik karo hep sonda olmalı (alıştırma sadeleşsin).");
                }
            }
        }
    }
}
