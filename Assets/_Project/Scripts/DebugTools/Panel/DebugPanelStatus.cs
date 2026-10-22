#if UNITY_EDITOR || DEVELOPMENT_BUILD
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Debug panelinin durum verisi: FPS (<see cref="FrameRateCounter"/>, gerçek zaman), canlı düşman (<see cref="DebugWaveControls"/>),
    /// evre ve gece (<see cref="IGameStateService"/>), dalga kalan/toplam (<see cref="WaveProgressEvent"/>), zaman ölçeği
    /// (<see cref="ITimeScaleService"/>), kare başı GC (<see cref="GcAllocationMeter"/>; sadece panel açıkken ölçülür, her yenilemede
    /// sıfırlanır: son yarım saniye) ve stres testi (<see cref="DebugStressTest"/>). Eksik servisin satırı yazılmaz veya "—" kalır.
    /// Kısayol bileşenleri gibi sadece editörde ve development build'de derlenir.
    /// </summary>
    public sealed class DebugPanelStatus
    {
        private readonly FrameRateCounter _fps;
        private readonly GcAllocationMeter _gc = new GcAllocationMeter();
        private readonly DebugWaveControls _waveControls;
        private readonly DebugStressTest _stress;
        private IGameStateService _gameState;
        private IWaveService _waves;
        private ITimeScaleService _timeScale;
        private int _waveRemaining;
        private int _waveTotal;

        /// <param name="refreshInterval">FPS penceresi = yenileme aralığı (sn).</param>
        public DebugPanelStatus(DebugWaveControls waveControls, DebugStressTest stress, float refreshInterval)
        {
            _waveControls = waveControls;
            _stress = stress;
            _fps = new FrameRateCounter(refreshInterval);
        }

        /// <summary>Runtime servisleri (Start'ta). Null olabilir.</summary>
        public void SetServices(IGameStateService gameState, IWaveService waves, ITimeScaleService timeScale)
        {
            _gameState = gameState;
            _waves = waves;
            _timeScale = timeScale;
        }

        public void Subscribe()
        {
            EventBus<WaveProgressEvent>.Subscribe(OnWaveProgress);
        }

        public void Unsubscribe()
        {
            EventBus<WaveProgressEvent>.Unsubscribe(OnWaveProgress);
        }

        /// <summary>Panel açıldı: FPS ve GC ölçümü baştan başlar.</summary>
        public void Begin()
        {
            _fps.Reset();
            _gc.Start();
        }

        /// <summary>Panel kapandı: GC sayacı bırakılır.</summary>
        public void End()
        {
            _gc.Dispose();
        }

        /// <summary>Kare ekler; yenileme zamanı geldiyse true.</summary>
        public bool Tick(float unscaledDeltaTime)
        {
            _gc.Sample();
            return _fps.Tick(unscaledDeltaTime);
        }

        public void WriteTo(DebugPanelView view)
        {
            DebugPanelStatusView status = view.Status;
            status.SetFps(_fps.Fps);
            status.SetAliveEnemies(_waveControls != null ? _waveControls.CountAliveEnemies() : 0);
            if (_gameState != null)
            {
                status.SetPhase(_gameState.CurrentPhase);
                status.SetNight(_gameState.NightIndex + 1, _waves != null ? _waves.TotalNights : 0);
            }

            status.SetRemaining(_waves != null && _waves.IsNightRunning, _waveRemaining, _waveTotal);
            float scale = _timeScale != null ? _timeScale.TimeScale : Time.timeScale;
            status.SetTimeScale(scale);
            view.SelectTimeScale(scale);

            AllocationWindow gc = _gc.Window;
            status.SetGc(_gc.IsAvailable && gc.Frames > 0, gc.AverageBytes, gc.AllocatingRatio);
            gc.Reset();

            bool stressRunning = _stress != null && _stress.IsRunning;
            status.SetStress(stressRunning, stressRunning ? _stress.AliveCount : 0, _stress != null ? _stress.EnemyTarget : 0);
            view.SetStressRunning(stressRunning);
        }

        private void OnWaveProgress(WaveProgressEvent progress)
        {
            _waveRemaining = progress.RemainingEnemies;
            _waveTotal = progress.TotalEnemies;
        }
    }
}
#endif
