using LastHaven.Core;

namespace LastHaven.Player
{
    /// <summary>
    /// Etkileşim tuşunu basılı tutma (saf C#; <see cref="Interactor"/> her kare sürer). Hedef basılıyken
    /// <see cref="IInteractable.HoldDuration"/> kadar tutulunca <see cref="IInteractable.OnInteract"/> çağrılır; tuş basılı kalırsa
    /// fazla süre devreder ve etkileşim tekrarlanır (onarım: 0.1 sn'de bir tur). <see cref="IInteractable.OnInteract"/> false
    /// dönerse tutma durur ve tuş bırakılana kadar başlamaz. Hedef değişince veya tuş bırakılınca ilerleme sıfırlanır.
    /// </summary>
    public sealed class InteractionHold
    {
        // Çok uzun bir karede (takılma) bile sınırsız etkileşim olmasın; kalan süre atılır.
        private const int MaxInteractionsPerTick = 8;

        /// <summary>Seçili hedef; yoksa null.</summary>
        public IInteractable Target { get; private set; }

        /// <summary>Tuş basılı ve tutma sürüyor (durmadı).</summary>
        public bool IsHolding { get; private set; }

        /// <summary><see cref="IInteractable.OnInteract"/> false döndü; tuş bırakılana kadar yeni tutma başlamaz.</summary>
        public bool IsStopped { get; private set; }

        /// <summary>Bu turda basılı tutulan süre (sn).</summary>
        public float Elapsed { get; private set; }

        /// <summary>Bu turun ilerlemesi (0..1); tutma yokken 0.</summary>
        public float Progress
        {
            get
            {
                if (!IsHolding || Target == null)
                {
                    return 0f;
                }

                float duration = Target.HoldDuration;
                return duration > 0f ? UnityEngine.Mathf.Clamp01(Elapsed / duration) : 0f;
            }
        }

        /// <summary>Hedefi değiştirir; değiştiyse süren tutma kesilir (eski hedefe ilerleme 0) ve true döner.</summary>
        public bool SetTarget(IInteractable target)
        {
            if (ReferenceEquals(target, Target))
            {
                return false;
            }

            Cancel();
            Target = target;
            return true;
        }

        /// <summary>Bir kare ilerletir. <paramref name="held"/> false ise tutma kesilir ve durma kaldırılır.</summary>
        public void Tick(bool held, float deltaTime)
        {
            if (!held)
            {
                Cancel();
                IsStopped = false;
                return;
            }

            if (Target == null || IsStopped)
            {
                return;
            }

            if (InteractableSet.IsDestroyed(Target) || !Target.CanInteract)
            {
                Cancel();
                return;
            }

            IsHolding = true;
            float duration = Target.HoldDuration;
            if (duration <= 0f)
            {
                Interact();
                return;
            }

            Elapsed += deltaTime;
            int interactions = 0;
            while (IsHolding && Elapsed >= duration)
            {
                Elapsed -= duration;
                if (!Interact())
                {
                    return;
                }

                // İş bitti (ör. tam onarıldı, sandık açıldı) veya kare sınırı: kalan süre devretmez.
                if (!Target.CanInteract || ++interactions >= MaxInteractionsPerTick)
                {
                    Elapsed = 0f;
                    break;
                }
            }

            if (IsHolding)
            {
                Target.OnHoldProgress(Progress);
            }
        }

        // Tur bitti: hedefe 1 ilerleme + etkileşim. False dönerse tutma durur.
        private bool Interact()
        {
            Target.OnHoldProgress(1f);
            if (Target.OnInteract())
            {
                return true;
            }

            Cancel();
            IsStopped = true;
            return false;
        }

        private void Cancel()
        {
            if (IsHolding && !InteractableSet.IsDestroyed(Target))
            {
                Target.OnHoldProgress(0f);
            }

            IsHolding = false;
            Elapsed = 0f;
        }
    }
}
