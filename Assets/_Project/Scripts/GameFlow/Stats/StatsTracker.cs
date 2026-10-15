using LastHaven.Core;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Run istatistikleri (saf C#). Evreyi <see cref="NotifyPhaseChanged"/> ile, öldürme/jeton/kombo/faizi Record* metotlarıyla alır.
    /// Gece evresine girişte gece sayaçları (ve son faiz) sıfırlanır; sadece gece evresindekiler geceye sayılır, hepsi toplama.
    /// Faiz şafakta ödendiği için gece evresine bakmaz. Oyun bitince (GameOver/Victory) sayaçlar donar: oyun durmasa da
    /// ekrandaki sonuç değişmez.
    /// </summary>
    public sealed class StatsTracker : IRunStats
    {
        /// <inheritdoc />
        public int NightKills { get; private set; }

        /// <inheritdoc />
        public int TotalKills { get; private set; }

        /// <inheritdoc />
        public int NightMoneyEarned { get; private set; }

        /// <inheritdoc />
        public int TotalMoneyEarned { get; private set; }

        /// <inheritdoc />
        public int NightBestCombo { get; private set; }

        /// <inheritdoc />
        public int BestCombo { get; private set; }

        /// <inheritdoc />
        public int LastInterest { get; private set; }

        /// <inheritdoc />
        public int TotalInterest { get; private set; }

        /// <summary>Gece evresinde mi (değerler geceye sayılıyor mu).</summary>
        public bool IsNightActive { get; private set; }

        /// <summary>Oyun bitti (yenilgi veya zafer); sayaçlar artık değişmez.</summary>
        public bool IsFinished { get; private set; }

        public void NotifyPhaseChanged(GamePhase phase)
        {
            if (IsFinished)
            {
                return;
            }

            if (phase == GamePhase.GameOver || phase == GamePhase.Victory)
            {
                IsFinished = true;
                IsNightActive = false;
                return;
            }

            bool night = phase == GamePhase.Night;
            if (night && !IsNightActive)
            {
                NightKills = 0;
                NightMoneyEarned = 0;
                NightBestCombo = 0;
                LastInterest = 0;
            }

            IsNightActive = night;
        }

        public void RecordKill()
        {
            if (IsFinished)
            {
                return;
            }

            TotalKills++;
            if (IsNightActive)
            {
                NightKills++;
            }
        }

        /// <summary>Toplanan jeton (değeri). 0 ve negatif yok sayılır.</summary>
        public void RecordCoin(int value)
        {
            if (IsFinished || value <= 0)
            {
                return;
            }

            TotalMoneyEarned = SaturatingAdd(TotalMoneyEarned, value);
            if (IsNightActive)
            {
                NightMoneyEarned = SaturatingAdd(NightMoneyEarned, value);
            }
        }

        /// <summary>Kombonun anlık sayısı; en yüksek değer tutulur.</summary>
        public void RecordCombo(int count)
        {
            if (IsFinished)
            {
                return;
            }

            if (count > BestCombo)
            {
                BestCombo = count;
            }

            if (IsNightActive && count > NightBestCombo)
            {
                NightBestCombo = count;
            }
        }

        /// <summary>Şafakta ödenen faiz (0 da olabilir; son faiz yine güncellenir).</summary>
        public void RecordInterest(int amount)
        {
            if (IsFinished || amount < 0)
            {
                return;
            }

            LastInterest = amount;
            TotalInterest = SaturatingAdd(TotalInterest, amount);
        }

        private static int SaturatingAdd(int value, int amount)
        {
            return value > int.MaxValue - amount ? int.MaxValue : value + amount;
        }
    }
}
