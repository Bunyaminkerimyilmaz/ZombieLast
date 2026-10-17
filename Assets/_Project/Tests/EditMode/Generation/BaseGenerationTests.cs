using LastHaven.Building;
using LastHaven.Core;
using LastHaven.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Üs üreticisi: GeneratorData varsayılanları (GDD §12) ve jeneratör prefab yapısı (kök/Visual ayrımı, üs ışıkları).
    /// Asset oluşturmaz; veri bellekte, prefab hiyerarşisi sahnede kurulup silinir.
    /// </summary>
    public class BaseGenerationTests
    {
        private const float Tolerance = 0.001f;

        private GameObject _root;
        private ScriptableObject _data;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            if (_data != null)
            {
                Object.DestroyImmediate(_data);
            }
        }

        [Test]
        public void GeneratorData_Defaults_MatchGdd()
        {
            var data = ScriptableObject.CreateInstance<GeneratorData>();
            _data = data;

            Assert.AreEqual(1500f, data.MaxHealth, Tolerance);
            Assert.AreEqual(150, data.FuelCapacity);
            Assert.AreEqual(0.4f, data.BaseFuelConsumption, Tolerance);
            Assert.AreEqual(0.05f, data.FuelConsumptionPerTurret, Tolerance);
            Assert.AreEqual(30, data.EmergencyFuelPrice);
            Assert.AreEqual(10, data.EmergencyFuelAmount);
            Assert.AreEqual(1f, data.EmergencyFuelHoldDuration, Tolerance);
            Assert.AreEqual(2f, data.EmergencyFuelRange, Tolerance);
            Assert.AreEqual("Acil yakıt +10 (30 para)", data.FormatEmergencyFuelPrompt());
        }

        [Test]
        public void GeneratorDataAsset_HasGddFuelValues()
        {
            GeneratorData data = AssetDatabase.LoadAssetAtPath<GeneratorData>(BaseGenerator.GeneratorDataPath);

            Assert.IsNotNull(data, BaseGenerator.GeneratorDataPath);
            Assert.AreEqual(150, data.FuelCapacity, "float → int dönüşümünde değer korunmalı.");
            Assert.AreEqual(10, data.EmergencyFuelAmount);
            Assert.AreEqual(30, data.EmergencyFuelPrice);
        }

        [Test]
        public void BuildRoot_HasLogicComponentsOnRoot()
        {
            GameObject root = Build();

            Assert.AreEqual(GameLayers.Structure, root.layer);
            Assert.IsTrue(root.CompareTag(GameTags.Generator));
            Assert.IsNotNull(root.GetComponent<Health>());
            Assert.IsNotNull(root.GetComponent<Generator>());

            var collider = root.GetComponent<BoxCollider>();
            Assert.IsNotNull(collider);
            Assert.AreEqual(GeneratorPrefab.Size, collider.size);
            Assert.AreEqual(GeneratorPrefab.Size.y * 0.5f, collider.center.y, Tolerance, "Küp yere oturmalı.");
        }

        [Test]
        public void BuildRoot_Obstacle_DoesNotCarve()
        {
            var obstacle = Build().GetComponent<NavMeshObstacle>();

            Assert.IsNotNull(obstacle);
            Assert.IsFalse(obstacle.carving, "Carve olursa merkez NavMesh dışında kalır, yol kısmi çıkar.");
            Assert.AreEqual(GeneratorPrefab.Size, obstacle.size);
        }

        [Test]
        public void BuildRoot_Generator_IsGeneratorTarget()
        {
            var generator = Build().GetComponent<Generator>();

            Assert.AreEqual(TargetType.Generator, generator.TargetType);
            Assert.AreSame(generator.transform, generator.Transform);
        }

        [Test]
        public void BuildRoot_HasPowerAndEmergencyFuelOnRoot()
        {
            GameObject root = Build();
            var power = root.GetComponent<GeneratorPower>();
            var station = root.GetComponent<EmergencyFuelStation>();

            Assert.IsNotNull(power);
            Assert.IsNotNull(station);
            Assert.IsTrue(power.IsPowered, "Start'tan önce güç var sayılır (yakıt durumu Start'ta okunur).");
            Assert.IsFalse(power.IsConsuming, "Gece değil.");
            Assert.IsFalse(station.CanInteract, "Gece değil, cüzdan yok.");
        }

        [Test]
        public void EmergencyFuelStation_UsesDataAndMeasuresToGeneratorEdge()
        {
            GameObject root = Build();
            var data = ScriptableObject.CreateInstance<GeneratorData>();
            _data = data;
            var serialized = new SerializedObject(root.GetComponent<Generator>());
            serialized.FindProperty("_data").objectReferenceValue = data;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Physics.SyncTransforms();
            var station = root.GetComponent<EmergencyFuelStation>();

            Assert.AreEqual("Acil yakıt +10 (30 para)", station.Prompt);
            Assert.AreEqual(1f, station.HoldDuration, Tolerance);
            Assert.AreEqual(2f, station.InteractionRange, Tolerance);
            Assert.AreEqual(2f, station.GetDistance(new Vector3(3f, 0f, 0f)), Tolerance, "2×2 küpün kenarından.");
            Assert.AreEqual(0f, station.GetDistance(new Vector3(0.5f, 0f, 0f)), Tolerance);
            Assert.AreEqual(GeneratorPrefab.Size.y, station.PromptPosition.y, Tolerance, "İpucu küpün tepesinde.");
        }

        [Test]
        public void BuildRoot_VisualHasMeshOnly()
        {
            GameObject root = Build();
            Transform visual = VisualRoot.Find(root.transform);

            Assert.IsNotNull(visual);
            var renderer = visual.GetComponentInChildren<MeshRenderer>();
            Assert.IsNotNull(renderer);
            Assert.IsEmpty(visual.GetComponentsInChildren<Collider>());
            Assert.IsEmpty(visual.GetComponentsInChildren<Light>(), "Işıklar Visual dışında olmalı (model değişince kaybolmasın).");
            Assert.AreEqual(0f, renderer.bounds.min.y, 0.01f, "Gövde yere oturmalı.");
            Assert.AreEqual(GeneratorPrefab.Size.x, renderer.bounds.size.x, 0.01f);
        }

        [Test]
        public void BuildRoot_HasFourPointLights_InsideBaseArea()
        {
            GameObject root = Build();
            Transform group = root.transform.Find(GeneratorPrefab.LightsName);
            var config = ScriptableObject.CreateInstance<BaseAreaConfig>();
            _data = config;

            Assert.IsNotNull(group);
            Assert.AreSame(root.transform, group.parent);
            var lights = group.GetComponent<BaseLights>();
            Assert.IsNotNull(lights);
            Assert.AreEqual(GeneratorPrefab.LightCount, lights.Lights.Count);
            for (int i = 0; i < lights.Lights.Count; i++)
            {
                Light light = lights.Lights[i];
                Assert.IsNotNull(light);
                Assert.AreEqual(LightType.Point, light.type);
                Assert.AreEqual(LightShadows.None, light.shadows);
                Assert.IsTrue(light.transform.IsChildOf(group));
                Assert.IsTrue(BaseAreaMath.Contains(Vector3.zero, config.Radius, light.transform.position));
                Assert.Greater(light.transform.position.y, GeneratorPrefab.Size.y, "Işık jeneratörün üstünde.");
                for (int j = 0; j < i; j++)
                {
                    Assert.AreNotEqual(lights.Lights[j].transform.position, light.transform.position);
                }
            }
        }

        [Test]
        public void BaseLights_SetLit_TogglesAllLights()
        {
            var lights = Build().GetComponentInChildren<BaseLights>();

            lights.SetLit(false);

            Assert.IsFalse(lights.IsLit);
            foreach (Light light in lights.Lights)
            {
                Assert.IsFalse(light.enabled);
            }

            lights.SetLit(true);

            Assert.IsTrue(lights.IsLit);
            foreach (Light light in lights.Lights)
            {
                Assert.IsTrue(light.enabled);
            }
        }

        private GameObject Build()
        {
            _root = GeneratorPrefab.BuildRoot(null);
            return _root;
        }
    }
}
