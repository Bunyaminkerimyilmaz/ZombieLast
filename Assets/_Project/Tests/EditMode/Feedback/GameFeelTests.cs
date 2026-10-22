using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Feedback;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Oyun hissi eşlemesi (14.1): <see cref="FeedbackRules"/> (silah/ağır düşman/öldüren → sarsıntı ve donma),
    /// <see cref="VignettePulse"/> (hasar flaşı + düşük can nabzı) ve <see cref="GameFeelEffects"/>'in olay → istek köprüsü.
    /// </summary>
    public class GameFeelTests
    {
        private const float Tolerance = 0.0001f;
        private const string Shotgun = "shotgun";
        private const string Tank = "tank";

        private static readonly VignetteSettings Vignette = new VignetteSettings(
            flashWeight: 1f, flashDuration: 0.4f, lowHealthThreshold: 0.3f, pulsePeriod: 1f, pulseMinWeight: 0.2f, pulseMaxWeight: 0.6f);

        private readonly List<CameraShakeRequestedEvent> _shakes = new List<CameraShakeRequestedEvent>();
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            _shakes.Clear();
            EventBus<CameraShakeRequestedEvent>.Subscribe(OnShake);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<CameraShakeRequestedEvent>.Unsubscribe(OnShake);
            foreach (Object created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        [Test]
        public void ShotShake_WeaponOverrideById_ElseFallback()
        {
            var overrides = new List<WeaponShake> { new WeaponShake(Shotgun, 0.2f) };

            Assert.AreEqual(0.2f, FeedbackRules.GetShotShake(Shotgun, overrides, 0.05f), Tolerance);
            Assert.AreEqual(0.05f, FeedbackRules.GetShotShake("pistol", overrides, 0.05f), Tolerance);
            Assert.AreEqual(0.05f, FeedbackRules.GetShotShake(null, overrides, 0.05f), Tolerance);
            Assert.AreEqual(0.05f, FeedbackRules.GetShotShake("SHOTGUN", overrides, 0.05f), Tolerance, "Id karşılaştırması ordinal.");
        }

        [Test]
        public void IsHeavy_OrdinalIdMatch()
        {
            var heavy = new List<string> { Tank, "brute" };

            Assert.IsTrue(FeedbackRules.IsHeavy(Tank, heavy));
            Assert.IsFalse(FeedbackRules.IsHeavy("walker", heavy));
            Assert.IsFalse(FeedbackRules.IsHeavy("TANK", heavy));
            Assert.IsFalse(FeedbackRules.IsHeavy(null, heavy));
        }

        [TestCase(false, true, true, 0.1f)]
        [TestCase(false, false, true, 0f)]
        [TestCase(false, false, false, 0.1f)]
        [TestCase(true, false, true, 0.6f)]
        [TestCase(true, true, true, 0.6f)]
        public void KillShake_HeavyAlways_NormalOnlyForPlayerKillsWhenFlagged(
            bool heavy, bool killedByPlayer, bool playerKillsOnly, float expected)
        {
            Assert.AreEqual(expected, FeedbackRules.GetKillShake(heavy, killedByPlayer, 0.1f, 0.6f, playerKillsOnly), Tolerance);
        }

        [TestCase(true, true, true, 0.06f)]
        [TestCase(true, false, true, 0f)]
        [TestCase(true, false, false, 0.06f)]
        [TestCase(false, true, true, 0f)]
        public void KillHitStop_OnlyHeavy_PlayerKillsWhenFlagged(bool heavy, bool killedByPlayer, bool playerKillsOnly, float expected)
        {
            Assert.AreEqual(expected, FeedbackRules.GetKillHitStop(heavy, killedByPlayer, 0.06f, playerKillsOnly), Tolerance);
        }

        [Test]
        public void Vignette_Flash_PeaksThenFadesLinearly()
        {
            var pulse = new VignettePulse(Vignette);
            Assert.AreEqual(0f, pulse.Tick(0.1f, 1f, true), "Tam canda flaş yokken kenar görünmez.");

            pulse.Flash();
            Assert.AreEqual(1f, pulse.Tick(0.2f, 1f, true), Tolerance, "Flaş tepeden başlar.");
            Assert.AreEqual(0.5f, pulse.Tick(0.2f, 1f, true), Tolerance);
            Assert.AreEqual(0f, pulse.Tick(0.2f, 1f, true), Tolerance);
        }

        [Test]
        public void Vignette_LowHealth_PulsesBetweenMinAndMax_StopsWhenHealedOrDead()
        {
            var pulse = new VignettePulse(Vignette);

            Assert.AreEqual(0.2f, pulse.Tick(0.5f, 0.2f, true), Tolerance, "Nabız en sönükten başlar.");
            Assert.AreEqual(0.6f, pulse.Tick(0.5f, 0.2f, true), Tolerance, "Yarım periyotta en parlak.");
            Assert.AreEqual(0.2f, pulse.Tick(0.1f, 0.2f, true), Tolerance, "Tam periyotta yine en sönük.");
            Assert.AreEqual(0f, pulse.Tick(0.1f, 0.3f, true), Tolerance, "Eşikte (%30) nabız yok.");
            Assert.AreEqual(0f, pulse.Tick(0.1f, 0.1f, false), Tolerance, "Ölüyken nabız yok.");
        }

        [Test]
        public void Vignette_FlashAndPulse_TakesStronger()
        {
            var pulse = new VignettePulse(Vignette);
            pulse.Flash();

            Assert.AreEqual(1f, pulse.Tick(0.3f, 0.1f, true), Tolerance, "Flaş tepede, nabız en sönükte → flaş.");

            // 0.3 sn sonra: flaş 0.1 / 0.4 = 0.25'e indi, nabız 0.2 + 0.4 × (0.5 − 0.5·cos(0.6π)) ≈ 0.46 → nabız.
            float expectedPulse = 0.2f + 0.4f * (0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * 0.3f));
            Assert.AreEqual(expectedPulse, pulse.Tick(0.2f, 0.1f, true), Tolerance);
            Assert.Greater(expectedPulse, 0.25f);
        }

        [Test]
        public void Effects_PlayerShot_RequestsWeaponShake_TurretShotIgnored()
        {
            var effects = new GameFeelEffects(CreateConfig(), null, null);
            var player = CreateObject("Player", GameTags.Player);
            var turret = CreateObject("Turret", GameTags.Untagged);

            effects.OnWeaponFired(new WeaponFiredEvent(turret, Shotgun, Vector3.zero, Vector3.forward, 1));
            Assert.AreEqual(0, _shakes.Count, "Taret atışı sarsmaz.");

            effects.OnWeaponFired(new WeaponFiredEvent(player, Shotgun, Vector3.one, Vector3.forward, 8));
            Assert.AreEqual(1, _shakes.Count);
            Assert.AreEqual(0.2f, _shakes[0].Strength, Tolerance);
            Assert.IsTrue(_shakes[0].HasPosition);
            Assert.AreEqual(Vector3.one, _shakes[0].Position);
        }

        [Test]
        public void Effects_HeavyPlayerKill_ShakesAndHitStops_TurretKillOnlyShakes()
        {
            var hitStop = new FakeHitStop();
            var effects = new GameFeelEffects(CreateConfig(), null, hitStop);

            effects.OnEnemyKilled(new EnemyKilledEvent(Vector3.zero, Tank, 10, 5, killedByPlayer: true));
            Assert.AreEqual(1, _shakes.Count);
            Assert.AreEqual(0.06f, hitStop.LastDuration, Tolerance);

            hitStop.LastDuration = 0f;
            effects.OnEnemyKilled(new EnemyKilledEvent(Vector3.zero, Tank, 10, 5, killedByPlayer: false));
            Assert.AreEqual(2, _shakes.Count, "Ağır ölüm kim öldürürse öldürsün sarsar.");
            Assert.AreEqual(0f, hitStop.LastDuration, "Taret öldürmesi oyunu dondurmaz.");

            effects.OnEnemyKilled(new EnemyKilledEvent(Vector3.zero, "walker", 2, 1, killedByPlayer: false));
            Assert.AreEqual(2, _shakes.Count, "Normal ölüm sadece oyuncunun öldürmesinde sarsar.");
        }

        [Test]
        public void Effects_Explosion_RequestsPositionedShake()
        {
            var effects = new GameFeelEffects(CreateConfig(), null, null);

            effects.OnExplosion(new ExplosionEvent(new Vector3(3f, 0f, 4f), 3f, null));

            Assert.AreEqual(1, _shakes.Count);
            Assert.IsTrue(_shakes[0].HasPosition);
            Assert.Greater(_shakes[0].Strength, 0f);
        }

        [Test]
        public void ShakeEvent_StrengthOnlyConstructor_IsGlobal()
        {
            var global = new CameraShakeRequestedEvent(0.25f);

            Assert.IsFalse(global.HasPosition);
            Assert.AreEqual(0.25f, global.Strength, Tolerance);
        }

        private FeedbackConfig CreateConfig()
        {
            var config = ScriptableObject.CreateInstance<FeedbackConfig>();
            _created.Add(config);
            var serialized = new SerializedObject(config);
            SerializedProperty shakes = serialized.FindProperty("_weaponShakes");
            shakes.arraySize = 1;
            shakes.GetArrayElementAtIndex(0).FindPropertyRelative("_weaponId").stringValue = Shotgun;
            shakes.GetArrayElementAtIndex(0).FindPropertyRelative("_strength").floatValue = 0.2f;
            SerializedProperty heavy = serialized.FindProperty("_heavyEnemyIds");
            heavy.arraySize = 1;
            heavy.GetArrayElementAtIndex(0).stringValue = Tank;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private GameObject CreateObject(string name, string tag)
        {
            var gameObject = new GameObject(name) { tag = tag };
            _created.Add(gameObject);
            return gameObject;
        }

        private void OnShake(CameraShakeRequestedEvent shake)
        {
            _shakes.Add(shake);
        }

        private sealed class FakeHitStop : IHitStopService
        {
            public float LastDuration { get; set; }

            public bool IsHitStopped => LastDuration > 0f;

            public void HitStop(float duration)
            {
                LastDuration = duration;
            }
        }
    }
}
