using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>WeaponRuntime: şarjör, doldurma, sonsuz yedek, atış hızı, yükseltmeler ve run modifier'ları.</summary>
    public class WeaponRuntimeTests
    {
        private const float Tolerance = 0.0001f;
        private const float Frame = 1f / 60f;

        private TestWeapons _weapons;

        [SetUp]
        public void SetUp()
        {
            _weapons = new TestWeapons();
        }

        [TearDown]
        public void TearDown()
        {
            _weapons.DestroyAll();
        }

        [Test]
        public void New_StartsWithFullMagAndDataReserve()
        {
            var weapon = new WeaponRuntime(_weapons.Create(magSize: 6, reserve: 36));

            Assert.AreEqual(6, weapon.Ammo);
            Assert.AreEqual(36, weapon.Reserve);
            Assert.IsFalse(weapon.HasInfiniteReserve);
            Assert.IsTrue(weapon.CanFire);
        }

        [Test]
        public void TryFire_ConsumesOneRoundAndStartsCooldown()
        {
            var weapon = new WeaponRuntime(_weapons.Create(pellets: 8, fireRate: 2f, magSize: 6));

            Assert.IsTrue(weapon.TryFire());
            Assert.AreEqual(5, weapon.Ammo, "Saçma sayısından bağımsız, atış başına bir mermi.");
            Assert.IsFalse(weapon.TryFire(), "Bekleme süresi bitmeden ikinci atış yok.");

            weapon.Tick(0.49f);
            Assert.IsFalse(weapon.CanFire);
            weapon.Tick(0.02f);
            Assert.IsTrue(weapon.TryFire());
            Assert.AreEqual(4, weapon.Ammo);
        }

        [Test]
        public void FireRate_HeldTrigger_KeepsRateIndependentOfFrameTime()
        {
            // 0.07 sn aralık 60 FPS karelerinin katı değil; devreden süre olmasa 5 karede bir (~12/sn) atardı.
            var weapon = new WeaponRuntime(_weapons.Create(fireRate: 1f / 0.07f, magSize: 1000));
            int shots = 0;

            for (int frame = 0; frame < 600; frame++)
            {
                weapon.Tick(Frame);
                if (weapon.TryFire())
                {
                    shots++;
                }
            }

            Assert.AreEqual(10f / 0.07f, shots, 1.5f);
        }

        [Test]
        public void EmptyMag_CannotFire_AndNextTickStartsReload()
        {
            var weapon = new WeaponRuntime(_weapons.Create(fireRate: 100f, magSize: 2, reserve: 10));
            FireAll(weapon);

            Assert.AreEqual(0, weapon.Ammo);
            Assert.IsFalse(weapon.TryFire());
            Assert.IsFalse(weapon.IsReloading);

            weapon.Tick(Frame);

            Assert.IsTrue(weapon.IsReloading, "Şarjör boşalınca doldurma kendiliğinden başlar.");
        }

        [Test]
        public void Reload_FillsMagFromReserveAfterReloadTime()
        {
            var weapon = new WeaponRuntime(_weapons.Create(fireRate: 100f, magSize: 6, reserve: 36, reloadTime: 2f));
            weapon.TryFire();
            weapon.Tick(0.1f);
            weapon.TryFire();

            Assert.IsTrue(weapon.TryStartReload());
            weapon.Tick(1.9f);
            Assert.IsTrue(weapon.IsReloading);
            Assert.AreEqual(4, weapon.Ammo, "Süre dolmadan mermi değişmez.");
            Assert.IsFalse(weapon.TryFire(), "Doldururken ateş edilemez.");

            weapon.Tick(0.11f);

            Assert.IsFalse(weapon.IsReloading);
            Assert.AreEqual(6, weapon.Ammo);
            Assert.AreEqual(34, weapon.Reserve);
        }

        [Test]
        public void Reload_WithLowReserve_LoadsOnlyWhatIsLeft()
        {
            var weapon = new WeaponRuntime(_weapons.Create(fireRate: 100f, magSize: 6, reserve: 2, reloadTime: 0f));
            FireAll(weapon);

            weapon.Tick(Frame); // doldurma başlar
            weapon.Tick(Frame); // süre 0: biter

            Assert.AreEqual(2, weapon.Ammo);
            Assert.AreEqual(0, weapon.Reserve);
        }

        [Test]
        public void TryStartReload_FailsWhenFullOrReserveEmptyOrAlreadyReloading()
        {
            var full = new WeaponRuntime(_weapons.Create(magSize: 6, reserve: 36));
            Assert.IsFalse(full.TryStartReload(), "Şarjör doluyken doldurma yok.");

            var empty = new WeaponRuntime(_weapons.Create(fireRate: 100f, magSize: 2, reserve: 0));
            FireAll(empty);
            empty.Tick(Frame);
            Assert.IsFalse(empty.TryStartReload(), "Yedek yokken doldurma yok.");
            Assert.IsFalse(empty.IsReloading);

            var reloading = new WeaponRuntime(_weapons.Create(magSize: 6, reserve: 36));
            reloading.TryFire();
            Assert.IsTrue(reloading.TryStartReload());
            Assert.IsFalse(reloading.TryStartReload());
        }

        [Test]
        public void InfiniteReserve_NeverRunsOut()
        {
            var weapon = new WeaponRuntime(_weapons.Create(fireRate: 100f, magSize: 12, reserve: WeaponData.InfiniteReserve, reloadTime: 1.2f));
            Assert.IsTrue(weapon.HasInfiniteReserve);

            for (int cycle = 0; cycle < 20; cycle++)
            {
                FireAll(weapon);
                weapon.Tick(Frame);
                Assert.IsTrue(weapon.IsReloading, $"{cycle}. döngüde doldurma başlamadı.");
                weapon.Tick(1.2f);
                Assert.AreEqual(12, weapon.Ammo);
            }

            Assert.AreEqual(0, weapon.Reserve);
        }

        [Test]
        public void CancelReload_KeepsAmmoAndRaisesFinishedFalse()
        {
            var weapon = new WeaponRuntime(_weapons.Create(magSize: 6, reserve: 36));
            var finished = new List<bool>();
            weapon.ReloadFinished += finished.Add;
            weapon.TryFire();
            weapon.TryStartReload();

            weapon.CancelReload();
            weapon.Tick(5f);

            Assert.IsFalse(weapon.IsReloading);
            Assert.AreEqual(5, weapon.Ammo);
            Assert.AreEqual(36, weapon.Reserve);
            CollectionAssert.AreEqual(new[] { false }, finished);
        }

        [Test]
        public void Events_FireInOrder()
        {
            var weapon = new WeaponRuntime(_weapons.Create(fireRate: 100f, magSize: 1, reserve: 5, reloadTime: 1f));
            var log = new List<string>();
            weapon.AmmoChanged += () => log.Add($"ammo {weapon.Ammo}");
            weapon.ReloadStarted += duration => log.Add($"start {duration}");
            weapon.ReloadFinished += completed => log.Add($"finish {completed}");

            weapon.TryFire();
            weapon.Tick(Frame);
            weapon.Tick(1f);

            CollectionAssert.AreEqual(new[] { "ammo 0", "start 1", "ammo 1", "finish True" }, log);
        }

        [Test]
        public void Upgrades_StackPerLevelUpToMax()
        {
            var weapon = new WeaponRuntime(_weapons.Create(damage: 20f, fireRate: 4f, magSize: 12), _weapons.CreateUpgrades());

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(weapon.TryUpgrade(WeaponUpgradeType.Damage));
            }

            Assert.IsFalse(weapon.TryUpgrade(WeaponUpgradeType.Damage), "Hasar en fazla 5 seviye.");
            Assert.AreEqual(5, weapon.GetUpgradeLevel(WeaponUpgradeType.Damage));
            Assert.AreEqual(30f, weapon.Damage, Tolerance, "+%10 × 5 = ×1.5");

            weapon.TryUpgrade(WeaponUpgradeType.FireRate);
            Assert.AreEqual(4.32f, weapon.FireRate, Tolerance);
        }

        [Test]
        public void MagUpgrade_RoundsHalfUp_AndKeepsCurrentAmmo()
        {
            var weapon = new WeaponRuntime(_weapons.Create(magSize: 6, reserve: 36), _weapons.CreateUpgrades());
            int ammoEvents = 0;
            weapon.AmmoChanged += () => ammoEvents++;

            weapon.TryUpgrade(WeaponUpgradeType.MagSize);

            Assert.AreEqual(8, weapon.MagSize, "6 × 1.25 = 7.5 → 8");
            Assert.AreEqual(6, weapon.Ammo);
            Assert.AreEqual(1, ammoEvents);
            Assert.IsTrue(weapon.TryStartReload(), "Kapasite artınca dolu şarjör de doldurulabilir.");
        }

        [Test]
        public void WithoutUpgradeConfig_CannotUpgrade()
        {
            var weapon = new WeaponRuntime(_weapons.Create());

            Assert.IsFalse(weapon.TryUpgrade(WeaponUpgradeType.Damage));
            Assert.AreEqual(0, weapon.GetMaxUpgradeLevel(WeaponUpgradeType.Damage));
        }

        [Test]
        public void RunModifiers_ApplyOnTopOfUpgrades()
        {
            var modifiers = new RunModifiers();
            var weapon = new WeaponRuntime(_weapons.Create(damage: 20f, fireRate: 4f, magSize: 12), _weapons.CreateUpgrades(), modifiers);
            weapon.TryUpgrade(WeaponUpgradeType.Damage);

            modifiers.AddModifier(StatModifier.Percent(StatType.PlayerDamage, 0.15f, "heavy_rounds"));
            modifiers.AddModifier(StatModifier.Percent(StatType.PlayerFireRate, 0.3f, "quick_trigger"));
            modifiers.AddModifier(StatModifier.Flat(StatType.PlayerMagSize, 3f, "debug"));

            Assert.AreEqual(20f * 1.1f * 1.15f, weapon.Damage, Tolerance);
            Assert.AreEqual(5.2f, weapon.FireRate, Tolerance);
            Assert.AreEqual(15, weapon.MagSize);
        }

        [Test]
        public void FireRate_ReducedToZeroByModifiers_IsClampedAboveZero()
        {
            var modifiers = new RunModifiers();
            var weapon = new WeaponRuntime(_weapons.Create(fireRate: 4f), null, modifiers);

            modifiers.AddModifier(StatModifier.Percent(StatType.PlayerFireRate, -2f, "debug"));

            Assert.Greater(weapon.FireRate, 0f);
            Assert.IsFalse(float.IsInfinity(weapon.FireInterval));
        }

        [Test]
        public void AddReserve_IncreasesReserveAndRaisesAmmoChanged()
        {
            var weapon = new WeaponRuntime(_weapons.Create(magSize: 6, reserve: 36));
            int changes = 0;
            weapon.AmmoChanged += () => changes++;

            Assert.IsTrue(weapon.AddReserve(12));

            Assert.AreEqual(48, weapon.Reserve);
            Assert.AreEqual(6, weapon.Ammo, "Şarjör değişmez.");
            Assert.AreEqual(1, changes);
        }

        [Test]
        public void AddReserve_InfiniteReserveOrNonPositive_DoesNothing()
        {
            var pistol = new WeaponRuntime(_weapons.Create());
            var shotgun = new WeaponRuntime(_weapons.Create(reserve: 36));

            Assert.IsFalse(pistol.AddReserve(12));
            Assert.AreEqual(0, pistol.Reserve);
            Assert.IsFalse(shotgun.AddReserve(0));
            Assert.IsFalse(shotgun.AddReserve(-5));
            Assert.AreEqual(36, shotgun.Reserve);
        }

        [Test]
        public void AddReserve_WhenEmpty_LetsNextTickReload()
        {
            var weapon = new WeaponRuntime(_weapons.Create(magSize: 2, reserve: 0, reloadTime: 0.5f, fireRate: 100f));
            FireAll(weapon);
            weapon.Tick(Frame);
            Assert.IsFalse(weapon.IsReloading, "Yedek yokken doldurma başlamaz.");

            weapon.AddReserve(12);
            weapon.Tick(Frame);

            Assert.IsTrue(weapon.IsReloading);
        }

        [Test]
        public void AddReserve_SaturatesAtIntMax()
        {
            var weapon = new WeaponRuntime(_weapons.Create(reserve: int.MaxValue - 5));

            weapon.AddReserve(100);

            Assert.AreEqual(int.MaxValue, weapon.Reserve);
        }

        private static void FireAll(WeaponRuntime weapon)
        {
            // Atış hızı yüksek test silahlarında bekleme süresini Tick ile geçerek şarjörü boşaltır.
            int guard = 1000;
            while (weapon.Ammo > 0 && guard-- > 0)
            {
                if (!weapon.TryFire())
                {
                    weapon.Tick(Frame);
                }
            }
        }
    }
}
