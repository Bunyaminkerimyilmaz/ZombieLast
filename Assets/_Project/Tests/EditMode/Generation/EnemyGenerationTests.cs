using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Düşman üreticisi: GDD §7 değerlerinin EnemyData'ya doğru alanlara yazılması ve prefab yapısı (kök/Visual ayrımı, ölçüler).
    /// Asset oluşturmaz; veri bellekte, prefab hiyerarşisi sahnede kurulup silinir.
    /// </summary>
    public class EnemyGenerationTests
    {
        private const float Tolerance = 0.001f;

        private static IEnumerable<EnemyDefinition> AllTypes => EnemyDefinitions.All;

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _created.Count; i++)
            {
                if (_created[i] != null)
                {
                    Object.DestroyImmediate(_created[i]);
                }
            }

            _created.Clear();
        }

        [Test]
        public void Definitions_IdsAndAssetNames_AreUnique()
        {
            var ids = new HashSet<string>();
            var assetNames = new HashSet<string>();
            foreach (EnemyDefinition definition in EnemyDefinitions.All)
            {
                Assert.IsTrue(ids.Add(definition.Id), definition.Id);
                Assert.IsTrue(assetNames.Add(definition.AssetName), definition.AssetName);
            }
        }

        // --- GDD §7 değerleri ---

        [Test]
        public void Apply_Walker_MatchesGdd()
        {
            EnemyData data = CreateData(EnemyDefinitions.Walker);

            AssertCommon(data, "walker", 60f, 2.2f, 10f, 1f, 1.5f, 10, 1);
            Assert.AreEqual(6f, data.AggroRadius, Tolerance);
            Assert.IsNull(data.PreferredTarget);
            Assert.AreEqual(1f, data.StructureDamageMultiplier, Tolerance);
            Assert.AreEqual(0f, data.KnockbackResistance, Tolerance);
        }

        [Test]
        public void Apply_Runner_MatchesGdd()
        {
            EnemyData data = CreateData(EnemyDefinitions.Runner);

            AssertCommon(data, "runner", 35f, 5f, 6f, 1.5f, 1.3f, 12, 1);
            Assert.AreEqual(6f, data.AggroRadius, Tolerance);
            Assert.IsNull(data.PreferredTarget);
            Assert.AreEqual(1f, data.StructureDamageMultiplier, Tolerance);
        }

        [Test]
        public void Apply_Tank_MatchesGdd()
        {
            EnemyData data = CreateData(EnemyDefinitions.Tank);

            AssertCommon(data, "tank", 400f, 1.6f, 35f, 0.6f, 2f, 60, 5);
            Assert.AreEqual(3f, data.AggroRadius, Tolerance);
            Assert.AreEqual(TargetType.Barricade, data.PreferredTarget);
            Assert.AreEqual(15f, data.PreferredTargetRadius, Tolerance);
            Assert.AreEqual(2f, data.StructureDamageMultiplier, Tolerance);
            Assert.AreEqual(1f, data.KnockbackResistance, Tolerance, "Tank geri itilmez.");
        }

        [TestCaseSource(nameof(AllTypes))]
        public void Apply_KeepsDataScaleAtOne_AndBurrowsBelowBody(EnemyDefinition definition)
        {
            EnemyData data = CreateData(definition);

            // Ölçek Visual'da; EnemyData.Scale de uygulansaydı iki kez büyürdü.
            Assert.AreEqual(1f, data.Scale, Tolerance);
            Assert.Greater(data.BurrowDepth, definition.BodyHeight, "Yerden çıkarken gövde tamamen gömülü başlamalı.");
        }

        // --- Prefab yapısı ---

        [TestCaseSource(nameof(AllTypes))]
        public void BuildRoot_HasLogicComponentsOnRoot(EnemyDefinition definition)
        {
            GameObject root = Build(definition);

            Assert.AreEqual(GameLayers.Enemy, root.layer);
            Assert.IsTrue(root.CompareTag(GameTags.Enemy));
            Assert.IsNotNull(root.GetComponent<NavMeshAgent>());
            Assert.IsNotNull(root.GetComponent<CapsuleCollider>());
            Assert.IsNotNull(root.GetComponent<Health>());
            Assert.IsNotNull(root.GetComponent<Enemy>());
        }

        [TestCaseSource(nameof(AllTypes))]
        public void BuildRoot_VisualHasMeshOnly(EnemyDefinition definition)
        {
            GameObject root = Build(definition);
            Transform visual = root.transform.Find(PlayerGenerator.VisualName);

            Assert.IsNotNull(visual);
            Assert.IsNotNull(visual.GetComponentInChildren<MeshRenderer>());
            Assert.IsEmpty(visual.GetComponentsInChildren<Collider>());
            Assert.IsNull(visual.GetComponentInChildren<Enemy>());
        }

        [TestCaseSource(nameof(AllTypes))]
        public void BuildRoot_ColliderAndAgent_MatchVisualBody(EnemyDefinition definition)
        {
            GameObject root = Build(definition);
            var collider = root.GetComponent<CapsuleCollider>();
            var agent = root.GetComponent<NavMeshAgent>();
            Bounds body = root.transform.Find(PlayerGenerator.VisualName).GetComponentInChildren<MeshRenderer>().bounds;

            Assert.AreEqual(body.size.y, collider.height, 0.01f);
            Assert.AreEqual(body.extents.x, collider.radius, 0.01f);
            Assert.AreEqual(0f, body.min.y, 0.01f, "Gövde yere oturmalı.");
            Assert.AreEqual(collider.height * 0.5f, collider.center.y, Tolerance);
            Assert.AreEqual(collider.radius, agent.radius, Tolerance);
            Assert.AreEqual(collider.height, agent.height, Tolerance);
        }

        [TestCaseSource(nameof(AllTypes))]
        public void BuildRoot_HasHitFeedback(EnemyDefinition definition)
        {
            GameObject root = Build(definition);
            Transform visual = root.transform.Find(VisualRoot.Name);

            var driver = root.GetComponent<AnimationDriver>();
            Assert.IsNotNull(driver);
            Assert.AreSame(visual, driver.Visual);
            Assert.AreEqual(definition.BodyRadius, driver.Pose.Settings.ToppleLift, Tolerance, "Yatan gövde yere gömülmesin.");

            // GDD §7: vuruşta 0.08 sn beyaz flaş.
            var flash = root.GetComponent<HitFlash>();
            Assert.IsNotNull(flash);
            Assert.AreEqual(Color.white, flash.Color);
            Assert.AreEqual(0.08f, flash.Duration, Tolerance);

            Assert.IsNotNull(root.GetComponent<EnemyHitReaction>());
        }

        [TestCaseSource(nameof(AllTypes))]
        public void BuildRoot_HealthBar_AboveHead_OutsideVisual(EnemyDefinition definition)
        {
            GameObject root = Build(definition);

            var bar = root.GetComponentInChildren<EnemyHealthBar>(true);

            Assert.IsNotNull(bar);
            Assert.AreSame(root.transform, bar.transform.parent, "Can barı Visual dışında, kökün çocuğu.");
            Assert.Greater(bar.transform.localPosition.y, definition.BodyHeight);
            Assert.IsNotNull(bar.transform.Find(EnemyHealthBarBuilder.FillName));
            Assert.IsNotNull(bar.transform.Find(EnemyHealthBarBuilder.BackgroundName));
            Assert.IsEmpty(bar.GetComponentsInChildren<Collider>());
        }

        [Test]
        public void BuildRoot_Sizes_FollowGddShapes()
        {
            // GDD §18: Koşucu ince (×0.8 genişlik), Tank büyük (×1.6).
            CapsuleCollider walker = Build(EnemyDefinitions.Walker).GetComponent<CapsuleCollider>();
            CapsuleCollider runner = Build(EnemyDefinitions.Runner).GetComponent<CapsuleCollider>();
            CapsuleCollider tank = Build(EnemyDefinitions.Tank).GetComponent<CapsuleCollider>();

            Assert.AreEqual(0.5f, walker.radius, Tolerance);
            Assert.AreEqual(2f, walker.height, Tolerance);
            Assert.AreEqual(0.4f, runner.radius, Tolerance);
            Assert.AreEqual(2f, runner.height, Tolerance);
            Assert.AreEqual(0.8f, tank.radius, Tolerance);
            Assert.AreEqual(3.2f, tank.height, Tolerance);
        }

        private static void AssertCommon(
            EnemyData data, string id, float health, float speed, float damage, float attackRate, float range, int money, int xp)
        {
            Assert.AreEqual(id, data.Id);
            Assert.IsFalse(string.IsNullOrEmpty(data.DisplayName));
            Assert.AreEqual(health, data.MaxHealth, Tolerance);
            Assert.AreEqual(speed, data.MoveSpeed, Tolerance);
            Assert.AreEqual(damage, data.Damage, Tolerance);
            Assert.AreEqual(attackRate, data.AttackRate, Tolerance);
            Assert.AreEqual(1f / attackRate, data.AttackInterval, Tolerance);
            Assert.AreEqual(range, data.AttackRange, Tolerance);
            Assert.AreEqual(money, data.MoneyReward);
            Assert.AreEqual(xp, data.XpReward);
        }

        private EnemyData CreateData(EnemyDefinition definition)
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            _created.Add(data);
            EnemiesGenerator.Apply(data, definition);
            return data;
        }

        private GameObject Build(EnemyDefinition definition)
        {
            GameObject root = EnemyPrefabs.BuildRoot(definition, null);
            _created.Add(root);
            return root;
        }
    }
}
