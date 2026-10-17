using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Economy;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Wallet (7.1): başlangıç değerleri, ekleme, harcama, karşılayabilme ve değişim olayları.</summary>
    public class WalletTests
    {
        private Wallet _wallet;
        private List<(int Value, int Delta)> _moneyChanges;
        private List<(int Value, int Delta)> _fuelChanges;

        [SetUp]
        public void SetUp()
        {
            _wallet = new Wallet(300, 100);
            _moneyChanges = new List<(int, int)>();
            _fuelChanges = new List<(int, int)>();
            _wallet.MoneyChanged += (value, delta) => _moneyChanges.Add((value, delta));
            _wallet.FuelChanged += (value, delta) => _fuelChanges.Add((value, delta));
        }

        [Test]
        public void New_HasStartingValues()
        {
            Assert.AreEqual(300, _wallet.Money);
            Assert.AreEqual(100, _wallet.Fuel);
            Assert.AreEqual(300, _wallet.Get(WalletResource.Money));
            Assert.AreEqual(100, _wallet.Get(WalletResource.Fuel));
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        public void New_NegativeStart_Throws(int money, int fuel)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Wallet(money, fuel));
        }

        [Test]
        public void Add_Money_IncreasesAndRaisesEvent()
        {
            _wallet.Add(WalletResource.Money, 45);

            Assert.AreEqual(345, _wallet.Money);
            CollectionAssert.AreEqual(new[] { (345, 45) }, _moneyChanges);
            Assert.IsEmpty(_fuelChanges);
        }

        [Test]
        public void Add_Fuel_IncreasesAndRaisesOnlyFuelEvent()
        {
            _wallet.Add(WalletResource.Fuel, 15);

            Assert.AreEqual(115, _wallet.Fuel);
            Assert.AreEqual(300, _wallet.Money);
            CollectionAssert.AreEqual(new[] { (115, 15) }, _fuelChanges);
            Assert.IsEmpty(_moneyChanges);
        }

        [Test]
        public void Add_Zero_DoesNothing()
        {
            _wallet.Add(WalletResource.Money, 0);

            Assert.AreEqual(300, _wallet.Money);
            Assert.IsEmpty(_moneyChanges);
        }

        [Test]
        public void Add_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.Add(WalletResource.Money, -5));
            Assert.AreEqual(300, _wallet.Money);
        }

        [Test]
        public void Add_SaturatesAtIntMax()
        {
            _wallet.Add(WalletResource.Money, int.MaxValue);

            Assert.AreEqual(int.MaxValue, _wallet.Money);
            Assert.AreEqual(int.MaxValue - 300, _moneyChanges[0].Delta);

            _wallet.Add(WalletResource.Money, 10);

            Assert.AreEqual(int.MaxValue, _wallet.Money);
            Assert.AreEqual(1, _moneyChanges.Count, "Doymuş cüzdanda değişim yok, olay da yok.");
        }

        [Test]
        public void TrySpend_Enough_DeductsAndRaisesNegativeDelta()
        {
            bool spent = _wallet.TrySpend(WalletResource.Money, 50);

            Assert.IsTrue(spent);
            Assert.AreEqual(250, _wallet.Money);
            CollectionAssert.AreEqual(new[] { (250, -50) }, _moneyChanges);
        }

        [Test]
        public void TrySpend_ExactAmount_LeavesZero()
        {
            Assert.IsTrue(_wallet.TrySpend(WalletResource.Fuel, 100));

            Assert.AreEqual(0, _wallet.Fuel);
            CollectionAssert.AreEqual(new[] { (0, -100) }, _fuelChanges);
        }

        [Test]
        public void TrySpend_NotEnough_ReturnsFalseAndChangesNothing()
        {
            bool spent = _wallet.TrySpend(WalletResource.Money, 301);

            Assert.IsFalse(spent);
            Assert.AreEqual(300, _wallet.Money);
            Assert.IsEmpty(_moneyChanges);
        }

        [Test]
        public void TrySpend_Zero_ReturnsTrueWithoutEvent()
        {
            Assert.IsTrue(_wallet.TrySpend(WalletResource.Money, 0));

            Assert.AreEqual(300, _wallet.Money);
            Assert.IsEmpty(_moneyChanges);
        }

        [Test]
        public void TrySpend_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.TrySpend(WalletResource.Money, -1));
            Assert.AreEqual(300, _wallet.Money);
        }

        [Test]
        public void TrySpend_Money_DoesNotTouchFuel()
        {
            _wallet.TrySpend(WalletResource.Money, 100);

            Assert.AreEqual(100, _wallet.Fuel);
            Assert.IsEmpty(_fuelChanges);
        }

        [TestCase(0, true)]
        [TestCase(299, true)]
        [TestCase(300, true)]
        [TestCase(301, false)]
        public void CanAfford_ComparesWithCurrent(int amount, bool expected)
        {
            Assert.AreEqual(expected, _wallet.CanAfford(WalletResource.Money, amount));
        }

        [Test]
        public void CanAfford_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.CanAfford(WalletResource.Fuel, -1));
        }

        [Test]
        public void InvalidResource_Throws()
        {
            var invalid = (WalletResource)99;

            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.Get(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.Add(invalid, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.TrySpend(invalid, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.GetCapacity(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.SetCapacity(invalid, 1));
        }

        // --- Kapasite (9.2: yakıt kapasitesi 150, jeneratör ayarlar) ---

        [Test]
        public void Capacity_DefaultsToUnlimited()
        {
            Assert.AreEqual(int.MaxValue, _wallet.GetCapacity(WalletResource.Money));
            Assert.AreEqual(int.MaxValue, _wallet.GetCapacity(WalletResource.Fuel));
        }

        [Test]
        public void Add_OverCapacity_SaturatesAtCapacity()
        {
            _wallet.SetCapacity(WalletResource.Fuel, 150);

            _wallet.Add(WalletResource.Fuel, 70);

            Assert.AreEqual(150, _wallet.Fuel);
            CollectionAssert.AreEqual(new[] { (150, 50) }, _fuelChanges, "Değişim gerçekte eklenen kadar.");
        }

        [Test]
        public void Add_WhenFull_DoesNothingWithoutEvent()
        {
            _wallet.SetCapacity(WalletResource.Fuel, 100);

            _wallet.Add(WalletResource.Fuel, 10);

            Assert.AreEqual(100, _wallet.Fuel);
            Assert.IsEmpty(_fuelChanges);
        }

        [Test]
        public void SetCapacity_BelowCurrent_ClampsWithEvent()
        {
            _wallet.SetCapacity(WalletResource.Fuel, 60);

            Assert.AreEqual(60, _wallet.GetCapacity(WalletResource.Fuel));
            Assert.AreEqual(60, _wallet.Fuel);
            CollectionAssert.AreEqual(new[] { (60, -40) }, _fuelChanges);
        }

        [Test]
        public void SetCapacity_AboveCurrent_KeepsAmountWithoutEvent()
        {
            _wallet.SetCapacity(WalletResource.Fuel, 150);

            Assert.AreEqual(100, _wallet.Fuel);
            Assert.IsEmpty(_fuelChanges);
            Assert.AreEqual(int.MaxValue, _wallet.GetCapacity(WalletResource.Money), "Diğer kaynak etkilenmez.");
        }

        [Test]
        public void SetCapacity_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.SetCapacity(WalletResource.Fuel, -1));
            Assert.AreEqual(int.MaxValue, _wallet.GetCapacity(WalletResource.Fuel));
        }
    }
}
