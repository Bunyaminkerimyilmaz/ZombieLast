using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// HitFlash: Visual altındaki renderer'lara MaterialPropertyBlock ile renk, süre sonunda temizleme,
    /// _BaseColor/_EmissionColor olmayan materyali atlama.
    /// </summary>
    public class HitFlashTests
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private GameObject _root;
        private Transform _visual;
        private Material _lit;
        private Material _noColor;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Entity");
            _visual = new GameObject(VisualRoot.Name).transform;
            _visual.SetParent(_root.transform, false);
            _lit = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _noColor = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_lit);
            Object.DestroyImmediate(_noColor);
        }

        [Test]
        public void Flash_SetsColorOnVisualRenderers_ThenClearsAfterDuration()
        {
            Renderer body = CreateRenderer(_visual, _lit);
            var flash = _root.AddComponent<HitFlash>();

            flash.Flash(Color.white, 0.08f);

            Assert.IsTrue(flash.IsFlashing);
            Assert.IsTrue(body.HasPropertyBlock());
            var block = new MaterialPropertyBlock();
            body.GetPropertyBlock(block);
            Assert.AreEqual(Color.white, block.GetColor(BaseColorId));

            flash.Advance(0.05f);
            Assert.IsTrue(flash.IsFlashing);

            flash.Advance(0.05f);
            Assert.IsFalse(flash.IsFlashing);
            Assert.IsFalse(body.HasPropertyBlock(), "Süre bitince blok temizlenmeli; materyale dokunulmaz.");
        }

        [Test]
        public void Flash_SkipsRenderersOutsideVisual_AndWithoutColorProperties()
        {
            CreateRenderer(_visual, _lit);
            Renderer noColor = CreateRenderer(_visual, _noColor);
            Renderer outside = CreateRenderer(_root.transform, _lit);
            var flash = _root.AddComponent<HitFlash>();

            flash.Flash();

            Assert.AreEqual(1, flash.TargetCount);
            Assert.IsFalse(noColor.HasPropertyBlock());
            Assert.IsFalse(outside.HasPropertyBlock());
        }

        [Test]
        public void Flash_WhileFlashing_RestartsDuration()
        {
            CreateRenderer(_visual, _lit);
            var flash = _root.AddComponent<HitFlash>();

            flash.Flash(Color.red, 0.1f);
            flash.Advance(0.08f);
            flash.Flash(Color.red, 0.1f);
            flash.Advance(0.08f);

            Assert.IsTrue(flash.IsFlashing);
        }

        [Test]
        public void OnDespawned_StopsFlash()
        {
            Renderer body = CreateRenderer(_visual, _lit);
            var flash = _root.AddComponent<HitFlash>();
            flash.Flash();

            ((IPoolable)flash).OnDespawned();

            Assert.IsFalse(flash.IsFlashing);
            Assert.IsFalse(body.HasPropertyBlock());
        }

        private static Renderer CreateRenderer(Transform parent, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(parent, false);
            var renderer = cube.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            return renderer;
        }
    }
}
