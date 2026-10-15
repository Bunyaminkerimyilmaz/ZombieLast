using System;
using LastHaven.Core;
using LastHaven.Economy;
using LastHaven.GameFlow;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Şafak faizi (InterestRule, GDD §10: %10, en fazla 50) ve cüzdana faiz/erken hazır bonusu eklenmesi (WalletIncome).</summary>
    public class WalletIncomeTests
    {
        private static readonly InterestRule GddInterest = new InterestRule(0.1f, 50);

        [TestCase(300, 30)]
        [TestCase(305, 30)]
        [TestCase(499, 49)]
        [TestCase(500, 50)]
        [TestCase(1000, 50)]
        [TestCase(9, 0)]
        [TestCase(0, 0)]
        [TestCase(-20, 0)]
        public void Calculate_IsTenPercentFlooredAndCapped(int money, int expected)
        {
            Assert.AreEqual(expected, GddInterest.Calculate(money));
        }

        [Test]
        public void Calculate_FloatRate_HasNoRoundingLoss()
        {
            // 0.7f float olarak 0.69999999'dur; düz float çarpımı 10 × 0.7 için 6 verirdi.
            Assert.AreEqual(7, new InterestRule(0.7f, 1000).Calculate(10));
            Assert.AreEqual(29, new InterestRule(0.29f, 1000).Calculate(100));
        }

        [Test]
        public void Calculate_HugeMoney_StaysAtCap()
        {
            Assert.AreEqual(50, GddInterest.Calculate(int.MaxValue));
        }

        [Test]
        public void Rule_ClampsRateAndCap()
        {
            var negative = new InterestRule(-0.5f, -10);
            var tooHigh = new InterestRule(3f, 1000);

            Assert.AreEqual(0f, negative.Rate);
            Assert.AreEqual(0, negative.Cap);
            Assert.AreEqual(0, negative.Calculate(1000));
            Assert.AreEqual(1f, tooHigh.Rate);
            Assert.AreEqual(100, tooHigh.Calculate(100));
            Assert.AreEqual(0f, new InterestRule(float.NaN, 50).Rate);
        }

        [Test]
        public void ZeroCap_PaysNothing()
        {
            Assert.AreEqual(0, new InterestRule(0.1f, 0).Calculate(1000));
        }

        [Test]
        public void PayInterest_AddsToWalletAndReturnsAmount()
        {
            var wallet = new Wallet(300, 100);
            var income = new WalletIncome(wallet, GddInterest);
            int lastDelta = 0;
            wallet.MoneyChanged += (_, delta) => lastDelta = delta;

            int paid = income.PayInterest();

            Assert.AreEqual(30, paid);
            Assert.AreEqual(330, wallet.Money);
            Assert.AreEqual(30, lastDelta);
            Assert.AreEqual(100, wallet.Fuel);
        }

        [Test]
        public void PayInterest_UsesCurrentMoney_Capped()
        {
            var wallet = new Wallet(300, 0);
            var income = new WalletIncome(wallet, GddInterest);
            wallet.Add(WalletResource.Money, 900);

            Assert.AreEqual(50, income.PayInterest());
            Assert.AreEqual(1250, wallet.Money);
        }

        [Test]
        public void PayInterest_NoMoney_PaysZeroWithoutEvent()
        {
            var wallet = new Wallet(0, 0);
            var income = new WalletIncome(wallet, GddInterest);
            int events = 0;
            wallet.MoneyChanged += (_, _) => events++;

            Assert.AreEqual(0, income.PayInterest());
            Assert.AreEqual(0, wallet.Money);
            Assert.AreEqual(0, events);
        }

        [Test]
        public void AddEarlyReadyBonus_AddsBonus()
        {
            var wallet = new Wallet(300, 100);
            var income = new WalletIncome(wallet, GddInterest);

            Assert.AreEqual(45, income.AddEarlyReadyBonus(45));
            Assert.AreEqual(345, wallet.Money);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void AddEarlyReadyBonus_ZeroOrNegative_IsIgnored(int bonus)
        {
            var wallet = new Wallet(300, 100);
            var income = new WalletIncome(wallet, GddInterest);

            Assert.AreEqual(0, income.AddEarlyReadyBonus(bonus));
            Assert.AreEqual(300, wallet.Money);
        }

        [Test]
        public void EarlyReadyBonusFromGameFlow_ReachesWallet()
        {
            // GDD §9: 97 sn kala Enter → 9 tam adım × 5 = 45 para (hesap GameFlow'da, ekleme cüzdanda).
            var flow = new GameFlowSettings(120f, 4f, 15f, 5, 10f);
            var wallet = new Wallet(300, 100);
            var income = new WalletIncome(wallet, GddInterest);

            income.AddEarlyReadyBonus(flow.CalculateEarlyReadyBonus(97f));

            Assert.AreEqual(345, wallet.Money);
        }

        [Test]
        public void New_NullWallet_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new WalletIncome(null, GddInterest));
        }
    }
}
