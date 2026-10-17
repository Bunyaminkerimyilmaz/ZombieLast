using LastHaven.Building;
using LastHaven.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Taretin görsel köprüsü: kafa yaw'a döner, güç yokken aşağı eğilir ve gösterge söner; havuza dönünce dinlenme pozu.</summary>
    public class TurretVisualTests
    {
        private const float Tolerance = 0.01f;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private GameObject _root;
        private Transform _head;
        private Renderer _indicator;
        private TurretVisual _visual;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TurretVisualTests");
            var visual = new GameObject(VisualRoot.Name).transform;
            visual.SetParent(_root.transform, false);
            _head = new GameObject("Head").transform;
            _head.SetParent(visual, false);
            _head.localPosition = new Vector3(0f, 1.2f, 0f);
            _indicator = new GameObject("Indicator").AddComponent<MeshRenderer>();
            _indicator.transform.SetParent(_head, false);

            _visual = _root.AddComponent<TurretVisual>();
            var serialized = new SerializedObject(_visual);
            serialized.FindProperty("_head").objectReferenceValue = _head;
            serialized.FindProperty("_indicator").objectReferenceValue = _indicator;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void Apply_Powered_TurnsHeadToYawAndLightsIndicator()
        {
            _visual.Snap(0f, true);

            _visual.Apply(90f, true, 0.1f);

            AssertVector(Vector3.right, _head.forward);
            Assert.AreEqual(0f, _visual.Pitch, Tolerance);
            Assert.IsTrue(_visual.IsIndicatorLit);
            Assert.Greater(GetIndicatorColor().g, 0.5f, "Yeşil.");
        }

        [Test]
        public void Apply_Unpowered_TiltsDownGraduallyAndDimsIndicator()
        {
            _visual.Snap(0f, true);

            _visual.Apply(0f, false, 0.1f);
            Assert.That(_visual.Pitch, Is.GreaterThan(0f).And.LessThan(30f), "Yavaşça eğilir.");

            _visual.Apply(0f, false, 5f);
            Assert.AreEqual(30f, _visual.Pitch, Tolerance);
            Assert.Less(_head.forward.y, -0.4f, "Namlu aşağı bakıyor.");
            Assert.IsFalse(_visual.IsIndicatorLit);
            Assert.Less(GetIndicatorColor().g, 0.3f, "Sönük.");
        }

        [Test]
        public void Apply_PowerReturns_RaisesHeadAndRelightsIndicator()
        {
            _visual.Snap(0f, false);
            Assert.AreEqual(30f, _visual.Pitch, Tolerance, "Snap anında oturtur.");

            _visual.Apply(0f, true, 5f);

            Assert.AreEqual(0f, _visual.Pitch, Tolerance);
            Assert.IsTrue(_visual.IsIndicatorLit);
        }

        [Test]
        public void Apply_KeepsHeadRestRotationRelativeToRoot()
        {
            Object.DestroyImmediate(_visual);
            _head.localRotation = Quaternion.Euler(-90f, 0f, 0f); // Ör. FBX modelinin dinlenme dönüşü.
            _visual = _root.AddComponent<TurretVisual>();
            var serialized = new SerializedObject(_visual);
            serialized.FindProperty("_head").objectReferenceValue = _head;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            _visual.Apply(0f, true, 0f);

            AssertVector(Quaternion.Euler(-90f, 0f, 0f) * Vector3.forward, _head.forward);
        }

        [Test]
        public void OnDespawned_RestoresHeadRestRotation()
        {
            _visual.Apply(135f, false, 5f);

            _visual.OnDespawned();

            AssertVector(Vector3.forward, _head.forward);
            Assert.AreEqual(0f, _visual.Pitch);
        }

        [Test]
        public void MissingParts_AreSkipped()
        {
            var bare = new GameObject("Bare").AddComponent<TurretVisual>();
            try
            {
                Assert.DoesNotThrow(() => bare.Apply(45f, false, 1f));
            }
            finally
            {
                Object.DestroyImmediate(bare.gameObject);
            }
        }

        private Color GetIndicatorColor()
        {
            var block = new MaterialPropertyBlock();
            _indicator.GetPropertyBlock(block);
            return block.GetColor(BaseColorId);
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, "x");
            Assert.AreEqual(expected.y, actual.y, Tolerance, "y");
            Assert.AreEqual(expected.z, actual.z, Tolerance, "z");
        }
    }
}
