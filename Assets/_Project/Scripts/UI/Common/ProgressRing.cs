using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Bir elementin kenarlığının üstüne dolan yay çizer (basılı tutma ilerleme halkası). Element USS'te yuvarlak olmalı
    /// (<c>border-radius</c> = yarı boy); kenarlık halkanın izidir. Yay tepeden saat yönünde dolar; rengi elementin USS
    /// <c>color</c>'ı, kalınlığı üst kenarlık kalınlığı. Stil kodda yok: renk ve kalınlık tema değişkenlerinden gelir.
    /// </summary>
    public sealed class ProgressRing
    {
        /// <summary>Yayın başladığı açı: tepe (Painter2D'de 0° sağ, açı saat yönünde artar).</summary>
        private const float StartDegrees = -90f;

        private const float FullTurnDegrees = 360f;

        private readonly VisualElement _element;

        public ProgressRing(VisualElement element)
        {
            _element = element;
            _element.generateVisualContent += Draw;
        }

        /// <summary>Doluluk (0..1).</summary>
        public float Progress { get; private set; }

        /// <summary>Doluluğu ayarlar (0..1'e kırpılır); değiştiyse yeniden çizilir.</summary>
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (Mathf.Approximately(progress, Progress))
            {
                return;
            }

            Progress = progress;
            _element.MarkDirtyRepaint();
        }

        /// <summary>Çizimi elementten ayırır (UIDocument ağacı yeniden kurulmadan önce).</summary>
        public void Detach()
        {
            _element.generateVisualContent -= Draw;
        }

        private void Draw(MeshGenerationContext context)
        {
            if (Progress <= 0f)
            {
                return;
            }

            IResolvedStyle style = _element.resolvedStyle;
            float thickness = style.borderTopWidth;
            Rect rect = _element.layout;
            float radius = (Mathf.Min(rect.width, rect.height) - thickness) * 0.5f;
            if (thickness <= 0f || radius <= 0f)
            {
                return;
            }

            Painter2D painter = context.painter2D;
            painter.lineWidth = thickness;
            painter.lineCap = LineCap.Butt;
            painter.strokeColor = style.color;
            painter.BeginPath();
            painter.Arc(new Vector2(rect.width * 0.5f, rect.height * 0.5f), radius,
                Angle.Degrees(StartDegrees), Angle.Degrees(StartDegrees + FullTurnDegrees * Progress));
            painter.Stroke();
        }
    }
}
