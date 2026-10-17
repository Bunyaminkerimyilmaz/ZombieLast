using LastHaven.UI;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>BossBarState: doğan boss barı açar, can ve yenilgi sadece izlenen boss'tan, yeni boss eskinin yerine geçer.</summary>
    public class BossBarStateTests
    {
        private sealed class Boss
        {
        }

        [Test]
        public void New_IsHidden()
        {
            var state = new BossBarState<Boss>();

            Assert.IsFalse(state.IsVisible);
            Assert.AreEqual(string.Empty, state.Name);
        }

        [Test]
        public void Spawn_ShowsFullBar_HealthUpdates()
        {
            var state = new BossBarState<Boss>();
            var brute = new Boss();

            state.Spawn(brute, "Brute", 3000f);
            Assert.IsTrue(state.IsVisible);
            Assert.AreEqual("Brute", state.Name);
            Assert.AreEqual(3000f, state.Current);
            Assert.AreEqual(3000f, state.Max);

            Assert.IsTrue(state.SetHealth(brute, 1400f, 3000f));
            Assert.AreEqual(1400f, state.Current);
        }

        [Test]
        public void OtherBoss_IsIgnored_UntilItSpawns()
        {
            var state = new BossBarState<Boss>();
            var brute = new Boss();
            var other = new Boss();
            state.Spawn(brute, "Brute", 3000f);

            Assert.IsFalse(state.SetHealth(other, 10f, 6000f));
            Assert.IsFalse(state.Defeat(other));
            Assert.IsTrue(state.IsVisible);
            Assert.AreEqual(3000f, state.Current);

            state.Spawn(other, "Brute (Final)", 6000f);
            Assert.IsFalse(state.SetHealth(brute, 1f, 3000f), "Önceki boss artık izlenmiyor.");
            Assert.AreEqual(6000f, state.Max);
        }

        [Test]
        public void Defeat_Hides_AndHideClears()
        {
            var state = new BossBarState<Boss>();
            var brute = new Boss();
            state.Spawn(brute, "Brute", 3000f);

            Assert.IsTrue(state.Defeat(brute));
            Assert.IsFalse(state.IsVisible);
            Assert.IsFalse(state.Defeat(brute), "İkinci yenilgi yok sayılır.");

            state.Spawn(brute, "Brute", 3000f);
            state.Hide();
            Assert.IsFalse(state.IsVisible);
            Assert.IsFalse(state.SetHealth(brute, 5f, 3000f));
        }
    }
}
