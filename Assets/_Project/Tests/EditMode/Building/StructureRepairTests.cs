using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Economy;
using LastHaven.Editor;
using LastHaven.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// StructureRepair (GDD §11, 8.3): hasarlı yerleşik yapı onarım sunar; tur başına 1 para / 5 can, 50 can/sn, para bitince durur,
    /// mesafe ayak izinin kenarına, ipucu "Onar (N)", onarım barı. Oyuncunun basılı tutması (<see cref="InteractionHold"/>) ile de denenir.
    /// EditMode'da OnEnable çalışmaz; kayıt (IInteractableRegistry) Play duman testinde doğrulanır.
    /// </summary>
    public class StructureRepairTests
    {
        private const float Tolerance = 0.001f;

        private Wallet _wallet;
        private EconomyConfig _economy;
        private RepairConfig _config;
        private BuildableData _data;
        private GameObject _root;
        private Structure _structure;
        private StructureRepair _repair;
        private StructureRepairBar _bar;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            _wallet = new Wallet(1000, 0);
            _economy = ScriptableObject.CreateInstance<EconomyConfig>();
            ServiceLocator.Register<IWallet>(_wallet);
            ServiceLocator.Register<IEconomyRules>(_economy);
            ServiceLocator.Register<ITargetRegistry>(new TargetSet());
            ServiceLocator.Register<IInteractableRegistry>(new InteractableSet());

            _config = ScriptableObject.CreateInstance<RepairConfig>();
            _data = CreateBarricadeData();

            _root = new GameObject("StructureRepairTests");
            var collider = _root.AddComponent<BoxCollider>();
            collider.size = new Vector3(3f, 1.5f, 1f);
            collider.center = new Vector3(0f, 0.75f, 0f);
            _structure = _root.AddComponent<Structure>();
            _repair = _root.AddComponent<StructureRepair>();
            _bar = StructureRepairBarBuilder.Build(_root.transform, 1.5f, null, null);

            var serialized = new SerializedObject(_repair);
            serialized.FindProperty("_config").objectReferenceValue = _config;
            serialized.FindProperty("_bar").objectReferenceValue = _bar;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_data);
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_economy);
            ServiceLocator.Clear();
        }

        [Test]
        public void Config_DefaultsMatchGdd()
        {
            Assert.AreEqual(2f, _config.Range, Tolerance, "2 m yakınken.");
            Assert.AreEqual(50f, _config.Settings.HealthPerSecond, Tolerance);
            Assert.AreEqual(5f, _config.Settings.HealthPerMoney, Tolerance);
            Assert.AreEqual(0.1f, _repair.HoldDuration, Tolerance, "Tur: 5 can / 50 can/sn.");
            Assert.AreEqual(2f, _repair.InteractionRange, Tolerance);
            Assert.AreEqual("Onar (12)", _config.FormatPrompt(12));
        }

        [Test]
        public void CanInteract_OnlyWhenPlacedAliveAndDamaged()
        {
            Assert.IsFalse(_repair.CanInteract, "Yerleşmemiş (havuzda).");

            Place();
            Assert.IsFalse(_repair.CanInteract, "Tam can.");

            Damage(100f);
            Assert.IsTrue(_repair.CanInteract);

            Damage(1000f);
            Assert.IsFalse(_repair.CanInteract, "Yıkılmış.");
            Assert.IsFalse(_repair.OnInteract());
        }

        [Test]
        public void Prompt_ShowsCostToFullAndIsCachedUntilItChanges()
        {
            Place();
            Damage(100f);

            string prompt = _repair.Prompt;
            Assert.AreEqual("Onar (20)", prompt, "100 eksik can / 5 = 20 para.");
            Assert.AreSame(prompt, _repair.Prompt, "Maliyet değişmedikçe aynı string (her kare yeni string yok).");

            _repair.OnInteract();
            Assert.AreEqual("Onar (19)", _repair.Prompt);
        }

        [Test]
        public void OnInteract_SpendsOneMoneyAndHealsFiveAndShowsBar()
        {
            Place();
            Damage(100f);

            Assert.IsTrue(_repair.OnInteract());

            Assert.AreEqual(999, _wallet.Money);
            Assert.AreEqual(205f, _structure.Health.Current, Tolerance);
            Assert.AreEqual(1, _repair.SessionSpent);
            Assert.IsTrue(_bar.IsVisible, "Onarım sürerken bar görünür.");
            Assert.AreEqual(205f / 300f, _bar.Fill, Tolerance, "Bar canın doluluğu.");
            Assert.AreEqual(50, _structure.TotalSpent, "Onarım parası satış iadesine sayılmaz.");
        }

        [Test]
        public void Holding_OneSecond_RepairsFiftyForTenMoney()
        {
            Place();
            Damage(200f);
            var hold = new InteractionHold();
            hold.SetTarget(_repair);

            for (int i = 0; i < 10; i++)
            {
                hold.Tick(true, 0.1f);
            }

            hold.Tick(true, 0.001f); // Float birikimi: 10. tur sınırda kalmasın.
            Assert.AreEqual(150f, _structure.Health.Current, Tolerance, "GDD §11: 50 can/sn.");
            Assert.AreEqual(990, _wallet.Money, "5 can başına 1 para.");
        }

        [Test]
        public void Holding_UntilFull_StopsAtMaxAndOffersNothing()
        {
            Place();
            Damage(12f);
            var hold = new InteractionHold();
            hold.SetTarget(_repair);

            hold.Tick(true, 1f);

            Assert.AreEqual(300f, _structure.Health.Current, Tolerance);
            Assert.AreEqual(997, _wallet.Money, "12 can: 5 + 5 + 2 → 3 para.");
            Assert.IsFalse(_repair.CanInteract);
            Assert.AreEqual(0, _repair.SessionSpent, "Tam onarılınca oturum kapanır.");
        }

        [Test]
        public void Holding_MoneyRunsOut_RepairStops()
        {
            Place();
            Damage(100f);
            _wallet.TrySpend(WalletResource.Money, 997);
            var hold = new InteractionHold();
            hold.SetTarget(_repair);

            hold.Tick(true, 0.5f);

            Assert.AreEqual(0, _wallet.Money);
            Assert.AreEqual(215f, _structure.Health.Current, Tolerance, "3 para → 3 tur.");
            Assert.IsTrue(hold.IsStopped, "Para bitince onarım durur (GDD §11).");
            Assert.IsTrue(_repair.CanInteract, "Hâlâ hasarlı: ipucu kalır.");

            _wallet.Add(WalletResource.Money, 1);
            hold.Tick(true, 0.5f);
            Assert.AreEqual(215f, _structure.Health.Current, Tolerance, "Tuş bırakılana kadar yeniden başlamaz.");
        }

        [Test]
        public void GetDistance_MeasuresToFootprintEdge()
        {
            Place();

            Assert.AreEqual(1f, _repair.GetDistance(new Vector3(2.5f, 0f, 0f)), Tolerance, "3 m'lik barikatın ucundan.");
            Assert.AreEqual(2f, _repair.GetDistance(new Vector3(0f, 0f, 2.5f)), Tolerance, "1 m'lik yüzünden.");
            Assert.AreEqual(0f, _repair.GetDistance(new Vector3(1f, 0f, 0.2f)), Tolerance, "İçeride.");
            Assert.AreEqual(1.5f, _repair.PromptPosition.y, Tolerance, "İpucu yapının tepesinde.");
        }

        [Test]
        public void OnDespawned_HidesBarAndResetsSession()
        {
            Place();
            Damage(50f);
            _repair.OnInteract();

            _repair.OnDespawned();

            Assert.IsFalse(_bar.IsVisible);
            Assert.AreEqual(0, _repair.SessionSpent);
        }

        private void Place()
        {
            _structure.Place(null, _data, 50);
        }

        private void Damage(float amount)
        {
            _structure.Health.TakeDamage(new DamageInfo(amount, DamageType.Melee, null, Vector3.zero, Vector3.forward));
        }

        private static BuildableData CreateBarricadeData()
        {
            var data = ScriptableObject.CreateInstance<BuildableData>();
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = "barricade";
            serialized.FindProperty("_displayName").stringValue = "Barikat";
            serialized.FindProperty("_footprint").vector2IntValue = new Vector2Int(3, 1);
            SerializedProperty list = serialized.FindProperty("_levels");
            list.arraySize = 1;
            list.GetArrayElementAtIndex(0).boxedValue = new BuildableLevel("Ahşap", 50, 300f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }
    }
}
