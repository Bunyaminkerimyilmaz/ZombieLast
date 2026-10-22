using System;
using Unity.Profiling;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Unity'nin "GC Allocated In Frame" sayacını (<see cref="ProfilerRecorder"/>) her kare okuyup <see cref="Window"/>'a yazar.
    /// Sayaç editörde ve development build'de vardır (Profiler penceresi açık olmasa da). Editörde editörün kendi ayırmaları
    /// (UI Toolkit canlı yeniden yükleme, Cinemachine'in editör OnGUI'si) da sayılır; oyunun payı için Profiler'da PlayerLoop'a bak.
    /// </summary>
    public sealed class GcAllocationMeter : IDisposable
    {
        private const string CounterName = "GC Allocated In Frame";

        private ProfilerRecorder _recorder;

        public AllocationWindow Window { get; } = new AllocationWindow();

        /// <summary>Sayaç okunabiliyor mu (sürüm build'inde false).</summary>
        public bool IsAvailable => _recorder.Valid;

        /// <summary>Okumaya başlar (zaten başladıysa sadece pencereyi sıfırlar).</summary>
        public void Start()
        {
            if (!_recorder.Valid)
            {
                _recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, CounterName);
            }

            Window.Reset();
        }

        /// <summary>Son tamamlanan karenin ayırmasını ekler; kare başına bir kez çağrılır.</summary>
        public void Sample()
        {
            if (_recorder.Valid)
            {
                Window.Add(_recorder.LastValue);
            }
        }

        public void Dispose()
        {
            _recorder.Dispose();
        }
    }
}
