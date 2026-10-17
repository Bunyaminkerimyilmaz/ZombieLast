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

        [Test]
        public void Tint_MixesTowardTintByAlpha_AndKeepsOriginalAlpha()
        {
            var original = new Color(0.2f, 0.8f, 0.2f, 0.5f);
            var tint = new Color(0f, 0f, 1f, 0.25f);

            Color mixed = HitFlash.Tint(original, tint);

            Assert.AreEqual(0.15f, mixed.r, 0.001f);
            Assert.AreEqual(0.6f, mixed.g, 0.001f);
            Assert.AreEqual(0.4f, mixed.b, 0.001f);
            Assert.AreEqual(0.5f, mixed.a, 0.001f, "Saydamlık korunur.");
            Assert.AreEqual(original, HitFlash.Tint(original, new Color(0f, 0f, 1f, 0f)), "Alfa 0: değişmez.");
        }

        [Test]
        public void SetTint_TintsMaterialColor_ClearTintRestores()
        {
            var green = new Color(0.2f, 0.8f, 0.2f, 1f);
            _lit.SetColor(BaseColorId, green);
            Renderer body = CreateRenderer(_visual, _lit);
            var flash = _root.AddComponent<HitFlash>();
            var tint = new Color(0.35f, 0.6f, 1f, 0.45f);

            flash.SetTint(tint);

            Assert.IsTrue(flash.HasTint);
            AssertColor(HitFlash.Tint(green, tint), GetBaseColor(body), "Materyalin kendi rengi tona karışır.");

            flash.ClearTint();

            Assert.IsFalse(flash.HasTint);
            Assert.IsFalse(body.HasPropertyBlock(), "Ton kalkınca materyal rengine döner.");
        }

        [Test]
        public void TintLayers_BaseUnderStatus_ClearingOneKeepsTheOther()
        {
            // 12.2: boss öfkesi (alt katman) ile yavaşlama tonu (üst katman) birbirini silmez.
            var red = new Color(0.4f, 0.1f, 0.1f, 1f);
            _lit.SetColor(BaseColorId, red);
            Renderer body = CreateRenderer(_visual, _lit);
            var flash = _root.AddComponent<HitFlash>();
            var dark = new Color(0f, 0f, 0f, 0.5f);
            var blue = new Color(0f, 0f, 1f, 0.5f);

            flash.SetTint(TintLayer.Base, dark);
            flash.SetTint(blue);

            Assert.IsTrue(flash.HasTintOn(TintLayer.Base));
            Assert.IsTrue(flash.HasTintOn(TintLayer.Status));
            AssertColor(HitFlash.Tint(HitFlash.Tint(red, dark), blue), GetBaseColor(body), "Alttan üste karışır.");

            flash.ClearTint();
            AssertColor(HitFlash.Tint(red, dark), GetBaseColor(body), "Yavaşlama bitince öfke tonu kalır.");

            flash.ClearTint(TintLayer.Base);
            Assert.IsFalse(flash.HasTint);
            Assert.IsFalse(body.HasPropertyBlock());
        }

        [Test]
        public void Flash_OverridesTint_ThenTintReturns()
        {
            Renderer body = CreateRenderer(_visual, _lit);
            var flash = _root.AddComponent<HitFlash>();
            var tint = new Color(0f, 0f, 1f, 0.5f);
            flash.SetTint(tint);
            Color tinted = GetBaseColor(body);

            flash.Flash(Color.white, 0.08f);
            Assert.AreEqual(Color.white, GetBaseColor(body), "Flaş sürerken flaş rengi.");

            flash.Advance(0.1f);
            Assert.IsFalse(flash.IsFlashing);
            Assert.IsTrue(body.HasPropertyBlock(), "Flaş bitince blok ton için kalır.");
            AssertColor(tinted, GetBaseColor(body), "Ton geri gelir.");
        }

        [Test]
        public void ClearTint_WhileFlashing_FlashEndsClean()
        {
            Renderer body = CreateRenderer(_visual, _lit);
            var flash = _root.AddComponent<HitFlash>();
            flash.SetTint(new Color(0f, 0f, 1f, 0.5f));
            flash.Flash(Color.white, 0.08f);

            flash.ClearTint();
            Assert.AreEqual(Color.white, GetBaseColor(body), "Flaş sürüyor.");

            flash.Advance(0.1f);
            Assert.IsFalse(body.HasPropertyBlock());
        }

        [Test]
        public void OnDespawned_ClearsTint()
        {
            Renderer body = CreateRenderer(_visual, _lit);
            var flash = _root.AddComponent<HitFlash>();
            flash.SetTint(new Color(0f, 0f, 1f, 0.5f));

            ((IPoolable)flash).OnDespawned();

            Assert.IsFalse(flash.HasTint);
            Assert.IsFalse(body.HasPropertyBlock());
        }

        // Property block renkleri renk uzayı dönüşümünden geçebilir; küçük yuvarlama payı.
        private static void AssertColor(Color expected, Color actual, string message)
        {
            Assert.AreEqual(expected.r, actual.r, 0.002f, message);
            Assert.AreEqual(expected.g, actual.g, 0.002f, message);
            Assert.AreEqual(expected.b, actual.b, 0.002f, message);
            Assert.AreEqual(expected.a, actual.a, 0.002f, message);
        }

        private static Color GetBaseColor(Renderer renderer)
        {
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            return block.GetColor(BaseColorId);
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
