using LastHaven.Core;
using UnityEngine;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Silah olaylarını konsola yazar (HUD gelene kadar): silah değişimi ve doldurma her zaman; mermi ve isabetler isteğe bağlı
    /// (her atışta yazdıkları için varsayılan kapalı). Sahneye <c>TestDummiesSceneStep</c> kurar.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatDebugLogger : MonoBehaviour
    {
        [SerializeField, Tooltip("Her mermi değişimini yaz (her atışta).")]
        private bool _logAmmo;

        [SerializeField, Tooltip("Her isabeti yaz (pompalıda atış başına 8 satıra kadar).")]
        private bool _logHits;

        private void OnEnable()
        {
            EventBus<WeaponChangedEvent>.Subscribe(OnWeaponChanged);
            EventBus<AmmoChangedEvent>.Subscribe(OnAmmoChanged);
            EventBus<ReloadStartedEvent>.Subscribe(OnReloadStarted);
            EventBus<ReloadFinishedEvent>.Subscribe(OnReloadFinished);
            EventBus<ShotHitEvent>.Subscribe(OnShotHit);
        }

        private void OnDisable()
        {
            EventBus<WeaponChangedEvent>.Unsubscribe(OnWeaponChanged);
            EventBus<AmmoChangedEvent>.Unsubscribe(OnAmmoChanged);
            EventBus<ReloadStartedEvent>.Unsubscribe(OnReloadStarted);
            EventBus<ReloadFinishedEvent>.Unsubscribe(OnReloadFinished);
            EventBus<ShotHitEvent>.Unsubscribe(OnShotHit);
        }

        private void OnWeaponChanged(WeaponChangedEvent changed)
        {
            Debug.Log($"[Combat] Silah: {changed.DisplayName} ({changed.SlotIndex + 1}. yuva)", this);
        }

        private void OnAmmoChanged(AmmoChangedEvent ammo)
        {
            if (!_logAmmo)
            {
                return;
            }

            string reserve = ammo.HasInfiniteReserve ? "∞" : ammo.Reserve.ToString();
            Debug.Log($"[Combat] Mermi ({ammo.WeaponId}): {ammo.Ammo}/{ammo.MagSize} · yedek {reserve}", this);
        }

        private void OnReloadStarted(ReloadStartedEvent reload)
        {
            Debug.Log($"[Combat] Doldurma başladı ({reload.WeaponId}, {reload.Duration:0.##} sn)", this);
        }

        private void OnReloadFinished(ReloadFinishedEvent reload)
        {
            Debug.Log(reload.Completed
                ? $"[Combat] Doldurma bitti ({reload.WeaponId})"
                : $"[Combat] Doldurma iptal ({reload.WeaponId})", this);
        }

        private void OnShotHit(ShotHitEvent hit)
        {
            if (_logHits)
            {
                Debug.Log($"[Combat] İsabet: {(hit.Target != null ? hit.Target.name : "?")} ({hit.TargetType}) hasar {hit.Damage:0.#}", this);
            }
        }
    }
}
