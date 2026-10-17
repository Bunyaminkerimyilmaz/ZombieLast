using LastHaven.Core;
using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// Etkileşim halkasının doluluğu (saf). Basılı tutma turunun ilerlemesini gösterir; tur tekrarlanırsa (onarım 0.1 sn'de bir tur
    /// atar) halka her turda boşalıp titremesin diye ilk tur bitince tuş bırakılana kadar dolu kalır. Tutma bitince veya hedef
    /// değişince sıfırlanır. Tek turluk etkileşim (acil yakıt, 1 sn) baştan sona dolar.
    /// </summary>
    public sealed class HoldRingProgress
    {
        private IInteractable _target;
        private float _lastProgress;
        private bool _repeating;

        /// <summary>Halkanın doluluğu (0..1).</summary>
        public float Value { get; private set; }

        /// <summary><see cref="InteractionPromptEvent"/>'in değerleriyle günceller ve yeni doluluğu döndürür.</summary>
        public float Update(IInteractable target, bool isHolding, float holdProgress)
        {
            if (!isHolding || !ReferenceEquals(target, _target))
            {
                _repeating = false;
                _lastProgress = 0f;
            }

            _target = target;
            if (!isHolding)
            {
                Value = 0f;
                return Value;
            }

            float progress = Mathf.Clamp01(holdProgress);
            if (progress < _lastProgress)
            {
                // Tur bitti, yenisi başladı.
                _repeating = true;
            }

            _lastProgress = progress;
            Value = _repeating ? 1f : progress;
            return Value;
        }
    }
}
