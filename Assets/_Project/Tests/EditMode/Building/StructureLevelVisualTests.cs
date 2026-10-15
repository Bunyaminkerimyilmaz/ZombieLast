using LastHaven.Building;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Seviye görseli (GDD §18): sadece seviye materyali kullanan yuvalar değişir; kullanıcının diğer materyalleri kalır.</summary>
    public class StructureLevelVisualTests
    {
        private Material _wood;
        private Material _metal;
        private Material _concrete;
        private Material _other;
        private BuildableLevel[] _levels;
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            _wood = new Material(shader) { name = "Wood" };
            _metal = new Material(shader) { name = "Metal" };
            _concrete = new Material(shader) { name = "Concrete" };
            _other = new Material(shader) { name = "Other" };
            _levels = new[]
            {
                new BuildableLevel("Ahşap", 50, 300f, _wood),
                new BuildableLevel("Metal", 120, 800f, _metal),
                new BuildableLevel("Beton", 300, 2000f, _concrete),
            };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_wood);
            Object.DestroyImmediate(_metal);
            Object.DestroyImmediate(_concrete);
            Object.DestroyImmediate(_other);
        }

        [Test]
        public void Resolve_LevelMaterial_BecomesTargetLevelMaterial()
        {
            Assert.AreSame(_metal, StructureLevelVisual.Resolve(_wood, _levels, 1));
            Assert.AreSame(_concrete, StructureLevelVisual.Resolve(_wood, _levels, 2));
            Assert.AreSame(_wood, StructureLevelVisual.Resolve(_concrete, _levels, 0), "Geri dönüş de (havuz) çalışır.");
        }

        [Test]
        public void Resolve_OtherMaterial_IsUntouched()
        {
            Assert.AreSame(_other, StructureLevelVisual.Resolve(_other, _levels, 2));
            Assert.IsNull(StructureLevelVisual.Resolve(null, _levels, 2));
        }

        [Test]
        public void Resolve_TargetLevelWithoutMaterial_KeepsOriginal()
        {
            var levels = new[] { new BuildableLevel("1", 10, 10f, _wood), new BuildableLevel("2", 10, 10f) };

            Assert.AreSame(_wood, StructureLevelVisual.Resolve(_wood, levels, 1));
        }

        [Test]
        public void Resolve_InvalidLevel_KeepsOriginal()
        {
            Assert.AreSame(_wood, StructureLevelVisual.Resolve(_wood, _levels, 3));
            Assert.AreSame(_wood, StructureLevelVisual.Resolve(_wood, _levels, -1));
            Assert.AreSame(_wood, StructureLevelVisual.Resolve(_wood, null, 1));
        }

        [Test]
        public void ApplyAndRestore_ChangesOnlyLevelSlots()
        {
            Renderer renderer = CreateVisual(_wood, _other);
            var visual = new StructureLevelVisual(VisualRoot.Find(_root.transform));

            visual.Apply(_levels, 1);

            Assert.AreEqual(1, visual.RendererCount);
            Assert.AreSame(_metal, renderer.sharedMaterials[0]);
            Assert.AreSame(_other, renderer.sharedMaterials[1]);

            visual.Restore();

            Assert.AreSame(_wood, renderer.sharedMaterials[0]);
            Assert.AreSame(_other, renderer.sharedMaterials[1]);
        }

        [Test]
        public void New_WithoutVisual_DoesNothing()
        {
            var visual = new StructureLevelVisual(null);

            Assert.DoesNotThrow(() => visual.Apply(_levels, 2));
            Assert.AreEqual(0, visual.RendererCount);
        }

        private Renderer CreateVisual(params Material[] materials)
        {
            _root = new GameObject("StructureLevelVisualTests");
            var visual = new GameObject(VisualRoot.Name);
            visual.transform.SetParent(_root.transform, false);
            var renderer = visual.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            return renderer;
        }
    }
}
