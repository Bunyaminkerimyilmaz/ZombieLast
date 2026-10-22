using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// UI fontunun gliflerini oyun başında atlasa ekler (15.2). Tema fontu dinamiktir: bir karakter ilk kez ölçülünce glifi atlasa
    /// eklenir ve kerning tablosu yeniden kurulur; Profiler'da tek karede ~0.5 MB GC ve takılma (ör. ilk hasar sayısı, ilk "GECE"
    /// başlığı). Font asset'ine genel API ile ulaşılamadığı için (tema eski tip Font veriyor, UI Toolkit içeride dönüştürüyor) görünmez
    /// iki etiket (normal ve kalın) tüm karakterlerle bir kez ölçülür, ilk düzenden sonra kendini kaldırır.
    /// </summary>
    public static class UiGlyphWarmup
    {
        /// <summary>Türkçe alfabe, İngilizce ekleri, rakamlar, noktalama ve HUD sembolleri.</summary>
        public const string Characters =
            "ABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZabcçdefgğhıijklmnoöpqrsştuüvwxyz0123456789 .,:;!?%+-/()[]'\"×∞—–·→…";

        /// <summary>Görünmez ısıtma etiketlerini <paramref name="parent"/>'a ekler; ilk düzende ölçülüp kaldırılırlar.</summary>
        public static void Attach(VisualElement parent)
        {
            if (parent == null)
            {
                return;
            }

            parent.Add(CreateLabel(UnityEngine.FontStyle.Normal));
            parent.Add(CreateLabel(UnityEngine.FontStyle.Bold));
        }

        private static Label CreateLabel(UnityEngine.FontStyle style)
        {
            var label = new Label(Characters) { pickingMode = PickingMode.Ignore };
            label.style.position = Position.Absolute;
            label.style.opacity = 0f;
            label.style.unityFontStyleAndWeight = style;
            label.RegisterCallback<GeometryChangedEvent>(OnMeasured);
            return label;
        }

        private static void OnMeasured(GeometryChangedEvent measured)
        {
            var label = (VisualElement)measured.target;
            label.UnregisterCallback<GeometryChangedEvent>(OnMeasured);
            label.RemoveFromHierarchy();
        }
    }
}
