using LastHaven.Building;
using LastHaven.Combat;
using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Yapı perk kuralları (13.2, GDD §13): Sağlam Duvar sadece barikata, Dikenli Duvar sadece yakın dövüşü yansıtır, Şok Taret.</summary>
    public class BuildingPerkRulesTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void SturdyWalls_OnlyBarricadesUseModifier()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Percent(StatType.BarricadeMaxHealth, 0.3f, "perk:sturdy_walls"));

            Assert.AreEqual(390f, StructureHealthRule.GetMaxHealth(TargetType.Barricade, 300f, modifiers), Tolerance);
            Assert.AreEqual(250f, StructureHealthRule.GetMaxHealth(TargetType.Turret, 250f, modifiers), Tolerance);
            Assert.AreEqual(40f, StructureHealthRule.GetMaxHealth(TargetType.Trap, 40f, modifiers), Tolerance);
            Assert.AreEqual(1500f, StructureHealthRule.GetMaxHealth(TargetType.Generator, 1500f, modifiers), Tolerance);
            Assert.AreEqual(300f, StructureHealthRule.GetMaxHealth(TargetType.Barricade, 300f, null), Tolerance);
        }

        [Test]
        public void SturdyWalls_TwoStacksAddUp_AndMaxStaysPositive()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Percent(StatType.BarricadeMaxHealth, 0.3f, "perk:sturdy_walls"));
            modifiers.AddModifier(StatModifier.Percent(StatType.BarricadeMaxHealth, 0.3f, "perk:sturdy_walls"));
            Assert.AreEqual(1280f, StructureHealthRule.GetMaxHealth(TargetType.Barricade, 800f, modifiers), Tolerance, "Metal 800 × 1.6.");

            var negative = new RunModifiers();
            negative.AddModifier(StatModifier.Percent(StatType.BarricadeMaxHealth, -2f, "test"));
            Assert.AreEqual(1f, StructureHealthRule.GetMaxHealth(TargetType.Barricade, 300f, negative), Tolerance);
        }

        [TestCase(DamageType.Melee, true)]
        [TestCase(DamageType.Explosion, false)]
        [TestCase(DamageType.Projectile, false)]
        [TestCase(DamageType.Bullet, false)]
        [TestCase(DamageType.Trap, false)]
        public void ThornWalls_OnlyMeleeReflects(DamageType type, bool reflects)
        {
            Assert.AreEqual(reflects, ThornsRule.Reflects(type));
        }

        [Test]
        public void ThornWalls_ReflectsFractionOfDamageTaken()
        {
            Assert.AreEqual(3f, ThornsRule.GetReflectedDamage(10f, 0.3f), Tolerance, "Yürüyen 10 → 3.");
            Assert.AreEqual(21f, ThornsRule.GetReflectedDamage(70f, 0.3f), Tolerance, "Tank 35 × 2 → 21.");
            Assert.AreEqual(0f, ThornsRule.GetReflectedDamage(0f, 0.3f), Tolerance);
            Assert.AreEqual(0f, ThornsRule.GetReflectedDamage(10f, 0f), Tolerance);
            Assert.AreEqual(0f, ThornsRule.GetReflectedDamage(10f, -1f), Tolerance);
        }

        [Test]
        public void ShockTurret_IgnoresMissesAndDisabledValues()
        {
            var results = new[] { ShotResult.Miss(UnityEngine.Vector3.zero, UnityEngine.Vector3.forward, 10f) };

            Assert.AreEqual(0, TurretShock.Apply(results, 0.25f, 1f), "Iska yavaşlatmaz.");
            Assert.AreEqual(0, TurretShock.Apply(results, 0f, 1f));
            Assert.AreEqual(0, TurretShock.Apply(results, 0.25f, 0f));
            Assert.AreEqual(0, TurretShock.Apply(null, 0.25f, 1f));
        }
    }
}
