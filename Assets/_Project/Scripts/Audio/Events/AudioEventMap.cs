using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Audio
{
    /// <summary>
    /// Oyun olayı → <see cref="SoundData"/> eşlemesi (<c>Data/Audio/AudioEventMap.asset</c>; üretici boş alanları doldurur).
    /// <see cref="AudioEventBinder"/> efektleri, <see cref="MusicController"/> müzik parçalarını buradan alır. Her alan opsiyoneldir:
    /// boş alan veya klipsiz ses sessizce atlanır.
    /// </summary>
    public sealed class AudioEventMap : ScriptableObject
    {
        [Header("Silahlar (WeaponFiredEvent)")]
        [SerializeField, Tooltip("Silah id'sine göre atış sesi (tetik başına bir kez).")]
        private List<WeaponSoundEntry> _weaponFired = new List<WeaponSoundEntry>();

        [SerializeField, Tooltip("Listede olmayan silahların atış sesi. Opsiyonel.")]
        private SoundData _weaponFiredFallback;

        [Header("İsabet (ShotHitEvent)")]
        [SerializeField, Tooltip("Düşmana / oyuncuya isabet (et). Opsiyonel.")]
        private SoundData _shotHitFlesh;

        [SerializeField, Tooltip("Yapıya / zemine / engele isabet. Opsiyonel.")]
        private SoundData _shotHitSurface;

        [Header("Düşmanlar")]
        [SerializeField, Tooltip("EnemyKilledEvent. Opsiyonel.")]
        private SoundData _enemyKilled;

        [SerializeField, Tooltip("BossSpawnedEvent. Opsiyonel.")]
        private SoundData _bossSpawned;

        [Header("Ekonomi")]
        [SerializeField, Tooltip("CoinCollectedEvent (jeton başına). Opsiyonel.")]
        private SoundData _coinCollected;

        [SerializeField, Tooltip("PurchaseCompletedEvent (market). Opsiyonel.")]
        private SoundData _purchaseCompleted;

        [Header("Yapılar ve güç")]
        [SerializeField, Tooltip("StructurePlacedEvent. Opsiyonel.")]
        private SoundData _structurePlaced;

        [SerializeField, Tooltip("StructureDestroyedEvent. Opsiyonel.")]
        private SoundData _structureDestroyed;

        [SerializeField, Tooltip("PowerLostEvent (yakıt bitti, jeneratör yıkıldı, fırtına kesintisi). Opsiyonel.")]
        private SoundData _powerLost;

        [Header("Akış ve ilerleme")]
        [SerializeField, Tooltip("DayEndingSoonEvent (gündüzün son 15 sn'si). Opsiyonel.")]
        private SoundData _dayEndingSoon;

        [SerializeField, Tooltip("LevelUpEvent (atlanan her seviye). Opsiyonel.")]
        private SoundData _levelUp;

        [SerializeField, Tooltip("PhaseChangedEvent: evreye göre geçiş sesi. Listede olmayan evre sessiz.")]
        private List<PhaseSoundEntry> _phaseChanged = new List<PhaseSoundEntry>();

        [Header("Arayüz")]
        [SerializeField, Tooltip("Buton tıklaması (fare, Enter, gamepad A). Opsiyonel.")]
        private SoundData _uiClick;

        [Header("Müzik")]
        [SerializeField, Tooltip("Durum → parça. Listede olmayan durum sessiz.")]
        private List<MusicTrackEntry> _music = new List<MusicTrackEntry>();

        public IReadOnlyList<WeaponSoundEntry> WeaponFired => _weaponFired;

        public SoundData WeaponFiredFallback => _weaponFiredFallback;

        public SoundData ShotHitFlesh => _shotHitFlesh;

        public SoundData ShotHitSurface => _shotHitSurface;

        public SoundData EnemyKilled => _enemyKilled;

        public SoundData BossSpawned => _bossSpawned;

        public SoundData CoinCollected => _coinCollected;

        public SoundData PurchaseCompleted => _purchaseCompleted;

        public SoundData StructurePlaced => _structurePlaced;

        public SoundData StructureDestroyed => _structureDestroyed;

        public SoundData PowerLost => _powerLost;

        public SoundData DayEndingSoon => _dayEndingSoon;

        public SoundData LevelUp => _levelUp;

        public IReadOnlyList<PhaseSoundEntry> PhaseChanged => _phaseChanged;

        public SoundData UiClick => _uiClick;

        public IReadOnlyList<MusicTrackEntry> Music => _music;

        /// <summary>Silahın atış sesi (id ordinal); listede yoksa (veya sesi boşsa) yedek ses.</summary>
        public SoundData GetWeaponFired(string weaponId)
        {
            for (int i = 0; i < _weaponFired.Count; i++)
            {
                WeaponSoundEntry entry = _weaponFired[i];
                if (entry != null && entry.Sound != null && string.Equals(entry.WeaponId, weaponId, StringComparison.Ordinal))
                {
                    return entry.Sound;
                }
            }

            return _weaponFiredFallback;
        }

        /// <summary>İsabet sesi: düşman ve oyuncu et, yapı ve yüzey sert yüzey.</summary>
        public SoundData GetShotHit(HitTargetType targetType)
        {
            return IsFlesh(targetType) ? _shotHitFlesh : _shotHitSurface;
        }

        /// <summary>Evreye geçiş sesi; yoksa null.</summary>
        public SoundData GetPhaseChanged(GamePhase phase)
        {
            for (int i = 0; i < _phaseChanged.Count; i++)
            {
                PhaseSoundEntry entry = _phaseChanged[i];
                if (entry != null && entry.Phase == phase)
                {
                    return entry.Sound;
                }
            }

            return null;
        }

        /// <summary>Durumun parçası (ilk eşleşen); yoksa false.</summary>
        public bool TryGetMusic(MusicCue cue, out MusicTrackEntry track)
        {
            for (int i = 0; i < _music.Count; i++)
            {
                if (_music[i] != null && _music[i].Cue == cue)
                {
                    track = _music[i];
                    return true;
                }
            }

            track = null;
            return false;
        }

        public static bool IsFlesh(HitTargetType targetType)
        {
            return targetType == HitTargetType.Enemy || targetType == HitTargetType.Player;
        }
    }
}
