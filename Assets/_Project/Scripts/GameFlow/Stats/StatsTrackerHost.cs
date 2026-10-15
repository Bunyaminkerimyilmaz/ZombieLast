using LastHaven.Core;
using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// [Systems]/GameFlow: run başında bir <see cref="StatsTracker"/> oluşturur ve <see cref="IRunStats"/> olarak kaydeder.
    /// <see cref="PhaseChangedEvent"/>, <see cref="EnemyKilledEvent"/> (kaynağı ne olursa olsun), <see cref="CoinCollectedEvent"/>,
    /// <see cref="ComboChangedEvent"/> ve <see cref="InterestPaidEvent"/>'i takipçiye aktarır.
    /// Sahne yeniden yüklenince (yeni run) sıfırlanır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StatsTrackerHost : MonoBehaviour
    {
        private StatsTracker _tracker;

        /// <summary>Debug ve testler için; oyun kodu servisi ServiceLocator'dan alır.</summary>
        public StatsTracker Tracker => _tracker;

        private void Awake()
        {
            _tracker = new StatsTracker();
            ServiceLocator.Register<IRunStats>(_tracker);
        }

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            EventBus<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
            EventBus<CoinCollectedEvent>.Subscribe(OnCoinCollected);
            EventBus<ComboChangedEvent>.Subscribe(OnComboChanged);
            EventBus<InterestPaidEvent>.Subscribe(OnInterestPaid);
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            EventBus<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);
            EventBus<CoinCollectedEvent>.Unsubscribe(OnCoinCollected);
            EventBus<ComboChangedEvent>.Unsubscribe(OnComboChanged);
            EventBus<InterestPaidEvent>.Unsubscribe(OnInterestPaid);
        }

        private void OnDestroy()
        {
            if (_tracker != null)
            {
                ServiceLocator.Unregister<IRunStats>(_tracker);
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            _tracker.NotifyPhaseChanged(changed.Phase);
        }

        private void OnEnemyKilled(EnemyKilledEvent killed)
        {
            _tracker.RecordKill();
        }

        private void OnCoinCollected(CoinCollectedEvent collected)
        {
            _tracker.RecordCoin(collected.Value);
        }

        private void OnComboChanged(ComboChangedEvent combo)
        {
            _tracker.RecordCombo(combo.Count);
        }

        private void OnInterestPaid(InterestPaidEvent paid)
        {
            _tracker.RecordInterest(paid.Amount);
        }
    }
}
