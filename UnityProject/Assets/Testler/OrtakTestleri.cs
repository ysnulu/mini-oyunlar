using NUnit.Framework;
using UnityEngine;
using Ulu;

namespace Ulu.Testler
{
    public class CizimTestleri
    {
        [Test]
        public void Daire_IstenenBoyuttaSpriteUretir()
        {
            var s = Cizim.Daire(Renk.Turkuaz, 32);
            Assert.IsNotNull(s);
            Assert.AreEqual(32, s.texture.width);
            Assert.AreEqual(32, s.texture.height);
        }

        [Test]
        public void AyniIstek_AyniSpriteyiDonduru()
        {
            var a = Cizim.Kare(Renk.Sari, 16);
            var b = Cizim.Kare(Renk.Sari, 16);
            Assert.AreSame(a, b, "Aynı sprite önbellekten gelmeli, her istekte yeniden üretilmemeli.");
        }

        [Test]
        public void Daire_KoseleriSaydamMerkeziDolu()
        {
            var s = Cizim.Daire(Color.white, 32);
            var doku = s.texture;
            Assert.Less(doku.GetPixel(0, 0).a, 0.1f, "Köşe saydam olmalı.");
            Assert.Greater(doku.GetPixel(16, 16).a, 0.9f, "Merkez dolu olmalı.");
        }
    }

    public class KayitTestleri
    {
        const string Oyun = "TestOyunu";

        [SetUp]
        public void Temizle() => Kayit.Sil(Oyun);

        [TearDown]
        public void Topla() => Kayit.Sil(Oyun);

        [Test]
        public void EnIyi_YalnizcaRekorKirildigindaGuncellenir()
        {
            Assert.AreEqual(0, Kayit.EnIyi(Oyun));

            Assert.IsTrue(Kayit.EnIyiDene(Oyun, 120));
            Assert.AreEqual(120, Kayit.EnIyi(Oyun));

            Assert.IsFalse(Kayit.EnIyiDene(Oyun, 90), "Daha düşük skor rekoru bozmamalı.");
            Assert.AreEqual(120, Kayit.EnIyi(Oyun));

            Assert.IsTrue(Kayit.EnIyiDene(Oyun, 121));
            Assert.AreEqual(121, Kayit.EnIyi(Oyun));
        }

        [Test]
        public void Bolum_GeriyeGitmez()
        {
            Kayit.BolumYaz(Oyun, 3);
            Kayit.BolumYaz(Oyun, 2);
            Assert.AreEqual(3, Kayit.Bolum(Oyun));
        }
    }
}
