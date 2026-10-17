using System.Globalization;
using LastHaven.Building;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// İnşa çubuğunda bir yapı kartı (<c>UI/HUD/BuildCard.uxml</c> örneği): yuva numarası (kısayol), ad, inşa fiyatı, ikon; ikon
    /// yoksa yapının rengi (1. seviye materyalinin ana rengi; o da yoksa USS'teki varsayılan renk). Seçiliyken
    /// <see cref="SelectedClass"/>, para yetmezken <see cref="DimmedClass"/> + kırmızı fiyat. Sınıflar sadece değişince yazılır.
    /// </summary>
    public sealed class BuildCardView
    {
        public const string CardClass = "build-card";
        public const string SelectedClass = "build-card--selected";
        public const string DimmedClass = "build-card--dimmed";
        public const string PriceShortClass = "build-card__price--short";

        // URP/Lit ve çoğu shader'ın ana rengi; eski shader'lar için _Color.
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly Label _price;
        private bool? _selected;
        private bool? _affordable;

        private BuildCardView(int slotIndex, BuildableData data, VisualElement root, Label price)
        {
            SlotIndex = slotIndex;
            Data = data;
            Root = root;
            _price = price;
        }

        /// <summary>0 tabanlı yuva (kısayol numarası = yuva + 1).</summary>
        public int SlotIndex { get; }

        public BuildableData Data { get; }

        /// <summary>Kartın kök elementi (şablon örneği; sarmalayıcıdan ayrılmış).</summary>
        public VisualElement Root { get; }

        /// <summary>
        /// Şablondan kart kurar ve metinleri, ikonu/rengi yazar. Şablonda kart sınıflı element veya beklenen elementler yoksa
        /// hata loglar ve null döner.
        /// </summary>
        public static BuildCardView Create(VisualTreeAsset template, int slotIndex, BuildableData data)
        {
            TemplateContainer container = template.Instantiate();
            VisualElement root = container.Q(className: CardClass);
            Label key = root?.Q<Label>(HudElementNames.CardKey);
            Label price = root?.Q<Label>(HudElementNames.CardPrice);
            Label name = root?.Q<Label>(HudElementNames.CardName);
            VisualElement swatch = root?.Q(HudElementNames.CardSwatch);
            if (key == null || price == null || name == null || swatch == null)
            {
                Debug.LogError($"BuildCardView: kart şablonunda '{CardClass}' sınıflı kök veya kart elementleri yok ({template.name}).");
                return null;
            }

            root.RemoveFromHierarchy();
            key.text = (slotIndex + 1).ToString(CultureInfo.InvariantCulture);
            price.text = HudFormat.Count(data.BuildCost);
            name.text = data.DisplayName;
            if (data.Icon != null)
            {
                swatch.style.backgroundImage = new StyleBackground(data.Icon);
            }
            else if (TryGetColor(data, out Color color))
            {
                swatch.style.backgroundColor = color;
            }

            return new BuildCardView(slotIndex, data, root, price);
        }

        /// <summary>Seçili mi ve para yetiyor mu (inşa fiyatı ≤ para).</summary>
        public void Apply(bool selected, int money)
        {
            bool affordable = Data.BuildCost <= money;
            if (_selected != selected)
            {
                _selected = selected;
                Root.EnableInClassList(SelectedClass, selected);
            }

            if (_affordable != affordable)
            {
                _affordable = affordable;
                Root.EnableInClassList(DimmedClass, !affordable);
                _price.EnableInClassList(PriceShortClass, !affordable);
            }
        }

        /// <summary>Yapının kart rengi: 1. seviye materyalinin ana rengi (opak). Materyal veya renk yoksa false.</summary>
        public static bool TryGetColor(BuildableData data, out Color color)
        {
            Material material = data != null ? data.GetLevel(0)?.Material : null;
            if (material == null || !(material.HasProperty(BaseColorId) || material.HasProperty(ColorId)))
            {
                color = default;
                return false;
            }

            color = material.GetColor(material.HasProperty(BaseColorId) ? BaseColorId : ColorId);
            color.a = 1f;
            return true;
        }
    }
}
