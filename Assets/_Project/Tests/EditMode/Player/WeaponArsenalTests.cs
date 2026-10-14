using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Core;
using LastHaven.Player;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>WeaponArsenal: başlangıç seçimi, silah değişimi, kilitler ve EventBus olayları.</summary>
    public class WeaponArsenalTests
    {
        private readonly List<WeaponChangedEvent> _changed = new List<WeaponChangedEvent>();
        private readonly List<AmmoChangedEvent> _ammo = new List<AmmoChangedEvent>();
        private readonly List<ReloadStartedEvent> _reloadStarted = new List<ReloadStartedEvent>();
        private readonly List<ReloadFinishedEvent> _reloadFinished = new List<ReloadFinishedEvent>();
        private TestWeapons _weapons;
        private WeaponSlotDefinition[] _definitions;

        [SetUp]
        public void SetUp()
        {
            _weapons = new TestWeapons();
            _definitions = new[]
            {
                new WeaponSlotDefinition(_weapons.Create(id: "pistol", magSize: 12), true),
                new WeaponSlotDefinition(_weapons.Create(id: "shotgun", magSize: 6, reserve: 36), false),
                new WeaponSlotDefinition(_weapons.Create(id: "rifle", magSize: 30, reserve: 180), false),
            };

            _changed.Clear();
            _ammo.Clear();
            _reloadStarted.Clear();
            _reloadFinished.Clear();
            EventBus<WeaponChangedEvent>.Subscribe(OnChanged);
            EventBus<AmmoChangedEvent>.Subscribe(OnAmmo);
            EventBus<ReloadStartedEvent>.Subscribe(OnReloadStarted);
            EventBus<ReloadFinishedEvent>.Subscribe(OnReloadFinished);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<WeaponChangedEvent>.Unsubscribe(OnChanged);
            EventBus<AmmoChangedEvent>.Unsubscribe(OnAmmo);
            EventBus<ReloadStartedEvent>.Unsubscribe(OnReloadStarted);
            EventBus<ReloadFinishedEvent>.Unsubscribe(OnReloadFinished);
            _weapons.DestroyAll();
        }

        [Test]
        public void New_HoldsFirstUnlockedWeapon_WithoutEvents()
        {
            var arsenal = new WeaponArsenal(_definitions, null, null);

            Assert.AreEqual(0, arsenal.CurrentSlot);
            Assert.AreEqual("pistol", arsenal.Current.Id);
            Assert.IsFalse(arsenal.IsUnlocked(1));
            Assert.IsFalse(arsenal.IsUnlocked(2));
            Assert.AreEqual(0, _changed.Count);

            arsenal.RaiseCurrentState();

            Assert.AreEqual(1, _changed.Count);
            Assert.AreEqual("pistol", _changed[0].WeaponId);
            Assert.AreEqual(1, _ammo.Count);
            Assert.AreEqual(12, _ammo[0].Ammo);
            Assert.IsTrue(_ammo[0].HasInfiniteReserve);
        }

        [Test]
        public void Select_LockedSlot_DoesNothing()
        {
            var arsenal = new WeaponArsenal(_definitions, null, null);

            Assert.IsFalse(arsenal.Select(1));
            Assert.IsFalse(arsenal.Cycle(1));
            Assert.AreEqual(0, arsenal.CurrentSlot);
            Assert.AreEqual(0, _changed.Count);
        }

        [Test]
        public void Select_Unlocked_RaisesChangedAndAmmo()
        {
            var arsenal = new WeaponArsenal(_definitions, null, null);
            arsenal.Unlock(2);

            Assert.IsTrue(arsenal.Select(2));

            Assert.AreEqual(1, _changed.Count);
            Assert.AreEqual(2, _changed[0].SlotIndex);
            Assert.AreEqual("rifle", _changed[0].WeaponId);
            Assert.AreEqual(1, _ammo.Count);
            Assert.AreEqual(30, _ammo[0].Ammo);
            Assert.AreEqual(180, _ammo[0].Reserve);
        }

        [Test]
        public void Select_CancelsReloadOfPreviousWeapon()
        {
            var arsenal = new WeaponArsenal(_definitions, null, null);
            arsenal.Unlock(1);
            WeaponRuntime pistol = arsenal.Current;
            pistol.TryFire();
            pistol.TryStartReload();

            arsenal.Cycle(1);

            Assert.IsFalse(pistol.IsReloading);
            Assert.AreEqual(11, pistol.Ammo);
            Assert.AreEqual(1, _reloadStarted.Count);
            Assert.AreEqual(1, _reloadFinished.Count);
            Assert.AreEqual("pistol", _reloadFinished[0].WeaponId);
            Assert.IsFalse(_reloadFinished[0].Completed);
        }

        [Test]
        public void Unlock_EmptySlotOrTwice_Fails()
        {
            var definitions = new[] { new WeaponSlotDefinition(null, false), _definitions[1] };
            var arsenal = new WeaponArsenal(definitions, null, null);

            Assert.IsFalse(arsenal.Unlock(0), "Boş yuva açılamaz.");
            Assert.IsTrue(arsenal.Unlock(1));
            Assert.IsFalse(arsenal.Unlock(1));
        }

        [Test]
        public void Unlock_WhenNothingHeld_HoldsIt()
        {
            var definitions = new[] { new WeaponSlotDefinition(_weapons.Create(id: "shotgun"), false) };
            var arsenal = new WeaponArsenal(definitions, null, null);
            Assert.IsNull(arsenal.Current);

            arsenal.Unlock(0);

            Assert.AreEqual(0, arsenal.CurrentSlot);
            Assert.AreEqual(1, _changed.Count);
        }

        [Test]
        public void AmmoChanged_OnlyForHeldWeapon()
        {
            var arsenal = new WeaponArsenal(_definitions, _weapons.CreateUpgrades(), null);
            arsenal.Unlock(1);

            arsenal.Current.TryFire();
            arsenal.GetWeapon(1).TryUpgrade(WeaponUpgradeType.MagSize);

            Assert.AreEqual(1, _ammo.Count, "Cepteki pompalının kapasite değişimi gönderilmez.");
            Assert.AreEqual("pistol", _ammo[0].WeaponId);
            Assert.AreEqual(11, _ammo[0].Ammo);
        }

        private void OnChanged(WeaponChangedEvent evt) => _changed.Add(evt);

        private void OnAmmo(AmmoChangedEvent evt) => _ammo.Add(evt);

        private void OnReloadStarted(ReloadStartedEvent evt) => _reloadStarted.Add(evt);

        private void OnReloadFinished(ReloadFinishedEvent evt) => _reloadFinished.Add(evt);
    }
}
