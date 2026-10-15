using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// Havuzlanan jeton (Pickup layer). Hareketi ve ömrü saf <see cref="CoinMotion"/>'da; bu bileşen sonucu Transform'a yazar
    /// (kök döner, model "Visual" altında), yanıp sönerken alt objelerdeki renderer'ları kapatıp açar ve sonucu
    /// <see cref="CoinDropper"/>'a bildirir (toplandı / süresi doldu). <see cref="CoinDropper"/> fırlatmadan hiçbir şey yapmaz.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Coin : MonoBehaviour, IPoolable
    {
        private readonly CoinMotion _motion = new CoinMotion();
        private Renderer[] _renderers;
        private bool _renderersVisible = true;
        private CoinDropper _owner;

        /// <summary>Jetonun değeri (para).</summary>
        public int Value { get; private set; }

        /// <summary>Hareket durumu (debug ve testler için).</summary>
        public CoinMotion Motion => _motion;

        /// <summary>
        /// Jetonu <paramref name="start"/>'tan <paramref name="landing"/>'e fırlatır. Havuzdan alındıktan hemen sonra
        /// <see cref="CoinDropper"/> çağırır.
        /// </summary>
        public void Launch(
            CoinDropper owner,
            int value,
            CoinMotionSettings settings,
            Vector3 start,
            Vector3 landing,
            float lifetime,
            float blinkDuration,
            float spinAngle)
        {
            _owner = owner;
            Value = value;
            _motion.Launch(settings, start, landing, lifetime, blinkDuration, spinAngle);
            ApplyMotion();
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            _motion.Stop();
            _owner = null;
            Value = 0;
            SetRenderersVisible(true);
        }

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Update()
        {
            if (_owner == null || _motion.State == CoinState.Inactive)
            {
                return;
            }

            CoinStep step = _motion.Tick(Time.deltaTime, _owner.Magnet);
            ApplyMotion();

            if (step == CoinStep.Collected)
            {
                _owner.Collect(this);
            }
            else if (step == CoinStep.Expired)
            {
                _owner.Expire(this);
            }
        }

        private void ApplyMotion()
        {
            transform.SetPositionAndRotation(_motion.Position, Quaternion.Euler(0f, _motion.SpinAngle, 0f));
            SetRenderersVisible(_motion.IsVisible);
        }

        private void SetRenderersVisible(bool visible)
        {
            if (_renderers == null || _renderersVisible == visible)
            {
                return;
            }

            _renderersVisible = visible;
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = visible;
            }
        }
    }
}
