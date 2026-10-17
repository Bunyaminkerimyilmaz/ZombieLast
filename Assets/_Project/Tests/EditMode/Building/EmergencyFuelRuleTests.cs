using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Economy;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// EmergencyFuelRule (GDD §12, 9.2): 30 para → 10 yakıt; para yetmezse veya depoda tam miktar kadar yer yoksa cüzdan değişmez.
    /// </summary>
    public class EmergencyFuelRuleTests
    {
        private const int Price = 30;
        private const int Amount = 10;

        [TestCase(0, 150, 10, true)]
        [TestCase(140, 150, 10, true)]
        [TestCase(141, 150, 10, false)]
        [TestCase(150, 150, 10, false)]
        [TestCase(0, 150, 0, false)]
        [TestCase(int.MaxValue - 5, int.MaxValue, 10, false)]
        public void HasRoom_RequiresWholeAmount(int fuel, int capacity, int amount, bool expected)
        {
            Assert.AreEqual(expected, EmergencyFuelRule.HasRoom(fuel, capacity, amount));
        }

        [Test]
        public void TryBuy_Purchases_MoneyThenFuel()
        {
            var wallet = new Wallet(100, 0);
            wallet.SetCapacity(WalletResource.Fuel, 150);
            var order = new List<string>();
            wallet.MoneyChanged += (_, _) => order.Add("money");
            wallet.FuelChanged += (_, _) => order.Add("fuel");

            EmergencyFuelResult result = EmergencyFuelRule.TryBuy(wallet, Price, Amount);

            Assert.AreEqual(EmergencyFuelResult.Purchased, result);
            Assert.AreEqual(70, wallet.Money);
            Assert.AreEqual(10, wallet.Fuel);
            CollectionAssert.AreEqual(new[] { "money", "fuel" }, order);
        }

        [Test]
        public void TryBuy_NotEnoughMoney_ChangesNothing()
        {
            var wallet = new Wallet(29, 0);

            Assert.AreEqual(EmergencyFuelResult.NotEnoughMoney, EmergencyFuelRule.TryBuy(wallet, Price, Amount));
            Assert.AreEqual(29, wallet.Money);
            Assert.AreEqual(0, wallet.Fuel);
        }

        [Test]
        public void TryBuy_TankFull_ChangesNothing()
        {
            var wallet = new Wallet(100, 145);
            wallet.SetCapacity(WalletResource.Fuel, 150);

            Assert.AreEqual(EmergencyFuelResult.TankFull, EmergencyFuelRule.TryBuy(wallet, Price, Amount));
            Assert.AreEqual(100, wallet.Money, "Para düşülmemeli.");
            Assert.AreEqual(145, wallet.Fuel);
        }

        [Test]
        public void TryBuy_ExactPrice_Purchases()
        {
            var wallet = new Wallet(30, 0);

            Assert.AreEqual(EmergencyFuelResult.Purchased, EmergencyFuelRule.TryBuy(wallet, Price, Amount));
            Assert.AreEqual(0, wallet.Money);
            Assert.AreEqual(10, wallet.Fuel);
        }
    }
}
