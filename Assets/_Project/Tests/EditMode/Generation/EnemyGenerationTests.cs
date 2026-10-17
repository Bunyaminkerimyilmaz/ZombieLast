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

        [Test]
        public void Apply_Spitter_MatchesGdd()
        {
            EnemyData data = CreateData(EnemyDefinitions.Spitter);

            AssertCommon(data, "spitter", 70f, 2.5f, 12f, 0.5f, 10f, 25, 2);
            Assert.AreEqual(6f, data.AggroRadius, Tolerance);
            Assert.AreEqual(TargetType.Turret, data.PreferredTarget, "GDD §7: Tükürücü önce taretleri hedefler.");
            Assert.AreEqual(15f, data.PreferredTargetRadius, Tolerance);
            Assert.IsInstanceOf<RangedAttack>(data.Attack);
        }

        [Test]
        public void Apply_Exploder_MatchesGdd()
        {
            EnemyData data = CreateData(EnemyDefinitions.Exploder);

            AssertCommon(data, "exploder", 50f, 3.5f, 60f, 1f, 1.2f, 20, 2);
            Assert.AreEqual(2f, data.StructureDamageMultiplier, Tolerance, "GDD §7: yapılara ×2.");
            Assert.IsNull(data.PreferredTarget);
            var attack = data.Attack as SuicideExplodeAttack;
            Assert.IsNotNull(attack);
            Assert.AreEqual(3f, attack.Radius, Tolerance, "GDD §7: 3 m alan.");
            Assert.AreEqual(1f, attack.EdgeDamageFraction, Tolerance, "Mesafeyle azalma kapalı.");
            Assert.AreEqual(0.15f, attack.ChainDelay, Tolerance);
            Assert.AreEqual(0f, data.CorpseDuration, Tolerance, "Patlayınca ceset kalmaz.");
            Assert.AreEqual(0f, data.SinkDuration, Tolerance);
        }

        [TestCase("Brute", "brute", 3000f)]
        [TestCase("BruteFinal", "brute_final", 6000f)]
        public void Apply_Brute_MatchesGdd(string assetName, string id, float health)
        {
            EnemyData data = CreateData(Find(assetName));

            // GDD §7: 1.8 hız, 40 hasar (4 m alan), 6 sn'de bir, 4 m menzil, 500 para, 30 XP, geri itilmez.
            AssertCommon(data, id, health, 1.8f, 40f, 1f / 6f, 4f, 500, 30);
            Assert.AreEqual(6f, data.AttackInterval, Tolerance);
            Assert.AreEqual(1f, data.KnockbackResistance, Tolerance, "Geri itilmez.");
            var slam = data.Attack as GroundSlamAttack;
            Assert.IsNotNull(slam);
            Assert.AreEqual(4f, slam.Radius, Tolerance, "GDD §7: 4 m alan.");
            Assert.AreEqual(1f, slam.Windup, Tolerance, "GDD §7: vuruştan 1 sn önce uyarı.");
        }

        [Test]
        public void Ids_MatchNightDefinitions()
        {
            // Geceler (5.3) içeriği bu id'lerle bağlar.
            Assert.AreEqual(NightDefinitions.SpitterId, EnemyDefinitions.Spitter.Id);
            Assert.AreEqual(NightDefinitions.ExploderId, EnemyDefinitions.Exploder.Id);
            Assert.AreEqual(NightDefinitions.BruteId, EnemyDefinitions.Brute.Id);
            Assert.AreEqual(NightDefinitions.BruteFinalId, EnemyDefinitions.BruteFinal.Id);
        }

        [TestCase("Walker", typeof(MeleeAttack))]
        [TestCase("Runner", typeof(MeleeAttack))]
        [TestCase("Tank", typeof(MeleeAttack))]
        [TestCase("Spitter", typeof(RangedAttack))]
        [TestCase("Exploder", typeof(SuicideExplodeAttack))]
        [TestCase("Brute", typeof(GroundSlamAttack))]
        [TestCase("BruteFinal", typeof(GroundSlamAttack))]
        public void Apply_WritesAttackStrategy(string assetName, System.Type expected)
        {
            EnemyDefinition definition = Find(assetName);
            EnemyData data = CreateData(definition);

            Assert.IsInstanceOf(expected, data.Attack);
        }

        [Test]
        public void Data_WithoutAttack_FallsBackToMelee()
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            _created.Add(data);
            var serialized = new UnityEditor.SerializedObject(data);
            serialized.FindProperty(EnemyAttackSetup.AttackFieldName).managedReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.IsInstanceOf<MeleeAttack>(data.Attack);
            Assert.IsTrue(EnemyAttackSetup.EnsureAssigned(data, EnemyAttackKind.Ranged), "Boş saldırı doldurulur.");
            Assert.IsInstanceOf<RangedAttack>(data.Attack);
            Assert.IsFalse(EnemyAttackSetup.EnsureAssigned(data, EnemyAttackKind.Melee), "Dolu saldırıya dokunulmaz.");
            Assert.IsInstanceOf<RangedAttack>(data.Attack);
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

        [Test]
        public void BuildRoot_Exploder_IsPulsingSphere()
        {
            // GDD §18: Patlayan küre, yürürken nabız gibi büyüyüp küçülür.
            GameObject root = Build(EnemyDefinitions.Exploder);
            Transform body = root.transform.Find(VisualRoot.Name).Find(EnemyPrefabs.BodyName);
            var collider = root.GetComponent<CapsuleCollider>();

            Assert.AreEqual("Sphere", body.GetComponent<MeshFilter>().sharedMesh.name);
            Assert.AreEqual(collider.height, collider.radius * 2f, Tolerance, "Collider küre gibi.");
            Assert.AreEqual(0.15f, root.GetComponent<AnimationDriver>().Pose.Settings.WalkPulseScale, Tolerance);
            Assert.AreEqual(0f, Build(EnemyDefinitions.Walker).GetComponent<AnimationDriver>().Pose.Settings.WalkPulseScale, Tolerance);
        }

        [TestCaseSource(nameof(AllTypes))]
        public void BuildRoot_OnlyBossesHaveBossBehaviour(EnemyDefinition definition)
        {
            GameObject root = Build(definition);

            Assert.AreEqual(definition.IsBoss, root.GetComponent<BossBehaviour>() != null);
        }

        [Test]
        public void BuildRoot_Brute_IsGiantCapsule()
        {
            // GDD §18: dev kapsül (×2.5).
            GameObject root = Build(EnemyDefinitions.Brute);
            var collider = root.GetComponent<CapsuleCollider>();

            Assert.AreEqual(1.25f, collider.radius, Tolerance);
            Assert.AreEqual(5f, collider.height, Tolerance);
            Assert.IsNotNull(root.GetComponent<HitFlash>(), "Öfke koyulaşması HitFlash'ın alt ton katmanıyla.");
        }

        [Test]
        public void AttackPrefabs_Warning_ShowsAreaAndGrowsFill()
        {
            GameObject root = EnemyAttackPrefabs.BuildWarning(null);
            _created.Add(root);
            var warning = root.GetComponent<GroundWarning>();
            Transform visual = VisualRoot.Find(root.transform);
            Transform area = visual.Find(EnemyAttackPrefabs.WarningAreaName);
            Transform fill = visual.Find(EnemyAttackPrefabs.WarningFillName);

            Assert.IsEmpty(root.GetComponentsInChildren<Collider>(true));
            warning.Show(4f);
            Assert.AreEqual(8f, area.localScale.x, Tolerance, "Alan = darbe çapı (4 m yarıçap).");
            Assert.AreEqual(0f, fill.localScale.x, Tolerance, "İç disk boş başlar.");
            Assert.Less(area.localScale.y, 0.05f, "İnce disk.");

            warning.SetProgress(0.5f);
            Assert.AreEqual(4f, fill.localScale.x, Tolerance);
            warning.SetProgress(1f);
            Assert.AreEqual(8f, fill.localScale.z, Tolerance, "Dolunca alan kadar.");
            Assert.Greater(fill.localPosition.y, area.localPosition.y, "İç disk alanın üstünde (z-fighting yok).");
        }

        [Test]
        public void BuildRoot_Spitter_HasMuzzleOutsideVisual()
        {
            GameObject root = Build(EnemyDefinitions.Spitter);
            Transform muzzle = root.transform.Find(EnemyPrefabs.MuzzleName);

            Assert.IsNotNull(muzzle);
            Assert.AreSame(root.transform, muzzle.parent, "Bağlantı noktası Visual dışında.");
            Assert.Greater(muzzle.localPosition.y, EnemyDefinitions.Spitter.BodyHeight * 0.5f);
            Assert.Greater(muzzle.localPosition.z, 0f, "Gövdenin önünde.");

            var serialized = new UnityEditor.SerializedObject(root.GetComponent<Enemy>());
            Assert.AreSame(muzzle, serialized.FindProperty("_muzzle").objectReferenceValue);
            Assert.IsNull(Build(EnemyDefinitions.Walker).transform.Find(EnemyPrefabs.MuzzleName), "Yakın dövüşçüde yok.");
        }

        [Test]
        public void AttackPrefabs_Projectile_IsColliderlessEnemyProjectile()
        {
            GameObject root = EnemyAttackPrefabs.BuildProjectile(null);
            _created.Add(root);

            Assert.AreEqual(GameLayers.EnemyProjectile, root.layer);
            Assert.IsNotNull(root.GetComponent<ArcProjectile>());
            Assert.IsEmpty(root.GetComponentsInChildren<Collider>(true), "Çarpışma küre taramasıyla; collider yok.");
            Assert.IsNotNull(VisualRoot.Find(root.transform).GetComponentInChildren<MeshRenderer>());
        }

        [Test]
        public void AttackPrefabs_Explosion_ReachesRadius_AndFades()
        {
            GameObject root = EnemyAttackPrefabs.BuildExplosion(3f, null);
            _created.Add(root);
            var flash = root.GetComponent<ExplosionFlash>();
            Transform shell = VisualRoot.Find(root.transform).Find(EnemyAttackPrefabs.ShellName);

            Assert.IsNotNull(root.GetComponent<PooledLifetime>());
            Assert.AreEqual(3f, flash.Radius, Tolerance);
            Assert.IsEmpty(root.GetComponentsInChildren<Collider>(true));

            flash.Apply(0f);
            Assert.AreEqual(0.6f, shell.localScale.x, Tolerance, "Başlangıç yarıçapı 0.3 m.");
            Assert.Greater(root.GetComponent<Light>().intensity, 0f);

            flash.Apply(1f);
            Assert.AreEqual(6f, shell.localScale.x, Tolerance, "Patlama yarıçapına (3 m) ulaşır.");
            Assert.AreEqual(0f, root.GetComponent<Light>().intensity, Tolerance);
        }

        private static EnemyDefinition Find(string assetName)
        {
            foreach (EnemyDefinition definition in EnemyDefinitions.All)
            {
                if (string.Equals(definition.AssetName, assetName, System.StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            throw new System.ArgumentException(assetName);
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
