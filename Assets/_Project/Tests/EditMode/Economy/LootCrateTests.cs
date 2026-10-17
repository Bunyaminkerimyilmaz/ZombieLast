using LastHaven.Core;
using LastHaven.Economy;
using LastHaven.Player;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// LootCrate (GDD §9, 11.1): sahibi yokken etkileşim yok; kurulunca kaydolur, "Aç" ipucu, 2 sn basılı tutma, 2 m menzil
    /// (kenara); açılınca sahibine bir kez bildirir ve bir daha sunmaz; bırakılırsa ilerleme sıfırlanır; havuza dönünce kayıttan
    /// çıkar. Oyuncunun basılı tutması <see cref="InteractionHold"/> ile denenir.
    /// </summary>
    public class LootCrateTests
    {
        private const float Tolerance = 0.001f;

        private LootConfig _config;
        private InteractableSet _registry;
        private FakeOwner _owner;
        private GameObject _root;
        private LootCrate _crate;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LootConfig>();
            _registry = new InteractableSet();
            _owner = new FakeOwner();
            _root = new GameObject("LootCrateTests");
            var collider = _root.AddComponent<BoxCollider>();
            collider.size = Vector3.one;
            collider.center = new Vector3(0f, 0.5f, 0f);
            _crate = _root.AddComponent<LootCrate>();
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Config_DefaultsMatchGdd()
        {
            Assert.AreEqual(6, _config.CratesPerDay);
            Assert.AreEqual(22f, _config.MinDistance, Tolerance);
            Assert.AreEqual(38f, _config.MaxDistance, Tolerance);
            Assert.AreEqual(2f, _config.OpenHoldDuration, Tolerance, "E 2 sn basılı.");
            Assert.AreEqual("Aç", _config.OpenPrompt, "GDD §16: \"E — Aç\".");
            Assert.IsTrue(GameLayers.Contains(_config.BlockingMask, GameLayers.Obstacle));
            Assert.IsFalse(GameLayers.Contains(_config.BlockingMask, GameLayers.Ground), "Zemin her noktayı engellerdi.");
        }

        [Test]
        public void WithoutSetup_OffersNothing()
        {
            Assert.IsFalse(_crate.CanInteract);
            Assert.IsFalse(_crate.OnInteract());
            Assert.AreEqual(0, _owner.Opened);
        }

        [Test]
        public void Setup_RegistersAndUsesConfig()
        {
            _crate.Setup(_owner, _config, _registry);

            Assert.IsTrue(_crate.CanInteract);
            Assert.AreEqual(1, _registry.Count);
            Assert.AreEqual("Aç", _crate.Prompt);
            Assert.AreEqual(2f, _crate.HoldDuration, Tolerance);
            Assert.AreEqual(2f, _crate.InteractionRange, Tolerance);
            Assert.AreEqual(1f, _crate.PromptPosition.y, Tolerance, "İpucu sandığın tepesinde.");
            Assert.AreEqual(1.5f, _crate.GetDistance(new Vector3(2f, 0f, 0f)), Tolerance, "Mesafe kenara.");
            Assert.AreSame(_crate, _registry.FindNearest(new Vector3(2f, 0f, 0f)));
            Assert.IsNull(_registry.FindNearest(new Vector3(3f, 0f, 0f)), "2.5 m: menzil dışı.");
        }

        [Test]
        public void OnInteract_OpensOnce_ThenStopsOffering()
        {
            _crate.Setup(_owner, _config, _registry);

            bool keepHolding = _crate.OnInteract();

            Assert.IsFalse(keepHolding, "Bir basışta bir sandık.");
            Assert.IsTrue(_crate.IsOpened);
            Assert.IsFalse(_crate.CanInteract);
            Assert.AreEqual(1, _owner.Opened);
            Assert.AreSame(_crate, _owner.LastCrate);

            _crate.OnInteract();
            Assert.AreEqual(1, _owner.Opened, "Açık sandık tekrar açılmaz.");
        }

        [Test]
        public void Hold_TwoSeconds_Opens_ReleaseResetsProgress()
        {
            _crate.Setup(_owner, _config, _registry);
            var hold = new InteractionHold();
            hold.SetTarget(_crate);

            hold.Tick(true, 1.5f);
            Assert.AreEqual(0.75f, hold.Progress, Tolerance);
            hold.Tick(false, 0.1f);
            Assert.AreEqual(0f, hold.Progress, "Bırakınca sıfırlanır.");

            hold.Tick(true, 1.9f);
            Assert.AreEqual(0, _owner.Opened, "Baştan sayılıyor: 1.9 sn yetmez.");
            hold.Tick(true, 0.2f);
            Assert.AreEqual(1, _owner.Opened);
            Assert.IsFalse(hold.IsHolding);
        }

        [Test]
        public void OnDespawned_Unregisters_AndResets()
        {
            _crate.Setup(_owner, _config, _registry);
            _crate.OnInteract();

            _crate.OnDespawned();

            Assert.AreEqual(0, _registry.Count);
            Assert.IsFalse(_crate.IsOpened);
            Assert.IsFalse(_crate.CanInteract, "Sahibi yok.");

            _crate.Setup(_owner, _config, _registry);
            Assert.IsTrue(_crate.CanInteract, "Havuzdan yeniden çıkınca kapalı başlar.");
        }

        private sealed class FakeOwner : ILootCrateOwner
        {
            public int Opened { get; private set; }

            public LootCrate LastCrate { get; private set; }

            public void Open(LootCrate crate)
            {
                Opened++;
                LastCrate = crate;
            }
        }
    }
}
