using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Bir elementin içini sağa bakan dolu üçgenle (ok ucu) çizer: uç sağ kenarın ortasında, taban sol kenar boyunca. Rengi
    /// elementin USS <c>color</c>'ı, boyutu elementin boyutu; yönü elementin (veya ebeveyninin) <c>rotate</c>'iyle verilir.
    /// Stil kodda yok.
    /// </summary>
    public sealed class ArrowGlyph
    {
        private readonly VisualElement _element;

        public ArrowGlyph(VisualElement element)
        {
            _element = element;
            _element.generateVisualContent += Draw;
        }

        /// <summary>Çizimi elementten ayırır (UIDocument ağacı yeniden kurulmadan önce).</summary>
        public void Detach()
        {
            _element.generateVisualContent -= Draw;
        }

        private void Draw(MeshGenerationContext context)
        {
            Rect rect = _element.contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            Painter2D painter = context.painter2D;
            painter.fillColor = _element.resolvedStyle.color;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.center.y));
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.ClosePath();
            painter.Fill();
        }
    }
}
