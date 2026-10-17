using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// Gündüz istihbaratı (GDD §9): gündüz evresinde bu gecenin saldırı yönlerindeki <see cref="SpawnZone"/>'ların kırmızı ışık
    /// sütununu yakar, diğer evrelerde söndürür. Yönler <see cref="IWaveService.GetTonightInfo"/>'dan, evre
    /// <see cref="PhaseChangedEvent"/>'ten (açılışta <see cref="IGameStateService"/>'ten).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpawnZoneBeacons : MonoBehaviour
    {
        [SerializeField, Tooltip("Işık sütunu yakılacak bölgeler ([Gameplay]/SpawnZones). Sahne adımı bağlar.")]
        private SpawnZone[] _zones = Array.Empty<SpawnZone>();

        private IWaveService _waves;
        private IGameStateService _gameState;
        private bool _started;

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            if (_started)
            {
                ShowCurrent();
            }
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _waves))
            {
                Debug.LogWarning("SpawnZoneBeacons: IWaveService bulunamadı; ışık sütunları yanmayacak.", this);
            }

            ServiceLocator.TryGet(out _gameState);
            _started = true;
            ShowCurrent();
        }

        private void ShowCurrent()
        {
            if (_gameState != null)
            {
                Show(_gameState.CurrentPhase, _gameState.NightIndex);
            }
            else
            {
                Show(GamePhase.Boot, 0);
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            Show(changed.Phase, changed.NightIndex);
        }

        private void Show(GamePhase phase, int nightIndex)
        {
            bool isDay = phase == GamePhase.Day && _waves != null && nightIndex >= 0 && nightIndex < _waves.TotalNights;
            NightInfo tonight = isDay ? _waves.GetTonightInfo(nightIndex) : null;
            for (int i = 0; i < _zones.Length; i++)
            {
                SpawnZone zone = _zones[i];
                if (zone != null)
                {
                    zone.SetBeaconShown(tonight != null && tonight.HasDirection(zone.Direction));
                }
            }
        }
    }
}
