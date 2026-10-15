using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Oyuncunun etkileşim ipucu değiştiğinde gönderilir (oyuncunun <c>Interactor</c>'ı): hedef değişince (yoksa
    /// <see cref="HasTarget"/> false), ipucu metni değişince ve basılı tutarken her kare (ilerleme). Boşta her kare gönderilmez.
    /// Etkileşim ipucu widget'ı (UI, Gün 10: "E — Onar (12)" + ilerleme halkası) dinler.
    /// </summary>
    public readonly struct InteractionPromptEvent : IEvent
    {
        public InteractionPromptEvent(IInteractable interactable, string prompt, Vector3 position, bool isHolding, float holdProgress)
        {
            Interactable = interactable;
            Prompt = prompt ?? string.Empty;
            Position = position;
            IsHolding = isHolding;
            HoldProgress = holdProgress;
        }

        /// <summary>İpucu verilen nesne; ipucu yoksa null.</summary>
        public IInteractable Interactable { get; }

        /// <summary>İpucu metni (tuş adı hariç), ör. "Onar (12)"; hedef yoksa boş.</summary>
        public string Prompt { get; }

        /// <summary>İpucunun dünya konumu (<see cref="IInteractable.PromptPosition"/>).</summary>
        public Vector3 Position { get; }

        /// <summary>Tuş basılı ve basılı tutma sürüyor (para bitip durduysa false).</summary>
        public bool IsHolding { get; }

        /// <summary>Basılı tutma turunun ilerlemesi (0..1); basılı değilken 0.</summary>
        public float HoldProgress { get; }

        public bool HasTarget => Interactable != null;
    }
}
