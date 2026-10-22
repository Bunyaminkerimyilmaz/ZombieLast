using System.Collections.Generic;
using LastHaven.Core;

namespace LastHaven.Audio
{
    /// <summary>
    /// Hangi müziğin çalacağı (saf C#): evre + yaşayan boss. Gündüz ve şafak sakin (<see cref="MusicCue.Day"/>), alacakaranlık ve
    /// gece gerilim (<see cref="MusicCue.Night"/>; alacakaranlıkta başlar ki gece başlarken yerine oturmuş olsun), gece boss
    /// yaşıyorsa <see cref="MusicCue.Boss"/>, oyun bitince yenilgi/zafer. Bosslar referansla tutulur; şafak ve gündüzde temizlenir.
    /// </summary>
    public sealed class MusicSelector
    {
        private readonly HashSet<object> _bosses = new HashSet<object>();

        public GamePhase Phase { get; private set; } = GamePhase.Boot;

        public int ActiveBossCount => _bosses.Count;

        /// <summary>Şu anki duruma göre çalması gereken parça.</summary>
        public MusicCue Current => Select(Phase, _bosses.Count > 0);

        public void SetPhase(GamePhase phase)
        {
            Phase = phase;
            if (phase == GamePhase.Day || phase == GamePhase.Dawn)
            {
                // Gece ancak tüm düşmanlar ölünce biter; kalan kayıt havuzdan ölmeden dönen boss olur.
                _bosses.Clear();
            }
        }

        public void BossSpawned(object boss)
        {
            if (boss != null)
            {
                _bosses.Add(boss);
            }
        }

        public void BossDefeated(object boss)
        {
            if (boss != null)
            {
                _bosses.Remove(boss);
            }
        }

        /// <summary>Evre ve boss durumundan parça.</summary>
        public static MusicCue Select(GamePhase phase, bool bossActive)
        {
            switch (phase)
            {
                case GamePhase.Day:
                case GamePhase.Dawn:
                    return MusicCue.Day;
                case GamePhase.Dusk:
                    return MusicCue.Night;
                case GamePhase.Night:
                    return bossActive ? MusicCue.Boss : MusicCue.Night;
                case GamePhase.GameOver:
                    return MusicCue.GameOver;
                case GamePhase.Victory:
                    return MusicCue.Victory;
                default:
                    return MusicCue.None;
            }
        }
    }
}
