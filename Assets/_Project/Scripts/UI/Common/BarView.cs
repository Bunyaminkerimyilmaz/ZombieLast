using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Dolum barı + "mevcut/maks" etiketi (<c>.bar</c> / <c>.bar-fill</c>). Dolum genişliği yüzde olarak yazılır; metin
    /// <see cref="FractionText"/> önbelleğinden gelir ve sadece değişince yazılır (her vuruşta yeni string oluşmaz).
    /// </summary>
    public sealed class BarView
    {
        private readonly VisualElement _fill;
        private readonly Label _value;

        public BarView(VisualElement fill, Label value)
        {
            _fill = fill;
            _value = value;
        }

        public void Set(float current, float max)
        {
            _fill.style.width = Length.Percent(HudFormat.Ratio(current, max) * 100f);
            string text = FractionText.Get(HudFormat.DisplayHealth(current), HudFormat.DisplayHealth(max));
            if (!ReferenceEquals(_value.text, text))
            {
                _value.text = text;
            }
        }
    }
}
