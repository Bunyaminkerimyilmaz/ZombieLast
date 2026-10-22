using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD açılırken metin önbelleklerini ısıtır (15.2): sık değişen sayıların metinleri (<see cref="NumberText.PrefillCounts"/>) ve
    /// UI fontunun glifleri (<see cref="UiGlyphWarmup"/>; görünmez etiketler ilk düzende ölçülüp kalkar). Böylece oynanışta ilk hasar
    /// sayısı, ilk gece başlığı vb. string ayırmaz ve font atlasını büyütmez. HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudTextWarmupController : UIDocumentController
    {
        private bool _warmed;

        protected override void BindElements()
        {
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            NumberText.PrefillCounts();
            if (IsBound && !_warmed)
            {
                _warmed = true;
                UiGlyphWarmup.Attach(ScreenElement);
            }
        }
    }
}
