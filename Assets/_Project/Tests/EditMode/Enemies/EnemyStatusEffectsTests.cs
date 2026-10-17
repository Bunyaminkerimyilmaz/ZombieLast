using LastHaven.Core;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// EnemyStatusEffects (9.3): yavaşlatma alıcısı (en güçlüsü), yavaşlamışken Visual'da ton (HitFlash), havuzda temizlenir,
    /// ölüye etkisiz. Hız çarpanı NavMeshAgent ister (Play duman testi); burada durum ve ton denetlenir.
    /// </summary>
    public class EnemyStatusEffectsTests
    {
        private GameObject _root;
        private Material _lit;
        private Renderer _body;
        private Health _health;
        private HitFlash _flash;
        private EnemyStatusEffects _effects;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("SlowTestEnemy");
            var visual = new GameObject(VisualRoot.Name);
            visual.transform.SetParent(_root.transform, false);
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(visual.transform, false);
            _lit = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _body = cube.GetComponent<Renderer>();
            _body.sharedMaterial = _lit;

            _health = _root.AddComponent<Health>();
            _flash = _root.AddComponent<HitFlash>();
            _effects = _root.AddComponent<EnemyStatusEffects>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_lit);
        }

        [Test]
        public void ApplySlow_StrongestApplies_AndTintsVisual()
        {
            Assert.IsFalse(_effects.IsSlowed);

            _effects.ApplySlow(0.25f, 1f);
            _effects.ApplySlow(0.4f, 0.5f);

            Assert.IsTrue(_effects.IsSlowed);
            Assert.AreEqual(0.4f, _effects.SlowFraction, 0.0001f);
            Assert.IsTrue(_flash.HasTint, "Yavaşlamışken mavi ton.");
            Assert.IsTrue(_body.HasPropertyBlock());
        }

        [Test]
        public void OnDespawned_ClearsSlowAndTint()
        {
            _effects.ApplySlow(0.4f, 1f);

            ((IPoolable)_effects).OnDespawned();

            Assert.IsFalse(_effects.IsSlowed);
            Assert.IsFalse(_flash.HasTint);
            Assert.IsFalse(_body.HasPropertyBlock());
        }

        [Test]
        public void ApplySlow_OnDeadEnemy_IsIgnored()
        {
            _health.TakeDamage(new DamageInfo(1000f, DamageType.Bullet, null, Vector3.zero, Vector3.forward));

            _effects.ApplySlow(0.4f, 1f);

            Assert.IsFalse(_effects.IsSlowed);
            Assert.IsFalse(_flash.HasTint);
        }

        [Test]
        public void Tint_IsLightBlue()
        {
            Color tint = _effects.SlowTint;

            Assert.Greater(tint.b, tint.r, "Mavi baskın.");
            Assert.Greater(tint.b, tint.g);
            Assert.That(tint.a, Is.InRange(0.1f, 0.7f), "Hafif: materyal rengi seçilir kalır.");
        }
    }
}
