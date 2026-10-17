using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.Waves;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// NightModifierController: olay varsa gece başında Activated (tüm veriyle), bitince Ended; olay yoksa olay yok; yeni gece öncekini
    /// bitirir; hız çarpanı.
    /// </summary>
    public class NightModifierControllerTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly List<NightModifierActivatedEvent> _activated = new List<NightModifierActivatedEvent>();
        private readonly List<NightModifierEndedEvent> _ended = new List<NightModifierEndedEvent>();

        [SetUp]
        public void SetUp()
        {
            EventBus<NightModifierActivatedEvent>.Subscribe(_activated.Add);
            EventBus<NightModifierEndedEvent>.Subscribe(_ended.Add);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<NightModifierActivatedEvent>.Unsubscribe(_activated.Add);
            EventBus<NightModifierEndedEvent>.Unsubscribe(_ended.Add);
            foreach (Object created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
            _activated.Clear();
            _ended.Clear();
        }

        [Test]
        public void Begin_RaisesActivatedWithAllSettings_EndRaisesEnded()
        {
            var controller = new NightModifierController();
            NightModifierData bloodMoon = Create(NightModifierDefinitions.BloodMoon);

            controller.Begin(4, bloodMoon);

            Assert.IsTrue(controller.IsActive);
            Assert.AreEqual(1, _activated.Count);
            Assert.AreEqual(4, _activated[0].NightIndex);
            NightModifierSettings settings = _activated[0].Modifier;
            Assert.AreEqual("blood_moon", settings.Id);
            Assert.AreEqual(1.5f, settings.EnemyCountMultiplier);
            Assert.AreEqual(2f, settings.CoinMultiplier);
            Assert.IsTrue(settings.OverridesLight);
            Assert.AreEqual("night_modifier:blood_moon", settings.SourceId);

            controller.End();
            Assert.IsFalse(controller.IsActive);
            Assert.AreEqual(1, _ended.Count);
            Assert.AreEqual("blood_moon", _ended[0].Modifier.Id);

            controller.End();
            Assert.AreEqual(1, _ended.Count, "İkinci End bir şey yapmaz.");
        }

        [Test]
        public void Begin_WithoutModifier_RaisesNothing()
        {
            var controller = new NightModifierController();

            controller.Begin(0, null);
            controller.End();

            Assert.IsFalse(controller.IsActive);
            Assert.IsEmpty(_activated);
            Assert.IsEmpty(_ended);
            Assert.AreEqual(1f, controller.EnemySpeedMultiplier);
        }

        [Test]
        public void Begin_EndsPreviousFirst()
        {
            var controller = new NightModifierController();
            controller.Begin(3, Create(NightModifierDefinitions.Fog));

            controller.Begin(4, Create(NightModifierDefinitions.BloodMoon));

            Assert.AreEqual(1, _ended.Count);
            Assert.AreEqual("fog", _ended[0].Modifier.Id);
            Assert.AreEqual(2, _activated.Count);
        }

        [Test]
        public void SpeedMultiplier_FromActiveModifier_ApplyToIgnoresNonEnemies()
        {
            var controller = new NightModifierController();
            controller.Begin(3, Create(NightModifierDefinitions.Fog));

            Assert.AreEqual(1.1f, controller.EnemySpeedMultiplier, 0.0001f, "GDD §14 Sis: +%10.");
            var plain = new GameObject("NotEnemy");
            _created.Add(plain);
            Assert.DoesNotThrow(() => controller.ApplyTo(plain));
            Assert.DoesNotThrow(() => controller.ApplyTo(null));
        }

        private NightModifierData Create(NightModifierDefinition definition)
        {
            var data = ScriptableObject.CreateInstance<NightModifierData>();
            _created.Add(data);
            NightModifiersGenerator.Apply(data, definition);
            return data;
        }
    }
}
