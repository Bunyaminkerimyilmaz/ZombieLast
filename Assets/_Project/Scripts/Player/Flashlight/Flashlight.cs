using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Oyuncunun feneri (GDD §18): kökün çocuğundaki spot ışık (Visual dışında; kök nişan yönüne döndüğü için fener de döner).
    /// Gece açık, gündüz kapalı: <see cref="PhaseChangedEvent"/> + <see cref="PhaseLighting"/> kuralıyla alacakaranlıkta yanar,
    /// şafakta söner. Oyuncu ölüyken kapalıdır (devrilen gövdenin üstünde havada yanmasın), doğunca gece ise yeniden yanar.
    /// Oyun gündüz başladığı için kapalı başlar.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Flashlight : MonoBehaviour
    {
        [SerializeField, Tooltip("Fenerin spot ışığı. Üretici bağlar.")]
        private Light _light;

        private GameObject _player;
        private bool _isNight;
        private bool _isDead;

        /// <summary>Fener şu an yanıyor mu.</summary>
        public bool IsOn => _light != null && _light.enabled;

        private void Awake()
        {
            if (_light == null)
            {
                Debug.LogWarning("Flashlight: spot ışık atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
            }

            var character = GetComponentInParent<PlayerCharacter>();
            _player = character != null ? character.gameObject : null;
            Refresh();
        }

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            EventBus<PlayerDiedEvent>.Subscribe(OnPlayerDied);
            EventBus<PlayerRespawnedEvent>.Subscribe(OnPlayerRespawned);
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            EventBus<PlayerDiedEvent>.Unsubscribe(OnPlayerDied);
            EventBus<PlayerRespawnedEvent>.Unsubscribe(OnPlayerRespawned);
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            LightingTarget target = PhaseLighting.For(changed.Phase);
            if (target == LightingTarget.Unchanged)
            {
                return;
            }

            _isNight = target == LightingTarget.Night;
            Refresh();
        }

        private void OnPlayerDied(PlayerDiedEvent died)
        {
            if (died.Player == _player)
            {
                _isDead = true;
                Refresh();
            }
        }

        private void OnPlayerRespawned(PlayerRespawnedEvent respawned)
        {
            if (respawned.Player == _player)
            {
                _isDead = false;
                Refresh();
            }
        }

        private void Refresh()
        {
            if (_light != null)
            {
                _light.enabled = _isNight && !_isDead;
            }
        }
    }
}
