using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary>
    /// Oyun olaylarını görsel efektlere bağlar ([Systems]/Feedback): <see cref="WeaponFiredEvent"/> → namlu ışığı,
    /// <see cref="ShotTracedEvent"/> → mermi izi, <see cref="ShotHitEvent"/> → vuruş efekti, <see cref="StructureDestroyedEvent"/> →
    /// moloz. Efektler <see cref="IPoolService"/>'ten alınır ve oyun başında ön ısıtılır. Combat ve Building bu sınıfı bilmez.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FeedbackDirector : MonoBehaviour
    {
        [SerializeField, Tooltip("Efekt prefab'ları, havuz sayıları, iz renkleri.")]
        private CombatFeedbackConfig _config;

        private IPoolService _pool;
        private TracerPalette _palette;

        /// <summary>İz namludan ileriye en az <paramref name="minLength"/> uzanıyorsa çizilir (geriye düşen isabetler çizilmez).</summary>
        public static bool ShouldDrawTracer(Vector3 start, Vector3 end, Vector3 direction, float minLength)
        {
            return Vector3.Dot(end - start, direction) >= minLength;
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("FeedbackDirector: CombatFeedbackConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _palette = new TracerPalette(_config.Weapons, _config.DefaultTracerColor);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _pool))
            {
                Debug.LogWarning("FeedbackDirector: IPoolService bulunamadı; atış efektleri gösterilmeyecek.", this);
                return;
            }

            Prewarm(_config.TracerPrefab, _config.TracerPrewarm);
            Prewarm(_config.ImpactPrefab, _config.ImpactPrewarm);
            Prewarm(_config.MuzzleFlashPrefab, _config.MuzzleFlashPrewarm);
            Prewarm(_config.DebrisPrefab, _config.DebrisPrewarm);
        }

        private void OnEnable()
        {
            EventBus<WeaponFiredEvent>.Subscribe(OnWeaponFired);
            EventBus<ShotTracedEvent>.Subscribe(OnShotTraced);
            EventBus<ShotHitEvent>.Subscribe(OnShotHit);
            EventBus<StructureDestroyedEvent>.Subscribe(OnStructureDestroyed);
        }

        private void OnDisable()
        {
            EventBus<WeaponFiredEvent>.Unsubscribe(OnWeaponFired);
            EventBus<ShotTracedEvent>.Unsubscribe(OnShotTraced);
            EventBus<ShotHitEvent>.Unsubscribe(OnShotHit);
            EventBus<StructureDestroyedEvent>.Unsubscribe(OnStructureDestroyed);
        }

        private void OnWeaponFired(WeaponFiredEvent fired)
        {
            if (_pool != null && _config.MuzzleFlashPrefab != null)
            {
                _pool.Get(_config.MuzzleFlashPrefab, fired.Origin, Quaternion.identity);
            }
        }

        private void OnShotTraced(ShotTracedEvent traced)
        {
            if (_pool == null || _config.TracerPrefab == null
                || !ShouldDrawTracer(traced.Start, traced.End, traced.Direction, _config.MinTracerLength))
            {
                return;
            }

            Tracer tracer = _pool.Get(_config.TracerPrefab, traced.Start, Quaternion.identity);
            tracer.Show(traced.Start, traced.End, _palette.Get(traced.WeaponId));
        }

        private void OnShotHit(ShotHitEvent hit)
        {
            if (_pool == null || _config.ImpactPrefab == null)
            {
                return;
            }

            Vector3 normal = hit.Normal.sqrMagnitude > 0f ? hit.Normal : -hit.Direction;
            ImpactEffect impact = _pool.Get(_config.ImpactPrefab, hit.Point, Quaternion.LookRotation(normal));
            impact.Play(hit.TargetType);
        }

        private void OnStructureDestroyed(StructureDestroyedEvent destroyed)
        {
            if (_pool == null || _config.DebrisPrefab == null)
            {
                return;
            }

            Bounds bounds = destroyed.Bounds;
            DebrisEffect debris = _pool.Get(_config.DebrisPrefab, bounds.center, Quaternion.identity);
            debris.Play(bounds);
        }

        private void Prewarm(Component prefab, int count)
        {
            if (prefab != null)
            {
                _pool.Prewarm(prefab.gameObject, count);
            }
        }
    }
}
