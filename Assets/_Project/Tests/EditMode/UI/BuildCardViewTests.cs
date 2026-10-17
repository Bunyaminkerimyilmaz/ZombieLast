using LastHaven.Building;
using LastHaven.Editor;
using LastHaven.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// İnşa çubuğu kartı (10.3), gerçek <c>BuildCard.uxml</c> şablonuyla: kısayol numarası, ad, fiyat; ikon, yoksa 1. seviye
    /// materyalinin rengi, o da yoksa USS varsayılanı; seçili vurgu ve para yetmeyince soluk + kırmızı fiyat.
    /// </summary>
    public class BuildCardViewTests
    {
        private const float Tolerance = 0.001f;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Color Brown = new Color(0.45f, 0.3f, 0.15f, 0.5f);

        private VisualTreeAsset _template;
        private Material _material;
        private Texture2D _texture;
        private Sprite _icon;
        private BuildableData _data;

        [SetUp]
        public void SetUp()
        {
            _template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIGenerator.BuildCardPath);
            Assert.IsNotNull(_template, UIGenerator.BuildCardPath);
            _material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _material.SetColor(BaseColorId, Brown);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
            Object.DestroyImmediate(_material);
            Object.DestroyImmediate(_icon);
            Object.DestroyImmediate(_texture);
        }

        [Test]
        public void Create_WritesKeyNameAndBuildCost()
        {
            _data = CreateData(_material, null);

            BuildCardView card = BuildCardView.Create(_template, 2, _data);

            Assert.IsNotNull(card);
            Assert.IsTrue(card.Root.ClassListContains(BuildCardView.CardClass));
            Assert.IsNull(card.Root.parent, "Kart şablon sarmalayıcısından ayrılmalı.");
            Assert.AreEqual(2, card.SlotIndex);
            Assert.AreEqual("3", card.Root.Q<Label>(HudElementNames.CardKey).text, "Kısayol numarası yuva + 1.");
            Assert.AreEqual("Barikat", card.Root.Q<Label>(HudElementNames.CardName).text);
            Assert.AreEqual("50", card.Root.Q<Label>(HudElementNames.CardPrice).text);
        }

        [Test]
        public void Create_WithoutIcon_UsesOpaqueLevelMaterialColor()
        {
            _data = CreateData(_material, null);

            BuildCardView card = BuildCardView.Create(_template, 0, _data);

            VisualElement swatch = card.Root.Q(HudElementNames.CardSwatch);
            Color color = swatch.style.backgroundColor.value;
            Assert.AreEqual(Brown.r, color.r, Tolerance);
            Assert.AreEqual(Brown.g, color.g, Tolerance);
            Assert.AreEqual(Brown.b, color.b, Tolerance);
            Assert.AreEqual(1f, color.a, Tolerance, "Kutu opak.");
            Assert.AreEqual(StyleKeyword.Null, swatch.style.backgroundImage.keyword, "İkon yokken görsel yazılmaz.");
        }

        [Test]
        public void Create_WithoutIconOrMaterial_LeavesSwatchToUss()
        {
            _data = CreateData(null, null);

            BuildCardView card = BuildCardView.Create(_template, 0, _data);

            VisualElement swatch = card.Root.Q(HudElementNames.CardSwatch);
            Assert.AreEqual(StyleKeyword.Null, swatch.style.backgroundColor.keyword, "Renk USS'teki varsayılandan gelmeli.");
            Assert.IsFalse(BuildCardView.TryGetColor(_data, out _));
        }

        [Test]
        public void Create_WithIcon_UsesIconInsteadOfColor()
        {
            _texture = new Texture2D(4, 4);
            _icon = Sprite.Create(_texture, new Rect(0f, 0f, 4f, 4f), Vector2.one * 0.5f);
            _data = CreateData(_material, _icon);

            BuildCardView card = BuildCardView.Create(_template, 0, _data);

            VisualElement swatch = card.Root.Q(HudElementNames.CardSwatch);
            Assert.AreSame(_icon, swatch.style.backgroundImage.value.sprite);
            Assert.AreEqual(StyleKeyword.Null, swatch.style.backgroundColor.keyword);
        }

        [Test]
        public void Apply_Selected_TogglesSelectedClass()
        {
            _data = CreateData(_material, null);
            BuildCardView card = BuildCardView.Create(_template, 0, _data);

            card.Apply(true, 300);
            Assert.IsTrue(card.Root.ClassListContains(BuildCardView.SelectedClass));

            card.Apply(false, 300);
            Assert.IsFalse(card.Root.ClassListContains(BuildCardView.SelectedClass));
        }

        [Test]
        public void Apply_NotEnoughMoney_DimsCardAndMarksPrice()
        {
            _data = CreateData(_material, null);
            BuildCardView card = BuildCardView.Create(_template, 0, _data);
            Label price = card.Root.Q<Label>(HudElementNames.CardPrice);

            card.Apply(false, 49);
            Assert.IsTrue(card.Root.ClassListContains(BuildCardView.DimmedClass));
            Assert.IsTrue(price.ClassListContains(BuildCardView.PriceShortClass));

            card.Apply(false, 50);
            Assert.IsFalse(card.Root.ClassListContains(BuildCardView.DimmedClass), "Fiyat kadar para yeter.");
            Assert.IsFalse(price.ClassListContains(BuildCardView.PriceShortClass));
        }

        private static BuildableData CreateData(Material material, Sprite icon)
        {
            var data = ScriptableObject.CreateInstance<BuildableData>();
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = "barricade";
            serialized.FindProperty("_displayName").stringValue = "Barikat";
            serialized.FindProperty("_icon").objectReferenceValue = icon;
            SerializedProperty levels = serialized.FindProperty("_levels");
            levels.arraySize = 2;
            levels.GetArrayElementAtIndex(0).boxedValue = new BuildableLevel("Ahşap", 50, 300f, material);
            levels.GetArrayElementAtIndex(1).boxedValue = new BuildableLevel("Metal", 120, 800f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }
    }
}
