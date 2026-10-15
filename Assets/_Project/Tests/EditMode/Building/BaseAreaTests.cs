using LastHaven.Building;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Üs alanı (GDD §11): yatay düzlemde daire içi kontrolü, varsayılan yarıçap ve BaseArea köprüsü.</summary>
    public class BaseAreaTests
    {
        private const float Tolerance = 0.001f;

        private GameObject _gameObject;
        private BaseAreaConfig _config;

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
            }

            if (_config != null)
            {
                Object.DestroyImmediate(_config);
            }
        }

        [Test]
        public void Contains_Center_IsInside()
        {
            Assert.IsTrue(BaseAreaMath.Contains(Vector3.zero, 14f, Vector3.zero));
        }

        [Test]
        public void Contains_Boundary_IsInclusive()
        {
            Assert.IsTrue(BaseAreaMath.Contains(Vector3.zero, 14f, new Vector3(14f, 0f, 0f)));
            Assert.IsTrue(BaseAreaMath.Contains(Vector3.zero, 5f, new Vector3(-3f, 0f, -4f)));
        }

        [Test]
        public void Contains_JustOutside_IsOutside()
        {
            Assert.IsFalse(BaseAreaMath.Contains(Vector3.zero, 14f, new Vector3(0f, 0f, 14.01f)));
            Assert.IsFalse(BaseAreaMath.Contains(Vector3.zero, 14f, new Vector3(10f, 0f, 10f)), "Köşegende 14.14 m.");
        }

        [Test]
        public void Contains_IgnoresHeight()
        {
            Assert.IsTrue(BaseAreaMath.Contains(new Vector3(0f, -5f, 0f), 5f, new Vector3(3f, 50f, 4f)));
        }

        [Test]
        public void Contains_UsesCenterOffset()
        {
            var center = new Vector3(20f, 0f, -10f);

            Assert.IsTrue(BaseAreaMath.Contains(center, 2f, new Vector3(21f, 0f, -11f)));
            Assert.IsFalse(BaseAreaMath.Contains(center, 2f, Vector3.zero));
        }

        [Test]
        public void Contains_NegativeRadius_NothingInside()
        {
            Assert.IsFalse(BaseAreaMath.Contains(Vector3.zero, -1f, Vector3.zero));
        }

        [Test]
        public void Config_DefaultRadius_MatchesGdd()
        {
            _config = ScriptableObject.CreateInstance<BaseAreaConfig>();

            Assert.AreEqual(14f, _config.Radius, Tolerance);
        }

        [Test]
        public void BaseArea_CenterIsTransform_RadiusFromConfig()
        {
            BaseArea area = CreateArea(new Vector3(10f, 0f, 10f), withConfig: true);

            Assert.AreEqual(new Vector3(10f, 0f, 10f), area.Center);
            Assert.AreEqual(14f, area.Radius, Tolerance);
            Assert.IsTrue(area.Contains(new Vector3(10f, 0f, 24f)));
            Assert.IsFalse(area.Contains(Vector3.zero), "Merkeze 14.14 m.");
        }

        [Test]
        public void BaseArea_WithoutConfig_HasZeroRadius()
        {
            BaseArea area = CreateArea(Vector3.zero, withConfig: false);

            Assert.AreEqual(0f, area.Radius);
            Assert.IsTrue(area.Contains(Vector3.zero));
            Assert.IsFalse(area.Contains(new Vector3(0.1f, 0f, 0f)));
        }

        private BaseArea CreateArea(Vector3 position, bool withConfig)
        {
            _gameObject = new GameObject("BaseArea");
            _gameObject.transform.position = position;
            var area = _gameObject.AddComponent<BaseArea>();
            if (withConfig)
            {
                _config = ScriptableObject.CreateInstance<BaseAreaConfig>();
                var serialized = new SerializedObject(area);
                serialized.FindProperty("_config").objectReferenceValue = _config;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return area;
        }
    }
}
