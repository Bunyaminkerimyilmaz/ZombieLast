using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Tests.EditMode
{
    /// <summary>TargetSelector: GDD §7 hedef seçim kuralları 1–3 ve 0.5 sn'lik değerlendirme zamanlaması.</summary>
    public class TargetSelectorTests
    {
        private const float Interval = 0.5f;

        // GDD §7: Yürüyen (tercih yok, aggro 6), Tank (barikat 15 m, aggro 3), Tükürücü (taret 15 m, aggro 6).
        private static readonly TargetSelectorSettings Walker = new TargetSelectorSettings(6f, null, 15f, Interval);
        private static readonly TargetSelectorSettings Tank = new TargetSelectorSettings(3f, TargetType.Barricade, 15f, Interval);
        private static readonly TargetSelectorSettings Spitter = new TargetSelectorSettings(6f, TargetType.Turret, 15f, Interval);

        private sealed class FakeTarget : ITargetable
        {
            public FakeTarget(Transform transform, TargetType type)
            {
                Transform = transform;
                TargetType = type;
            }

            public Transform Transform { get; }

            public TargetType TargetType { get; }

            public bool IsTargetable { get; set; } = true;
        }

        private readonly List<GameObject> _objects = new List<GameObject>();
        private TargetSet _registry;

        [SetUp]
        public void SetUp()
        {
            _registry = new TargetSet();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                if (_objects[i] != null)
                {
                    Object.DestroyImmediate(_objects[i]);
                }
            }

            _objects.Clear();
        }

        // --- Kural 1: aggro ---

        [Test]
        public void Select_PlayerInsideAggro_ChoosesPlayerOverEverything()
        {
            FakeTarget player = Register(TargetType.Player, new Vector3(2f, 0f, 0f));
            Register(TargetType.Barricade, new Vector3(1f, 0f, 0f));
            Register(TargetType.Generator, new Vector3(0f, 0f, 1f));

            Assert.AreSame(player, Select(Tank, Vector3.zero));
            Assert.AreSame(player, Select(Walker, Vector3.zero));
        }

        [Test]
        public void Select_AggroRadius_IsInclusiveAndPerType()
        {
            FakeTarget player = Register(TargetType.Player, new Vector3(6f, 0f, 0f));
            FakeTarget generator = Register(TargetType.Generator, new Vector3(-20f, 0f, 0f));

            Assert.AreSame(player, Select(Walker, Vector3.zero), "6 m sınırı dahil.");
            Assert.AreSame(generator, Select(Walker, new Vector3(-0.1f, 0f, 0f)), "6.1 m: aggro dışı.");
            Assert.AreSame(generator, Select(Tank, new Vector3(2.9f, 0f, 0f)), "Tank aggro'su 3 m: 3.1 m dışarıda.");
            Assert.AreSame(player, Select(Tank, new Vector3(3f, 0f, 0f)));
        }

        [Test]
        public void Select_NonTargetablePlayer_IsIgnored()
        {
            FakeTarget player = Register(TargetType.Player, new Vector3(1f, 0f, 0f));
            FakeTarget generator = Register(TargetType.Generator, new Vector3(10f, 0f, 0f));
            player.IsTargetable = false;

            Assert.AreSame(generator, Select(Walker, Vector3.zero));
        }

        [Test]
        public void Select_MeasuresAggroOnHorizontalPlane()
        {
            FakeTarget player = Register(TargetType.Player, new Vector3(5f, 30f, 0f));
            Register(TargetType.Generator, new Vector3(-10f, 0f, 0f));

            Assert.AreSame(player, Select(Walker, Vector3.zero));
        }

        // --- Kural 2: tercih edilen hedef ---

        [Test]
        public void Select_PreferredTargetWithinRadius_BeatsGenerator()
        {
            FakeTarget barricade = Register(TargetType.Barricade, new Vector3(15f, 0f, 0f));
            Register(TargetType.Generator, new Vector3(-5f, 0f, 0f));

            Assert.AreSame(barricade, Select(Tank, Vector3.zero), "15 m sınırı dahil.");
        }

        [Test]
        public void Select_PreferredTargetOutsideRadius_FallsBackToGenerator()
        {
            Register(TargetType.Barricade, new Vector3(15.5f, 0f, 0f));
            FakeTarget generator = Register(TargetType.Generator, new Vector3(-30f, 0f, 0f));

            Assert.AreSame(generator, Select(Tank, Vector3.zero));
        }

        [Test]
        public void Select_PreferredTarget_PicksNearestOfThatType()
        {
            Register(TargetType.Barricade, new Vector3(10f, 0f, 0f));
            FakeTarget near = Register(TargetType.Barricade, new Vector3(0f, 0f, -4f));
            Register(TargetType.Generator, new Vector3(1f, 0f, 0f));

            Assert.AreSame(near, Select(Tank, Vector3.zero));
        }

        [Test]
        public void Select_OnlyThePreferredTypeCounts()
        {
            Register(TargetType.Barricade, new Vector3(2f, 0f, 0f));
            FakeTarget turret = Register(TargetType.Turret, new Vector3(8f, 0f, 0f));
            FakeTarget generator = Register(TargetType.Generator, new Vector3(20f, 0f, 0f));

            Assert.AreSame(turret, Select(Spitter, Vector3.zero), "Tükürücü barikatı değil tareti tercih eder.");
            Assert.AreSame(generator, Select(Walker, Vector3.zero), "Tercihi olmayan yapılara bakmaz.");
        }

        [Test]
        public void Select_NonTargetablePreferred_IsIgnored()
        {
            FakeTarget barricade = Register(TargetType.Barricade, new Vector3(2f, 0f, 0f));
            FakeTarget generator = Register(TargetType.Generator, new Vector3(20f, 0f, 0f));
            barricade.IsTargetable = false;

            Assert.AreSame(generator, Select(Tank, Vector3.zero));
        }

        // --- Kural 3: varsayılan jeneratör ---

        [Test]
        public void Select_DefaultsToGenerator_AtAnyDistance()
        {
            Register(TargetType.Player, new Vector3(50f, 0f, 0f));
            FakeTarget generator = Register(TargetType.Generator, new Vector3(0f, 0f, 500f));

            Assert.AreSame(generator, Select(Walker, Vector3.zero));
        }

        [Test]
        public void Select_NothingAvailable_ReturnsNull()
        {
            Register(TargetType.Player, new Vector3(50f, 0f, 0f));

            Assert.IsNull(Select(Walker, Vector3.zero));
            Assert.IsNull(TargetSelector.Select(null, Walker, Vector3.zero), "Kayıt servisi yoksa hedef yok.");
        }

        // --- Zamanlama ---

        [Test]
        public void Tick_FirstEvaluationWaitsForInitialDelay()
        {
            FakeTarget generator = Register(TargetType.Generator, Vector3.zero);
            var selector = new TargetSelector(_registry, Walker);
            // İkili tabanda tam gösterilen süreler: sınır (kalan = 0) float hatası olmadan denenir.
            selector.Reset(Walker, 0.25f);

            Assert.IsFalse(selector.Tick(0.125f, Vector3.zero));
            Assert.IsNull(selector.Current);

            Assert.IsTrue(selector.Tick(0.125f, Vector3.zero), "Kalan süre tam 0 olunca değerlendirir.");
            Assert.AreSame(generator, selector.Current);
        }

        [Test]
        public void Tick_ReevaluatesOnlyEveryInterval()
        {
            FakeTarget generator = Register(TargetType.Generator, new Vector3(0f, 0f, 30f));
            FakeTarget player = Register(TargetType.Player, new Vector3(20f, 0f, 0f));
            var selector = new TargetSelector(_registry, Walker);
            selector.Reset(Walker, 0f);
            selector.Tick(0.01f, Vector3.zero);
            Assert.AreSame(generator, selector.Current);

            // Oyuncu aggro'ya girdi; bir sonraki değerlendirmeye kadar hedef değişmez.
            player.Transform.position = new Vector3(2f, 0f, 0f);
            Assert.IsFalse(selector.Tick(0.45f, Vector3.zero));
            Assert.AreSame(generator, selector.Current);

            Assert.IsTrue(selector.Tick(0.05f, Vector3.zero));
            Assert.AreSame(player, selector.Current);
        }

        [Test]
        public void Tick_KeepsPhaseAcrossEvaluations()
        {
            Register(TargetType.Generator, Vector3.zero);
            var selector = new TargetSelector(_registry, Walker);
            selector.Reset(Walker, 0.2f);

            Assert.IsTrue(selector.Tick(0.3f, Vector3.zero));
            Assert.AreEqual(0.4f, selector.TimeUntilEvaluation, 0.0001f, "0.1 sn gecikme bir sonraki aralıktan düşülür.");
        }

        [Test]
        public void Tick_LostTarget_ReevaluatesImmediately()
        {
            FakeTarget player = Register(TargetType.Player, new Vector3(1f, 0f, 0f));
            FakeTarget generator = Register(TargetType.Generator, new Vector3(10f, 0f, 0f));
            var selector = new TargetSelector(_registry, Walker);
            selector.Reset(Walker, 0f);
            selector.Tick(0.01f, Vector3.zero);
            Assert.AreSame(player, selector.Current);

            player.IsTargetable = false;
            Assert.IsTrue(selector.Tick(0.01f, Vector3.zero), "Ölen hedef aralığı beklemeden bırakılır.");
            Assert.AreSame(generator, selector.Current);
        }

        [Test]
        public void Tick_NoTarget_WaitsForNextInterval()
        {
            var selector = new TargetSelector(_registry, Walker);
            selector.Reset(Walker, 0f);
            Assert.IsTrue(selector.Tick(0.01f, Vector3.zero));
            Assert.IsNull(selector.Current);

            Assert.IsFalse(selector.Tick(0.01f, Vector3.zero), "Hedef yokken her frame değerlendirilmez.");
        }

        [Test]
        public void Reset_ClearsTargetAndAppliesNewSettings()
        {
            FakeTarget player = Register(TargetType.Player, new Vector3(5f, 0f, 0f));
            FakeTarget generator = Register(TargetType.Generator, new Vector3(-10f, 0f, 0f));
            var selector = new TargetSelector(_registry, Walker);
            selector.Reset(Walker, 0f);
            selector.Tick(0.01f, Vector3.zero);
            Assert.AreSame(player, selector.Current);

            selector.Reset(Tank, 0.1f);
            Assert.IsNull(selector.Current);
            Assert.AreSame(generator, selector.Evaluate(Vector3.zero), "Tank aggro'su 3 m: 5 m'deki oyuncu seçilmez.");
        }

        private ITargetable Select(in TargetSelectorSettings settings, Vector3 position)
        {
            return TargetSelector.Select(_registry, settings, position);
        }

        private FakeTarget Register(TargetType type, Vector3 position)
        {
            var gameObject = new GameObject($"Target_{type}");
            gameObject.transform.position = position;
            _objects.Add(gameObject);
            var target = new FakeTarget(gameObject.transform, type);
            _registry.Register(target);
            return target;
        }
    }
}
