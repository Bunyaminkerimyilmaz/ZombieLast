using System;
using LastHaven.Core;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Oyun akışı (saf C#, GDD §3, §9, §15). Sadece çağrılarla ve <see cref="Tick"/> ile ilerler; Unity'ye dokunmaz.
    /// <list type="bullet">
    /// <item>Boot → <see cref="Begin"/> → Day (1. gece öncesi).</item>
    /// <item>Day: süre biter veya <see cref="RequestReady"/> (erken hazır; önce <see cref="EarlyReady"/>) → Dusk.</item>
    /// <item>Dusk: süre biter → Night; geceyi başlatma callback'i çağrılır.</item>
    /// <item>Night: <see cref="NotifyNightCompleted"/> → Dawn; son gecedeyse Victory.</item>
    /// <item>Dawn: <see cref="RequestDawnContinue"/> → Day (sıradaki gece).</item>
    /// <item>Her evrede <see cref="NotifyGeneratorDestroyed"/> → GameOver. GameOver ve Victory son durumdur; sonraki çağrılar yok sayılır.</item>
    /// <item>Debug: <see cref="JumpToDay"/> her evreden (Boot ve son durumlar hariç) istenen gecenin gündüzüne.</item>
    /// </list>
    /// Tick başına en fazla bir süreli geçiş olur; biten evreden artan süre sonrakine devretmez.
    /// </summary>
    public sealed class GameStateMachine
    {
        private readonly GameFlowSettings _settings;
        private readonly Action<int> _startNight;
        private readonly DayClock _dayClock;

        /// <param name="totalNights">Toplam gece (en az 1). Bu gece atlatılınca zafer.</param>
        /// <param name="startNight">Gece evresine girilince 0 tabanlı gece indeksiyle çağrılır (dalgaları başlatır).</param>
        public GameStateMachine(GameFlowSettings settings, int totalNights, Action<int> startNight)
        {
            if (totalNights < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(totalNights), totalNights, "En az bir gece olmalı.");
            }

            _startNight = startNight ?? throw new ArgumentNullException(nameof(startNight));
            _settings = settings;
            _dayClock = new DayClock(settings.FinalWarningDuration);
            TotalNights = totalNights;
        }

        /// <summary>Evre değiştiğinde; yeni <see cref="Phase"/>, <see cref="NightIndex"/> ve <see cref="PhaseDuration"/> yazıldıktan sonra.</summary>
        public event Action PhaseChanged;

        /// <summary>Gündüzün kalan tam saniyesi (yukarı yuvarlı) değiştiğinde: gündüz başında bir kez, sonra saniyede bir (son değer 1).</summary>
        public event Action<int> DayTimerTicked;

        /// <summary>Gündüzün kalan süresi son uyarı süresine indiğinde, gündüz başına bir kez. Argüman: kalan süre (sn).</summary>
        public event Action<float> DayEndingSoon;

        /// <summary>Gündüz "Hazırım" ile bittiğinde, alacakaranlığa geçmeden önce. Argümanlar: kalan süre (sn), bonus (para).</summary>
        public event Action<float, int> EarlyReady;

        public GamePhase Phase { get; private set; } = GamePhase.Boot;

        /// <summary>0 tabanlı gece indeksi (<see cref="IGameStateService.NightIndex"/>).</summary>
        public int NightIndex { get; private set; }

        public int TotalNights { get; }

        /// <summary>Evrenin süresi (sn); süresiz evrelerde 0.</summary>
        public float PhaseDuration { get; private set; }

        /// <summary>Süreli evrede kalan süre (sn); süresiz evrelerde 0.</summary>
        public float PhaseTimeRemaining { get; private set; }

        /// <summary>
        /// Şu an <see cref="RequestReady"/> çağrılsa verilecek erken hazır bonusu (para; <see cref="EarlyReady"/> ile aynı hesap).
        /// Gündüz dışında 0.
        /// </summary>
        public int EarlyReadyBonus => Phase == GamePhase.Day ? _settings.CalculateEarlyReadyBonus(PhaseTimeRemaining) : 0;

        /// <summary>Oyun bitti mi (GameOver veya Victory).</summary>
        public bool IsOver => Phase == GamePhase.GameOver || Phase == GamePhase.Victory;

        /// <summary>Akışı başlatır: Boot → 1. gecenin gündüzü. Boot dışında false.</summary>
        public bool Begin()
        {
            if (Phase != GamePhase.Boot)
            {
                return false;
            }

            EnterDay(0);
            return true;
        }

        /// <summary>Süreli evreleri (Day, Dusk) ilerletir; diğer evrelerde bir şey yapmaz.</summary>
        public void Tick(float deltaTime)
        {
            if (Phase == GamePhase.Day)
            {
                TickDay(deltaTime);
            }
            else if (Phase == GamePhase.Dusk)
            {
                PhaseTimeRemaining = Math.Max(0f, PhaseTimeRemaining - deltaTime);
                if (PhaseTimeRemaining <= 0f)
                {
                    EnterNight();
                }
            }
        }

        /// <summary>"Hazırım": gündüzü erken bitirir (önce <see cref="EarlyReady"/>, sonra Dusk). Gündüz dışında false.</summary>
        public bool RequestReady()
        {
            if (Phase != GamePhase.Day)
            {
                return false;
            }

            float remaining = PhaseTimeRemaining;
            EarlyReady?.Invoke(remaining, _settings.CalculateEarlyReadyBonus(remaining));
            if (Phase == GamePhase.Day)
            {
                EnterDusk();
            }

            return true;
        }

        /// <summary>Gece dalgası bitti: Dawn, son gecedeyse Victory. Gece dışında false.</summary>
        public bool NotifyNightCompleted()
        {
            if (Phase != GamePhase.Night)
            {
                return false;
            }

            Enter(NightIndex >= TotalNights - 1 ? GamePhase.Victory : GamePhase.Dawn, 0f);
            return true;
        }

        /// <summary>Şafaktan devam: sıradaki gecenin gündüzü. Şafak dışında false.</summary>
        public bool RequestDawnContinue()
        {
            if (Phase != GamePhase.Dawn)
            {
                return false;
            }

            EnterDay(NightIndex + 1);
            return true;
        }

        /// <summary>
        /// Debug: herhangi bir evreden <paramref name="nightIndex"/>. gecenin gündüzüne atlar (erken hazır bonusu, şafak, faiz yok).
        /// Sürmekte olan dalgayı durdurmak çağıranın işi. Akış başlamadıysa (Boot), oyun bittiyse veya indeks geçersizse false.
        /// </summary>
        public bool JumpToDay(int nightIndex)
        {
            if (Phase == GamePhase.Boot || IsOver || nightIndex < 0 || nightIndex >= TotalNights)
            {
                return false;
            }

            EnterDay(nightIndex);
            return true;
        }

        /// <summary>Jeneratör yıkıldı: GameOver. Oyun zaten bittiyse false.</summary>
        public bool NotifyGeneratorDestroyed()
        {
            if (IsOver)
            {
                return false;
            }

            Enter(GamePhase.GameOver, 0f);
            return true;
        }

        private void TickDay(float deltaTime)
        {
            PhaseTimeRemaining = Math.Max(0f, PhaseTimeRemaining - deltaTime);
            if (PhaseTimeRemaining <= 0f)
            {
                EnterDusk();
                return;
            }

            if (_dayClock.TryAdvanceSecond(PhaseTimeRemaining, out int seconds))
            {
                DayTimerTicked?.Invoke(seconds);
            }

            WarnIfDayEndingSoon();
        }

        private void EnterDay(int nightIndex)
        {
            NightIndex = nightIndex;
            Enter(GamePhase.Day, _settings.DayDuration);

            // Dinleyici evreyi değiştirdiyse (ör. aynı anda jeneratör yıkıldı) gündüz olayları gönderilmez.
            if (Phase != GamePhase.Day)
            {
                return;
            }

            DayTimerTicked?.Invoke(_dayClock.Restart(PhaseTimeRemaining));
            WarnIfDayEndingSoon();
        }

        private void EnterDusk()
        {
            Enter(GamePhase.Dusk, _settings.DuskDuration);
        }

        private void EnterNight()
        {
            Enter(GamePhase.Night, 0f);
            if (Phase == GamePhase.Night)
            {
                _startNight(NightIndex);
            }
        }

        private void WarnIfDayEndingSoon()
        {
            if (_dayClock.TryWarn(PhaseTimeRemaining))
            {
                DayEndingSoon?.Invoke(PhaseTimeRemaining);
            }
        }

        private void Enter(GamePhase phase, float duration)
        {
            Phase = phase;
            PhaseDuration = duration;
            PhaseTimeRemaining = duration;
            PhaseChanged?.Invoke();
        }
    }
}
