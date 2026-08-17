using System.Collections.Generic;
using NUnit.Framework;
using Oge = OruntuAyar.Oge;

namespace Ulu.Testler
{
    /// <summary>
    /// Örüntü'nün çözülebilirlik güvenceleri. Buradaki testler oyun oynanmadan,
    /// yüzlerce rastgele soru üreterek "ekranda ayırt edilemeyen seçenek" gibi
    /// hataları yakalar — bu sınıf hata elle test edilerek zor bulunuyor.
    /// </summary>
    public class OruntuTestleri
    {
        static (List<Oge> secenekler, Oge dogru, Oge[] dizi) Soru(int bolum)
        {
            var ayar = OruntuAyar.Bolum(bolum);
            var aktif = OruntuAyar.AktifOzellikler(bolum);
            var kurallar = OruntuAyar.KurallariUret(aktif);

            var dizi = new Oge[ayar.diziUzunlugu];
            for (int i = 0; i < dizi.Length; i++) dizi[i] = OruntuAyar.OgeUret(kurallar, i);

            int gizli = ayar.ortadanSor ? UnityEngine.Random.Range(1, dizi.Length - 1) : dizi.Length - 1;
            var dogru = dizi[gizli];
            var onceki = dizi[gizli > 0 ? gizli - 1 : 0];

            var liste = new List<Oge>(OruntuAyar.Celdiriciler(dogru, onceki, aktif, ayar.inceCeldirici, kurallar))
            {
                dogru
            };
            return (liste, dogru, dizi);
        }

        [Test]
        public void Secenekler_EkrandaAyirtEdilebilir([Range(1, 12)] int bolum)
        {
            for (int deneme = 0; deneme < 80; deneme++)
            {
                var (secenekler, _, _) = Soru(bolum);
                Assert.AreEqual(4, secenekler.Count, $"Bölüm {bolum}: seçenek sayısı 4 olmalı.");
                Assert.IsFalse(OruntuAyar.AyirtEdilemezVar(secenekler),
                    $"Bölüm {bolum}: iki seçenek ekranda aynı görünüyor (deneme {deneme}).");
            }
        }

        [Test]
        public void Dizi_GercektenDegisiyor([Range(1, 12)] int bolum)
        {
            for (int deneme = 0; deneme < 80; deneme++)
            {
                var (_, _, dizi) = Soru(bolum);
                Assert.AreNotEqual(OruntuAyar.GorselAnahtar(dizi[0]), OruntuAyar.GorselAnahtar(dizi[1]),
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
    }
}
