using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// Gece olayının jeton çarpanı (GDD §14 Kan Ayı: jeton değeri ×2): <see cref="NightModifierActivatedEvent"/> gelince
    /// <see cref="IRunModifiers"/>'a <see cref="StatType.CoinValue"/> için geçici yüzde ekler (çarpan − 1; kaynak
    /// <see cref="NightModifierSettings.SourceId"/>), <see cref="NightModifierEndedEvent"/> gelince kaldırır. Jetonun değeri düştüğü anda
    /// hesaplandığı için (<see cref="CoinDropper"/>) olay gecesinde düşenler ×2 kalır. [Systems]/Economy'de durur.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NightCoinModifier : MonoBehaviour
    {
        private const string LogPrefix = "[Economy] ";

        private IRunModifiers _modifiers;

        /// <summary>
        /// Olayın jeton çarpanını yazar (çarpan 1 ise bir şey yapmaz). Önce aynı kaynağın eskisi kaldırılır. Yazdıysa true. Saf; testler
        /// doğrudan çağırır.
        /// </summary>
        public static bool Apply(IRunModifiers modifiers, in NightModifierSettings modifier)
        {
            if (modifiers == null)
            {
                return false;
            }

            modifiers.RemoveBySource(modifier.SourceId);
            if (Mathf.Approximately(modifier.CoinMultiplier, 1f))
            {
                return false;
            }

            modifiers.AddModifier(StatModifier.Percent(StatType.CoinValue, modifier.CoinMultiplier - 1f, modifier.SourceId));
            return true;
        }

        /// <summary>Olayın geçici modifier'larını kaldırır; kaldırılan sayısını döndürür.</summary>
        public static int Remove(IRunModifiers modifiers, in NightModifierSettings modifier)
        {
            return modifiers != null ? modifiers.RemoveBySource(modifier.SourceId) : 0;
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _modifiers))
            {
                Debug.LogWarning("NightCoinModifier: IRunModifiers bulunamadı; gece olayının jeton çarpanı uygulanmayacak.", this);
            }
        }

        private void OnEnable()
        {
            EventBus<NightModifierActivatedEvent>.Subscribe(OnActivated);
            EventBus<NightModifierEndedEvent>.Subscribe(OnEnded);
        }

        private void OnDisable()
        {
            EventBus<NightModifierActivatedEvent>.Unsubscribe(OnActivated);
            EventBus<NightModifierEndedEvent>.Unsubscribe(OnEnded);
        }

        private void OnActivated(NightModifierActivatedEvent activated)
        {
            NightModifierSettings modifier = activated.Modifier;
            if (Apply(_modifiers, modifier))
            {
                Debug.Log(FormattableString.Invariant(
                    $"{LogPrefix}{modifier.DisplayName}: jeton değeri ×{modifier.CoinMultiplier:0.##} (bu gece)."), this);
            }
        }

        private void OnEnded(NightModifierEndedEvent ended)
        {
            Remove(_modifiers, ended.Modifier);
        }
    }
}
