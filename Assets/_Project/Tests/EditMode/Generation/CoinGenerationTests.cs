using LastHaven.Core;
using LastHaven.Economy;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Jeton prefab'ı (7.2): Pickup layer/tag, trigger collider, kök/Visual ayrımı; üretilen asset ve config bağlantısı.
    /// Hiyerarşi testleri asset oluşturmaz (sahnede kurulup silinir).
    /// </summary>
    public class CoinGenerationTests
    {
        private const float Tolerance = 0.001f;

        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }
        }

        [Test]
        public void BuildRoot_RootIsPickupWithTriggerAndCoin()
        {
            GameObject root = Build();

            Assert.AreEqual(GameLayers.Pickup, root.layer);
            Assert.IsTrue(root.CompareTag(GameTags.Pickup));
            Assert.IsNotNull(root.GetComponent<Coin>());
            Assert.IsNull(root.GetComponent<Renderer>(), "Kökte mesh olmamalı.");

            var collider = root.GetComponent<SphereCollider>();
            Assert.IsNotNull(collider);
            Assert.IsTrue(collider.isTrigger, "Oyuncunun CharacterController'ı jetona takılmamalı.");
        }

        [Test]
        public void BuildRoot_VisualIsUprightFlatCylinderWithoutCollider()
        {
            Transform visual = VisualRoot.Find(Build().transform);
            Assert.IsNotNull(visual);
            Assert.AreEqual(GameLayers.Pickup, visual.gameObject.layer);

            Transform body = visual.Find(CoinPrefab.BodyName);
            Assert.IsNotNull(body);
            Assert.IsNotNull(body.GetComponent<MeshRenderer>());
            Assert.IsNull(body.GetComponent<Collider>(), "Görselde collider olmamalı.");
            Assert.Less(body.localScale.y, body.localScale.x, "Yassı silindir.");

            // Silindirin ekseni (yerel Y) yatay olmalı: jeton dik durur.
            Vector3 axis = body.localRotation * Vector3.up;
            Assert.AreEqual(0f, axis.y, Tolerance);
        }

        [Test]
        public void Asset_ExistsAndIsLinkedToConfig()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<Coin>(CoinPrefab.PrefabPath);
            var config = AssetDatabase.LoadAssetAtPath<EconomyConfig>(EconomyGenerator.ConfigPath);
            if (prefab == null || config == null)
            {
                Assert.Inconclusive("Jeton prefab'ı veya EconomyConfig yok; Tools/Last Haven/Rebuild All çalıştır.");
            }

            Assert.AreSame(prefab, config.CoinPrefab);
            Assert.Greater(config.CoinPrewarm, 0);
        }

        private GameObject Build()
        {
            _root = CoinPrefab.BuildRoot(null);
            return _root;
        }
    }
}
