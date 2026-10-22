using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Progression
{
    /// <summary>
    /// [Systems]/Progression: oyuncunun XP'si ve seviyesi (GDD §13). Saf <see cref="ExperienceTracker"/>'ı Awake'te kurar ve
    /// <see cref="IExperienceService"/> olarak kaydeder. <see cref="EnemyKilledEvent"/>'in XP ödülünü ekler (config'e göre tüm
    /// öldürmeler veya sadece oyuncununkiler), <see cref="XpChangedEvent"/> ve atlanan her seviye için <see cref="LevelUpEvent"/>
    /// gönderir. Oyun bitince (<see cref="GameOverEvent"/> / <see cref="VictoryEvent"/>) XP donar. Yeni sahne = yeni run.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ExperienceService : MonoBehaviour, IExperienceService
    {
        private const string LogPrefix = "[Progression] ";

        [SerializeField, Tooltip("Seviye eşiği ve XP kaynağı. Üretici bağlar.")]
        private ProgressionConfig _config;

        private ExperienceTracker _tracker;
        private bool _runEnded;

        /// <summary>Debug ve testler için; Awake'ten önce veya config yoksa null.</summary>
        public ExperienceTracker Tracker => _tracker;

        public int Level => _tracker.Level;

        public int Xp => _tracker.Xp;

        public int XpToNextLevel => _tracker.XpToNextLevel;

        public int TotalXp => _tracker.TotalXp;

        public int PendingLevelUps => _tracker.PendingLevelUps;

        public bool TryDequeueLevelUp(out int level)
        {
            return _tracker.TryDequeueLevelUp(out level);
        }

        /// <summary>
        /// XP ekler ve olayları gönderir (önce <see cref="XpChangedEvent"/>, sonra atlanan her seviye için sırayla
        /// <see cref="LevelUpEvent"/> + log). 0 veya negatif miktar, oyun bittiyse veya config yoksa etkisiz. Debug için de açık.
        /// </summary>
        public void AddXp(int amount)
        {
            if (_tracker == null || _runEnded || amount <= 0)
            {
                return;
            }

            int levelsGained = _tracker.AddXp(amount);
            EventBus<XpChangedEvent>.Raise(new XpChangedEvent(
                _tracker.Level, _tracker.Xp, _tracker.XpToNextLevel, _tracker.TotalXp, amount));

            int firstLevel = _tracker.Level - levelsGained + 1;
            for (int i = 0; i < levelsGained; i++)
            {
                int level = firstLevel + i;
                Debug.Log($"{LogPrefix}LevelUpEvent: seviye {level} (bekleyen {_tracker.PendingLevelUps}).", this);
                EventBus<LevelUpEvent>.Raise(new LevelUpEvent(level, _tracker.PendingLevelUps));
            }
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("ExperienceService: ProgressionConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _tracker = _config.CreateExperienceTracker();
            ServiceLocator.Register<IExperienceService>(this);
        }

        private void OnEnable()
        {
            EventBus<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
            EventBus<GameOverEvent>.Subscribe(OnGameOver);
            EventBus<VictoryEvent>.Subscribe(OnVictory);
        }

        private void OnDisable()
        {
            EventBus<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);
            EventBus<GameOverEvent>.Unsubscribe(OnGameOver);
            EventBus<VictoryEvent>.Unsubscribe(OnVictory);
        }

        private void OnDestroy()
        {
            if (_tracker != null)
            {
                ServiceLocator.Unregister<IExperienceService>(this);
            }
        }

        private void OnEnemyKilled(EnemyKilledEvent killed)
        {
            if (_tracker == null || (_config.PlayerKillsOnly && !killed.KilledByPlayer))
            {
                return;
            }

            AddXp(killed.XpReward);
        }

        private void OnGameOver(GameOverEvent gameOver)
        {
            _runEnded = true;
        }

        private void OnVictory(VictoryEvent victory)
        {
            _runEnded = true;
        }
    }
}
