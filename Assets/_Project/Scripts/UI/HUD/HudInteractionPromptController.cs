using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un etkileşim ipucu (GDD §16 "E — Aç"): oyuncunun etkileşebileceği nesnenin (onarım, acil yakıt...) üstünde tuş + ipucu
    /// metni ("E — Onar (12)"); tuş, basılı tutma ilerleme halkasının (<see cref="ProgressRing"/>) ortasında. Klavye/gamepad tuşu
    /// UXML'de ikisi de yazılı; hangisinin görüneceğini <see cref="HudInputHintsController"/> seçer. Kaynak
    /// <see cref="InteractionPromptEvent"/> (hedef/metin/tutma değişince ve basılı tutarken her kare); kutu her kare kameradan
    /// sonra nesnenin üstüne taşınır (<see cref="WorldAnchor"/>). Halka doluluğu <see cref="HoldRingProgress"/>'te. HUD'un
    /// UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(HudExecutionOrder.AfterCamera)]
    public sealed class HudInteractionPromptController : UIDocumentController
    {
        private readonly HoldRingProgress _holdRing = new HoldRingProgress();
        private VisualElement _prompt;
        private VisualElement _ringElement;
        private Label _text;
        private ProgressRing _ring;
        private WorldAnchor _anchor;
        private ICameraService _camera;
        private bool _hasTarget;
        private Vector3 _position;

        protected override void BindElements()
        {
            _prompt = Require<VisualElement>(HudElementNames.InteractionPrompt);
            _ringElement = Require<VisualElement>(HudElementNames.InteractionRing);
            _text = Require<Label>(HudElementNames.InteractionText);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _anchor = new WorldAnchor(_prompt);
            _ring = new ProgressRing(_ringElement);
            Hide();
            EventBus<InteractionPromptEvent>.Subscribe(OnPrompt);
        }

        private void OnDisable()
        {
            EventBus<InteractionPromptEvent>.Unsubscribe(OnPrompt);
            _ring?.Detach();
            _ring = null;
            _hasTarget = false;
        }

        private void Start()
        {
            if (IsBound && !ServiceLocator.TryGet(out _camera))
            {
                Debug.LogWarning("HudInteractionPromptController: ICameraService bulunamadı; etkileşim ipucu konumlanamayacak.", this);
            }
        }

        private void LateUpdate()
        {
            if (_hasTarget)
            {
                _anchor.Place(_camera?.Camera, _position);
            }
        }

        private void OnPrompt(InteractionPromptEvent prompt)
        {
            if (!prompt.HasTarget)
            {
                Hide();
                return;
            }

            // Interactor metni değişmedikçe aynı string örneğini gönderir: metin sadece değişince yazılır.
            if (!ReferenceEquals(_text.text, prompt.Prompt))
            {
                _text.text = prompt.Prompt;
            }

            _position = prompt.Position;
            _ring.SetProgress(_holdRing.Update(prompt.Interactable, prompt.IsHolding, prompt.HoldProgress));
            if (!_hasTarget)
            {
                _hasTarget = true;
                _prompt.SetShown(true);
                _anchor.Place(_camera?.Camera, _position);
            }
        }

        private void Hide()
        {
            _hasTarget = false;
            _holdRing.Update(null, false, 0f);
            _ring.SetProgress(0f);
            _prompt.SetShown(false);
        }
    }
}
