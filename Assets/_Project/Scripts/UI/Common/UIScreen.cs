namespace LastHaven.UI
{
    /// <summary>
    /// Bir <see cref="UnityEngine.UIElements.UIDocument"/>'ı süren ekranların tabanı (HUD, paneller): elementleri
    /// <see cref="UIDocumentController"/> bağlar, bu sınıf ekranı gösterir/gizler. Görünürlük UXML kök elementindeki
    /// (<see cref="UIClassNames.Screen"/>) <see cref="UIClassNames.Hidden"/> sınıfıyla değişir; paneller UXML'de gizli başlar,
    /// böylece editörde (Play dışında) Game view'ı kapatmazlar. Bir UIDocument'ta tek UIScreen olmalı.
    /// </summary>
    public abstract class UIScreen : UIDocumentController
    {
        private bool _shown = true;

        /// <summary>Ekran görünür mü (bileşen kapanıp açılsa da korunur).</summary>
        public bool IsShown => _shown;

        protected override void OnEnable()
        {
            base.OnEnable();
            ApplyShown();
        }

        /// <summary>Tüm ekranı gösterir/gizler.</summary>
        protected void SetShown(bool shown)
        {
            _shown = shown;
            ApplyShown();
        }

        private void ApplyShown()
        {
            ScreenElement?.SetShown(_shown);
        }
    }
}
