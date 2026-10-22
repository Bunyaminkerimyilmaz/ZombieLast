using System;
using System.Globalization;
using LastHaven.Core;
using UnityEngine.UIElements;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Debug panelinin "Durum" satırları (<c>UI/Debug/DebugPanel.uxml</c>): FPS, canlı düşman, evre, gece, dalga kalan/toplam, zaman
    /// ölçeği, kare başı GC ve stres testi. Sayılar <see cref="PanelNumbers"/> önbelleğinden, "a / b" çiftleri iki ayrı etiket: yenileme
    /// string ayırmaz (panel kendi ölçtüğü GC'yi kirletmesin). Değer değişmedikçe metin yazılmaz. Evre ve ölçek sadece değişince.
    /// </summary>
    public sealed class DebugPanelStatusView
    {
        private const string NoValue = "—";

        private readonly Label _fps;
        private readonly Label _enemies;
        private readonly Label _phase;
        private readonly Label _night;
        private readonly Label _nightTotal;
        private readonly Label _remaining;
        private readonly Label _remainingTotal;
        private readonly Label _timeScale;
        private readonly Label _gc;
        private readonly Label _gcFrames;
        private readonly Label _stressAlive;
        private readonly Label _stressTarget;
        private int _phaseValue = int.MinValue;
        private float _timeScaleValue = float.NaN;

        /// <param name="find">Adla Label bulur (bulamazsa null; eksikliği çağıran raporlar).</param>
        public DebugPanelStatusView(Func<string, Label> find)
        {
            _fps = find(DebugPanelElementNames.FpsValue);
            _enemies = find(DebugPanelElementNames.EnemiesValue);
            _phase = find(DebugPanelElementNames.PhaseValue);
            _night = find(DebugPanelElementNames.NightValue);
            _nightTotal = find(DebugPanelElementNames.NightTotal);
            _remaining = find(DebugPanelElementNames.RemainingValue);
            _remainingTotal = find(DebugPanelElementNames.RemainingTotal);
            _timeScale = find(DebugPanelElementNames.TimeScaleValue);
            _gc = find(DebugPanelElementNames.GcValue);
            _gcFrames = find(DebugPanelElementNames.GcFramesValue);
            _stressAlive = find(DebugPanelElementNames.StressAlive);
            _stressTarget = find(DebugPanelElementNames.StressTarget);
        }

        /// <summary>Zaman ölçeği metni: "0.5×", "2×".</summary>
        public static string FormatScale(float scale)
        {
            return scale.ToString("0.##", CultureInfo.InvariantCulture) + "×";
        }

        /// <summary>Son pencerenin FPS'i; 0 veya altı "—".</summary>
        public void SetFps(float fps)
        {
            int rounded = fps > 0f ? (int)Math.Round(fps) : 0;
            SetText(_fps, rounded > 0 ? PanelNumbers.Get(rounded) : NoValue);
        }

        public void SetAliveEnemies(int count)
        {
            SetText(_enemies, PanelNumbers.Get(count));
        }

        public void SetPhase(GamePhase phase)
        {
            if ((int)phase != _phaseValue)
            {
                _phaseValue = (int)phase;
                _phase.text = phase.ToString();
            }
        }

        public void SetNight(int number, int total)
        {
            SetPair(_night, _nightTotal, true, number, total);
        }

        /// <summary>Dalga kalan / toplam; gece sürmüyorsa "— / —".</summary>
        public void SetRemaining(bool nightRunning, int remaining, int total)
        {
            SetPair(_remaining, _remainingTotal, nightRunning, remaining, total);
        }

        public void SetTimeScale(float scale)
        {
            if (!scale.Equals(_timeScaleValue))
            {
                _timeScaleValue = scale;
                _timeScale.text = FormatScale(scale);
            }
        }

        /// <summary>Kare başı ortalama GC (bayt) ve ayıran karelerin yüzdesi; ölçülemiyorsa "—".</summary>
        public void SetGc(bool available, double averageBytes, double allocatingRatio)
        {
            SetText(_gc, available ? PanelNumbers.Get((int)Math.Round(averageBytes)) : NoValue);
            SetText(_gcFrames, available ? PanelNumbers.Get((int)Math.Round(allocatingRatio * 100d)) : NoValue);
        }

        /// <summary>Stres testinin canlı / hedef düşmanı; test çalışmıyorsa "— / —".</summary>
        public void SetStress(bool running, int alive, int target)
        {
            SetPair(_stressAlive, _stressTarget, running, alive, target);
        }

        private static void SetPair(Label first, Label second, bool shown, int a, int b)
        {
            SetText(first, shown ? PanelNumbers.Get(a) : NoValue);
            SetText(second, shown ? PanelNumbers.Get(b) : NoValue);
        }

        // Aynı (önbellekli) string referansı tekrar yazılmaz: metin değişmedikçe UI yeniden ölçülmez.
        private static void SetText(Label label, string text)
        {
            if (!ReferenceEquals(label.text, text))
            {
                label.text = text;
            }
        }
    }
}
