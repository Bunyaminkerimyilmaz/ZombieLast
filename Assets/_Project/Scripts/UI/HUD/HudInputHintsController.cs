using LastHaven.Core;
using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'daki tuş ipuçlarının cihazı: son kullanılan cihaz gamepad ise ekranın köküne <see cref="UIClassNames.Gamepad"/> sınıfını
    /// ekler; USS (<c>Theme.uss</c>) <c>.key--keyboard</c> / <c>.key--gamepad</c> elementlerinden uyanı gösterir (inşa çubuğu, yapı
    /// ipucu, etkileşim ipucu). <see cref="IInputReader.IsUsingGamepad"/> her kare okunur, sınıf sadece değişince yazılır.
    /// HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudInputHintsController : UIDocumentController
    {
        private IInputReader _input;
        private bool _applied;
        private bool _gamepad;

        protected override void BindElements()
        {
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // UIDocument her açılışta ağacı yeniden kurar: sınıf bir sonraki karede yeniden yazılsın.
            _applied = false;
        }

        private void Start()
        {
            if (IsBound && !ServiceLocator.TryGet(out _input))
            {
                Debug.LogWarning("HudInputHintsController: IInputReader bulunamadı; tuş ipuçları hep klavyeyi gösterecek.", this);
            }
        }

        private void Update()
        {
            if (!IsBound || _input == null)
            {
                return;
            }

            bool gamepad = _input.IsUsingGamepad;
            if (!_applied || gamepad != _gamepad)
            {
                _applied = true;
                _gamepad = gamepad;
                ScreenElement.EnableInClassList(UIClassNames.Gamepad, gamepad);
            }
        }
    }
}
