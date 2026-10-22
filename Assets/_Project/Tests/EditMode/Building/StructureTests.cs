using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Structure (GDD §11 barikat): yerleşme, hedef kaydı, yükseltme (para, can, görsel, olay), satış iadesi, yıkılma olayı.
    /// EditMode'da Awake/OnEnable çalışmaz; servisler (cüzdan, ekonomi kuralları, hedef kaydı) elle kaydedilir. Izgaradan
    /// kaldırma (BuildSite) Play duman testinde doğrulanır.
    /// </summary>
    public class StructureTests
    {
        private const float Tolerance = 0.001f;

        private readonly List<StructureUpgradedEvent> _upgraded = new List<StructureUpgradedEvent>();
        private readonly List<StructureDestroyedEvent> _destroyed = new List<StructureDestroyedEvent>();
        private Wallet _wallet;
        private EconomyConfig _economy;
        private TargetSet _targets;
        private Material _wood;
        private Material _metal;
        private Material _concrete;
        private Material _other;
        private BuildableData _data;
        private GameObject _root;
        private Renderer _body;
        private Structure _structure;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            _wallet = new Wallet(1000, 0);
            _economy = ScriptableObject.CreateInstance<EconomyConfig>();
            _targets = new TargetSet();
            ServiceLocator.Register<IWallet>(_wallet);
            ServiceLocator.Register<IEconomyRules>(_economy);
            ServiceLocator.Register<ITargetRegistry>(_targets);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            _wood = new Material(shader);
            _metal = new Material(shader);
            _concrete = new Material(shader);
            _other = new Material(shader);
            _data = CreateBarricadeData();

            _root = new GameObject("StructureTests");
            _root.AddComponent<BoxCollider>();
            _root.AddComponent<Health>();
            var visual = new GameObject(VisualRoot.Name);
            visual.transform.SetParent(_root.transform, false);
            _body = visual.AddComponent<MeshRenderer>();
            _body.sharedMaterials = new[] { _wood, _other };
            _structure = _root.AddComponent<Structure>();

            _upgraded.Clear();
            _destroyed.Clear();
            EventBus<StructureUpgradedEvent>.Subscribe(OnUpgraded);
            EventBus<StructureDestroyedEvent>.Subscribe(OnDestroyed);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<StructureUpgradedEvent>.Unsubscribe(OnUpgraded);
            EventBus<StructureDestroyedEvent>.Unsubscribe(OnDestroyed);
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_data);
            Object.DestroyImmediate(_economy);
            Object.DestroyImmediate(_wood);
            Object.DestroyImmediate(_metal);
            Object.DestroyImmediate(_concrete);
            Object.DestroyImmediate(_other);
            ServiceLocator.Clear();
        }

        [Test]
        public void New_IsNotPlaced()
        {
            Assert.IsFalse(_structure.IsPlaced);
            Assert.IsFalse(_structure.CanUpgrade);
            Assert.AreEqual(StructureUpgradeResult.NotPlaced, _structure.Upgrade());
            Assert.IsFalse(_structure.Sell());
        }

        [Test]
        public void Place_StartsAtFirstLevelWithFullHealth()
        {
            _structure.Place(null, _data, 50);

            Assert.IsTrue(_structure.IsPlaced);
            Assert.AreSame(_data, _structure.Data);
            Assert.AreEqual(1, _structure.Level);
            Assert.AreEqual(3, _structure.MaxLevel);
            Assert.AreEqual("Ahşap", _structure.CurrentLevel.DisplayName);
            Assert.AreEqual(50, _structure.TotalSpent);
            Assert.AreEqual(300f, _structure.Health.Max, Tolerance);
            Assert.AreEqual(300f, _structure.Health.Current, Tolerance);
            Assert.IsTrue(_structure.CanUpgrade);
            Assert.AreEqual(120, _structure.UpgradeCost);
            Assert.AreSame(_wood, _body.sharedMaterials[0]);
        }

        [Test]
        public void Place_RegistersAsDataTargetType()
        {
            _structure.Place(null, _data, 50);

            Assert.AreEqual(TargetType.Barricade, _structure.TargetType);
            Assert.That(_targets.GetAll(TargetType.Barricade), Has.Member(_structure));
        }

        [Test]
        public void Upgrade_PaysRaisesMaxKeepsDamageAndRaisesEvent()
        {
            _structure.Place(null, _data, 50);
            _structure.Health.TakeDamage(new DamageInfo(100f, DamageType.Melee, null, Vector3.zero, Vector3.forward));

            StructureUpgradeResult result = _structure.Upgrade();

            Assert.AreEqual(StructureUpgradeResult.Upgraded, result);
            Assert.AreEqual(880, _wallet.Money, "Metal: 120 para.");
            Assert.AreEqual(2, _structure.Level);
            Assert.AreEqual(170, _structure.TotalSpent);
            Assert.AreEqual(800f, _structure.Health.Max, Tolerance);
            Assert.AreEqual(700f, _structure.Health.Current, Tolerance, "Eksik 100 can korunur (yükseltme bedava onarım değil).");
            Assert.AreEqual(1, _upgraded.Count);
            Assert.AreSame(_root, _upgraded[0].Structure);
            Assert.AreEqual("barricade", _upgraded[0].BuildableId);
            Assert.AreEqual(2, _upgraded[0].Level);
            Assert.AreEqual(120, _upgraded[0].Cost);
        }

        [Test]
        public void Upgrade_ChangesOnlyLevelMaterial()
        {
            _structure.Place(null, _data, 50);

            _structure.Upgrade();
            Assert.AreSame(_metal, _body.sharedMaterials[0], "Kahve → gri.");
            Assert.AreSame(_other, _body.sharedMaterials[1], "Seviye materyali olmayan yuva değişmez.");

            _structure.Upgrade();
            Assert.AreSame(_concrete, _body.sharedMaterials[0], "Gri → beyaz.");
        }

        [Test]
        public void Upgrade_AtMaxLevel_ChangesNothing()
        {
            _structure.Place(null, _data, 50);
            _structure.Upgrade();
            _structure.Upgrade();
            int money = _wallet.Money;

            Assert.AreEqual(StructureUpgradeResult.MaxLevel, _structure.Upgrade());

            Assert.AreEqual(3, _structure.Level);
            Assert.AreEqual(money, _wallet.Money);
            Assert.AreEqual(2000f, _structure.Health.Max, Tolerance);
            Assert.AreEqual(2, _upgraded.Count);
            Assert.IsFalse(_structure.CanUpgrade);
            Assert.AreEqual(0, _structure.UpgradeCost);
        }

        [Test]
        public void Upgrade_NotEnoughMoney_ChangesNothing()
        {
            _structure.Place(null, _data, 50);
            _wallet.TrySpend(WalletResource.Money, 900);

            Assert.AreEqual(StructureUpgradeResult.NotEnoughMoney, _structure.Upgrade());

            Assert.AreEqual(100, _wallet.Money);
            Assert.AreEqual(1, _structure.Level);
            Assert.AreEqual(300f, _structure.Health.Max, Tolerance);
            Assert.AreSame(_wood, _body.sharedMaterials[0]);
            Assert.AreEqual(0, _upgraded.Count);
        }

        [Test]
        public void SellRefund_IsHalfOfTotalSpent()
        {
            _structure.Place(null, _data, 50);
            Assert.AreEqual(25, _structure.SellRefund);

            _structure.Upgrade();
            Assert.AreEqual(85, _structure.SellRefund, "(50 + 120) / 2.");

            _structure.Upgrade();
            Assert.AreEqual(235, _structure.SellRefund, "(50 + 120 + 300) / 2.");
        }

        [Test]
        public void Sell_WithoutSite_ReturnsFalseAndRefundsNothing()
        {
            _structure.Place(null, _data, 50);

            Assert.IsFalse(_structure.Sell());
            Assert.AreEqual(1000, _wallet.Money);
            Assert.IsTrue(_structure.IsPlaced);
        }

        [Test]
        public void Death_RaisesDestroyedEventAndLeavesTargetsAndRestoresVisual()
        {
            _root.transform.position = new Vector3(5f, 0f, -3f);
            _structure.Place(null, _data, 50);
            _structure.Upgrade();
            var source = new GameObject("Tank");
            try
            {
                _structure.Health.TakeDamage(new DamageInfo(5000f, DamageType.Melee, source, Vector3.zero, Vector3.forward));

                Assert.AreEqual(1, _destroyed.Count);
                Assert.AreSame(_root, _destroyed[0].Structure);
                Assert.AreEqual("barricade", _destroyed[0].BuildableId);
                Assert.AreEqual(new Vector3(5f, 0f, -3f), _destroyed[0].Position);
                Assert.AreSame(source, _destroyed[0].KillingBlow.Source);
                Assert.IsFalse(_structure.IsPlaced);
                Assert.That(_targets.GetAll(TargetType.Barricade), Has.No.Member(_structure));
                Assert.AreSame(_wood, _body.sharedMaterials[0], "Havuza dönen yapı ilk görünümüne döner.");
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void Place_Again_AfterDespawn_StartsOver()
        {
            _structure.Place(null, _data, 50);
            _structure.Upgrade();
            _structure.OnDespawned();

            _structure.Place(null, _data, 50);

            Assert.AreEqual(1, _structure.Level);
            Assert.AreEqual(50, _structure.TotalSpent);
            Assert.AreEqual(300f, _structure.Health.Max, Tolerance);
            Assert.AreEqual(300f, _structure.Health.Current, Tolerance);
            Assert.AreSame(_wood, _body.sharedMaterials[0]);
        }

        [Test]
        public void SturdyWalls_BeforePlacing_BoostsMaxHealth()
        {
            var modifiers = new RunModifiers();
            ServiceLocator.Register<IRunModifiers>(modifiers);
            modifiers.AddModifier(StatModifier.Percent(StatType.BarricadeMaxHealth, 0.3f, "perk:sturdy_walls"));

            _structure.Place(null, _data, 50);

            Assert.AreEqual(390f, _structure.Health.Max, Tolerance);
            Assert.AreEqual(390f, _structure.Health.Current, Tolerance);
        }

        [Test]
        public void SturdyWalls_WhilePlaced_RaisesMaxAndHealsOnlyTheIncrease()
        {
            var modifiers = new RunModifiers();
            ServiceLocator.Register<IRunModifiers>(modifiers);
            _structure.Place(null, _data, 50);
            _structure.Health.TakeDamage(new DamageInfo(100f, DamageType.Melee, null, Vector3.zero, Vector3.forward));

            modifiers.AddModifier(StatModifier.Percent(StatType.BarricadeMaxHealth, 0.3f, "perk:sturdy_walls"));

            Assert.AreEqual(390f, _structure.Health.Max, Tolerance);
            Assert.AreEqual(290f, _structure.Health.Current, Tolerance, "Eksik can (100) korunur.");

            _structure.Upgrade();
            Assert.AreEqual(1040f, _structure.Health.Max, Tolerance, "Metal 800 × 1.3.");
            Assert.AreEqual(940f, _structure.Health.Current, Tolerance);
        }

        [Test]
        public void SturdyWalls_AfterDespawn_NoLongerWatched()
        {
            var modifiers = new RunModifiers();
            ServiceLocator.Register<IRunModifiers>(modifiers);
            _structure.Place(null, _data, 50);
            _structure.OnDespawned();

            modifiers.AddModifier(StatModifier.Percent(StatType.BarricadeMaxHealth, 0.3f, "perk:sturdy_walls"));

            Assert.AreEqual(300f, _structure.Health.Max, Tolerance, "Havuzdaki yapı çarpanı dinlemez.");
            _structure.Place(null, _data, 50);
            Assert.AreEqual(390f, _structure.Health.Max, Tolerance, "Yeniden yerleşince güncel çarpanla.");
        }

        private BuildableData CreateBarricadeData()
        {
            var data = ScriptableObject.CreateInstance<BuildableData>();
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = "barricade";
            serialized.FindProperty("_displayName").stringValue = "Barikat";
            serialized.FindProperty("_footprint").vector2IntValue = new Vector2Int(3, 1);
            BuildableLevel[] levels =
            {
                new BuildableLevel("Ahşap", 50, 300f, _wood),
                new BuildableLevel("Metal", 120, 800f, _metal),
                new BuildableLevel("Beton", 300, 2000f, _concrete),
            };
            SerializedProperty list = serialized.FindProperty("_levels");
            list.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
            {
                list.GetArrayElementAtIndex(i).boxedValue = levels[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        private void OnUpgraded(StructureUpgradedEvent upgraded)
        {
            _upgraded.Add(upgraded);
        }

        private void OnDestroyed(StructureDestroyedEvent destroyed)
        {
            _destroyed.Add(destroyed);
        }
    }
}
