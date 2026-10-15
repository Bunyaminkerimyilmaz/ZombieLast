using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Oyuncunun etkileşimi (GDD §4: E / B-East basılı tut). Her kare <see cref="IInteractableRegistry"/>'den menzildeki en yakın
    /// etkileşilebilir nesneyi seçer; tutma mantığı <see cref="InteractionHold"/>'da. Sadece Gameplay girdi modunda ve oyuncu
    /// canlıyken çalışır (inşa modunda E yok). İpucu değişince <see cref="InteractionPromptEvent"/> gönderir.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class Interactor : MonoBehaviour
    {
        private const string LogPrefix = "[Player] ";

        private readonly InteractionHold _hold = new InteractionHold();
        private Health _health;
        private IInputReader _input;
        private IInteractableRegistry _registry;
        private string _lastPrompt;
        private bool _lastHolding;
        private float _lastProgress;

        /// <summary>Seçili etkileşim hedefi; yoksa null.</summary>
        public IInteractable Target => _hold.Target;

        public bool IsHolding => _hold.IsHolding;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _input))
            {
                Debug.LogWarning("Interactor: IInputReader bulunamadı; etkileşim çalışmayacak. Sahnede [Systems]/Input var mı?", this);
            }

            if (!ServiceLocator.TryGet(out _registry))
            {
                Debug.LogWarning("Interactor: IInteractableRegistry bulunamadı; etkileşim çalışmayacak. Sahnede [Systems]/Interaction var mı?", this);
            }
        }

        private void OnDisable()
        {
            if (_hold.SetTarget(null))
            {
                Publish(targetChanged: true);
            }
        }

        private void Update()
        {
            if (_input == null || _registry == null)
            {
                return;
            }

            bool canAct = _health.IsAlive && _input.Mode == InputMode.Gameplay;
            IInteractable nearest = canAct ? _registry.FindNearest(transform.position) : null;
            bool targetChanged = _hold.SetTarget(nearest);
            _hold.Tick(canAct && _input.IsInteractHeld, Time.deltaTime);
            Publish(targetChanged);
        }

        // İpucu sadece değişince (hedef, metin, tutma, ilerleme) duyurulur; boşta her kare olay yok.
        private void Publish(bool targetChanged)
        {
            IInteractable target = _hold.Target;
            string prompt = target != null ? target.Prompt : null;
            bool holding = _hold.IsHolding;
            float progress = _hold.Progress;
            if (!targetChanged && ReferenceEquals(prompt, _lastPrompt) && holding == _lastHolding && progress == _lastProgress)
            {
                return;
            }

            _lastPrompt = prompt;
            _lastHolding = holding;
            _lastProgress = progress;
            Vector3 position = target != null ? target.PromptPosition : Vector3.zero;
            EventBus<InteractionPromptEvent>.Raise(new InteractionPromptEvent(target, prompt, position, holding, progress));

            // UI ipucu (Gün 10) gelene kadar hedef log'da görünür.
            if (targetChanged && target != null)
            {
                Debug.Log($"{LogPrefix}Etkileşim: {prompt} (E basılı tut).", this);
            }
        }
    }
}
