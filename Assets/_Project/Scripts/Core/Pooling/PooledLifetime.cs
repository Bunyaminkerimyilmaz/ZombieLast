using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Süre dolunca objeyi <see cref="IPoolService"/>'e iade eder (havuz servisi yoksa yok eder).
    /// Süre her etkinleşmede ve havuzdan her çıkışta baştan başlar. Görseller solmak için <see cref="Progress"/>'i okur.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PooledLifetime : MonoBehaviour, IPoolable
    {
        [SerializeField, Min(0f), Tooltip("Obje bu kadar sn sonra havuza döner.")]
        private float _lifetime = 1f;

        private readonly Countdown _countdown = new Countdown();

        public float Lifetime => _lifetime;

        /// <summary>Ömür ilerlemesi 0 → 1; süre dolmuşsa veya hiç başlamadıysa 1.</summary>
        public float Progress => _countdown.Progress;

        public bool IsRunning => _countdown.IsRunning;

        public void OnSpawned()
        {
            _countdown.Start(_lifetime);
        }

        public void OnDespawned()
        {
            _countdown.Stop();
        }

        /// <summary>Süreyi ilerletir; dolduysa objeyi iade eder. Update'ten çağrılır; testler doğrudan çağırır.</summary>
        internal void Advance(float deltaTime)
        {
            if (_countdown.Tick(deltaTime))
            {
                Expire();
            }
        }

        private void OnEnable()
        {
            // Havuz dışında (sahneye elle konmuş) objelerde de süre işlesin.
            _countdown.Start(_lifetime);
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        private void Expire()
        {
            if (ServiceLocator.TryGet(out IPoolService pool))
            {
                pool.Release(gameObject);
                return;
            }

            Destroy(gameObject);
        }
    }
}
