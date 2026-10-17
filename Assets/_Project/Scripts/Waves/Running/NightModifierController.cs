using LastHaven.Core;
using LastHaven.Enemies;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// Gecenin olayı (GDD §14): <see cref="WaveSpawner"/> gece başında <see cref="Begin"/> (→ <see cref="NightModifierActivatedEvent"/>),
    /// gece bitince <see cref="End"/> (→ <see cref="NightModifierEndedEvent"/>) çağırır. Düşman adedi gecenin planında
    /// (<see cref="WavePlan.ScaleCount"/>), hız çarpanı doğan/katılan her düşmanda (<see cref="ApplyTo"/>,
    /// <see cref="SpeedSource.NightEvent"/>) uygulanır. Saf C#; Unity'ye sadece düşmanın bileşenine dokunur.
    /// </summary>
    public sealed class NightModifierController
    {
        private const string LogPrefix = "[Waves] ";

        private NightModifierSettings _settings;
        private int _nightIndex = -1;

        /// <summary>Bu gece bir olay var.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Etkin olayın ayarları; olay yokken varsayılan.</summary>
        public NightModifierSettings Settings => _settings;

        /// <summary>Etkin olayın düşman hız çarpanı; olay yoksa 1.</summary>
        public float EnemySpeedMultiplier => IsActive ? _settings.EnemySpeedMultiplier : 1f;

        /// <summary>
        /// Önceki olayı bitirir; <paramref name="modifier"/> doluysa yenisini etkinleştirip duyurur. Null ise olay yok (olay gönderilmez).
        /// </summary>
        /// <param name="context">Log bağlamı (null olabilir).</param>
        public void Begin(int nightIndex, NightModifierData modifier, Object context = null)
        {
            End();
            if (modifier == null)
            {
                return;
            }

            _settings = modifier.ToSettings();
            _nightIndex = nightIndex;
            IsActive = true;
            Debug.Log($"{LogPrefix}NightModifierActivatedEvent: {_settings.DisplayName} (Gece {nightIndex + 1}).", context);
            EventBus<NightModifierActivatedEvent>.Raise(new NightModifierActivatedEvent(nightIndex, _settings));
        }

        /// <summary>Etkin olay varsa bitirip duyurur; yoksa bir şey yapmaz.</summary>
        public void End()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            EventBus<NightModifierEndedEvent>.Raise(new NightModifierEndedEvent(_nightIndex, _settings));
        }

        /// <summary>Olayın hız çarpanını düşmana yazar (1 ise veya düşman değilse dokunmaz). Doğuş başına bir kez çağrılır.</summary>
        public void ApplyTo(GameObject enemy)
        {
            float multiplier = EnemySpeedMultiplier;
            if (enemy == null || Mathf.Approximately(multiplier, 1f) || !enemy.TryGetComponent(out Enemy target))
            {
                return;
            }

            target.SetSpeedMultiplier(SpeedSource.NightEvent, multiplier);
        }
    }
}
