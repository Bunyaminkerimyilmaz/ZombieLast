using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Bir kenar göstergesi (<c>HUD.uxml</c>, <see cref="IndicatorClass"/>): ortada hedefin simgesi, etrafında hedefe dönen ok.
    /// Kök <c>position: absolute</c>; merkezi verilen noktaya konur, ok taşıyıcısı (<see cref="PivotClass"/>, kökü kaplar)
    /// açıyla döner, ok ucu (<see cref="ArrowClass"/>, <see cref="ArrowGlyph"/>) taşıyıcının sağ kenarında. Ekran kenarına uzaklık
    /// kökün USS <c>margin-bottom</c>'ı (<see cref="EdgeGap"/>; margin-left/top 0 kalmalı). Stil sadece değişince yazılır.
    /// </summary>
    public sealed class EdgeIndicatorView
    {
        public const string IndicatorClass = "hud-edge-indicator";
        public const string PivotClass = "hud-edge-indicator__pivot";
        public const string ArrowClass = "hud-edge-indicator__arrow";

        private readonly VisualElement _root;
        private readonly VisualElement _pivot;
        private readonly ArrowGlyph _arrow;
        private Vector2 _topLeft = new Vector2(float.NaN, float.NaN);
        private float _angle = float.NaN;
        private bool _shown = true;

        public EdgeIndicatorView(VisualElement root)
        {
            _root = root;
            _pivot = root.Q(className: PivotClass);
            VisualElement arrow = root.Q(className: ArrowClass);
            if (arrow != null)
            {
                _arrow = new ArrowGlyph(arrow);
            }
        }

        /// <summary>Taşıyıcı ve ok bulundu.</summary>
        public bool IsValid => _pivot != null && _arrow != null;

        /// <summary>Göstergenin boyutu (ilk karede ölçülmemişse 0).</summary>
        public Vector2 Size
        {
            get
            {
                Vector2 size = _root.layout.size;
                return new Vector2(Sanitize(size.x), Sanitize(size.y));
            }
        }

        /// <summary>Göstergenin merkezinin ekran kenarına en az boşluğu (USS <c>margin-bottom</c>).</summary>
        public float EdgeGap => Sanitize(_root.resolvedStyle.marginBottom);

        /// <summary>
        /// Göstergeyi ekran (panel boyutu <paramref name="screen"/>) kenarında, ekran dışındaki hedefin viewport noktası
        /// <paramref name="viewport"/> yönünde gösterir (<see cref="EdgeIndicatorMath.ToEdge"/>; kenar boşluğu <see cref="EdgeGap"/>).
        /// </summary>
        public void ShowToward(Vector3 viewport, Vector2 screen)
        {
            Vector2 inset = Size * 0.5f + Vector2.one * EdgeGap;
            Vector2 center = EdgeIndicatorMath.ToEdge(viewport, screen, inset, out float angle);
            Show(center, angle);
        }

        /// <summary>Göstergeyi merkezi <paramref name="center"/>'da, oku <paramref name="angleDegrees"/> yönünde gösterir.</summary>
        public void Show(Vector2 center, float angleDegrees)
        {
            Vector2 topLeft = center - Size * 0.5f;
            if (topLeft != _topLeft)
            {
                _topLeft = topLeft;
                _root.style.left = topLeft.x;
                _root.style.top = topLeft.y;
            }

            if (!Mathf.Approximately(angleDegrees, _angle))
            {
                _angle = angleDegrees;
                _pivot.style.rotate = new Rotate(Angle.Degrees(angleDegrees));
            }

            SetShown(true);
        }

        public void Hide()
        {
            SetShown(false);
        }

        /// <summary>Ok çizimini ayırır (UIDocument ağacı yeniden kurulmadan önce).</summary>
        public void Detach()
        {
            _arrow?.Detach();
        }

        private void SetShown(bool shown)
        {
            if (_shown != shown)
            {
                _shown = shown;
                _root.SetShown(shown);
            }
        }

        private static float Sanitize(float value)
        {
            return float.IsNaN(value) || value < 0f ? 0f : value;
        }
    }
}
