using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// [Systems]/BuildingPerks: Dikenli Duvar (GDD §13). <see cref="PerkFlag.ThornWalls"/> açıkken bir barikat yakın dövüş hasarı
    /// alınca, gerçekte aldığı hasarın <see cref="BuildingPerkConfig.ThornsReflectFraction"/> kadarı saldırana döner (kaynak barikat,
    /// <see cref="DamageType.Trap"/>: yapıdan gelen hasar; oyuncunun öldürmesi sayılmaz). Tüm hasarları
    /// (<see cref="DamagedEvent"/>) dinler; bayrak kapalıyken ilk kontrolde çıkar.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BarricadeThorns : MonoBehaviour
    {
        [SerializeField, Tooltip("Yansıtma oranı. Üretici bağlar.")]
        private BuildingPerkConfig _config;

        private IRunModifiers _modifiers;

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("BarricadeThorns: BuildingPerkConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _modifiers))
            {
                Debug.LogWarning("BarricadeThorns: IRunModifiers bulunamadı; Dikenli Duvar çalışmayacak.", this);
            }
        }

        private void OnEnable()
        {
            EventBus<DamagedEvent>.Subscribe(OnDamaged);
        }

        private void OnDisable()
        {
            EventBus<DamagedEvent>.Unsubscribe(OnDamaged);
        }

        private void OnDamaged(DamagedEvent damaged)
        {
            if (_modifiers == null || !_modifiers.HasFlag(PerkFlag.ThornWalls))
            {
                return;
            }

            DamageInfo hit = damaged.Damage;
            GameObject barricade = damaged.Target;
            if (!ThornsRule.Reflects(hit.DamageType) || hit.Source == null || barricade == null || barricade.layer != GameLayers.Structure
                || !barricade.TryGetComponent(out Structure structure) || structure.TargetType != TargetType.Barricade)
            {
                return;
            }

            float amount = ThornsRule.GetReflectedDamage(damaged.AppliedAmount, _config.ThornsReflectFraction);
            if (amount <= 0f || !hit.Source.TryGetComponent(out IDamageable attacker) || !attacker.IsAlive)
            {
                return;
            }

            Vector3 attackerPosition = hit.Source.transform.position;
            Vector3 direction = attackerPosition - barricade.transform.position;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;
            attacker.TakeDamage(new DamageInfo(amount, DamageType.Trap, barricade, attackerPosition, direction));
        }
    }
}
