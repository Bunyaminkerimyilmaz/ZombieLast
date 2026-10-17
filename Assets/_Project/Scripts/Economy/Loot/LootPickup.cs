using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// Sandıktan düşen toplanabilir ödül (yakıt bidonu, mermi kutusu; Pickup layer). Hareketi ve ömrü jetonlarla aynı saf
    /// <see cref="CoinMotion"/>'da: yay çizerek düşer, oyuncunun mıknatısına girince ona uçar, toplanmazsa ömrünün sonunda yanıp
    /// söner ve kaybolur. Sonucu <see cref="LootDispenser"/>'a bildirir; türü prefab'da (<see cref="RewardType"/>).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LootPickup : MonoBehaviour, IPoolable
    {
        [SerializeField, Tooltip("Toplanınca verilen ödül. Üretici prefab'a yazar.")]
        private LootRewardType _rewardType = LootRewardType.Fuel;

        private readonly CoinMotion _motion = new CoinMotion();
        private Renderer[] _renderers;
        private bool _renderersVisible = true;
        private LootDispenser _owner;

        public LootRewardType RewardType => _rewardType;

        /// <summary>Yakıt birimi veya mermi paketi adedi.</summary>
        public int Amount { get; private set; }

        /// <summary>Hareket durumu (debug ve testler için).</summary>
        public CoinMotion Motion => _motion;

        /// <summary>Ödülü <paramref name="start"/>'tan <paramref name="landing"/>'e fırlatır. Havuzdan alındıktan hemen sonra çağrılır.</summary>
        public void Launch(
            LootDispenser owner,
            int amount,
            CoinMotionSettings settings,
            Vector3 start,
            Vector3 landing,
            float lifetime,
            float blinkDuration,
            float spinAngle)
        {
            _owner = owner;
            Amount = amount;
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
            Amount = 0;
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
