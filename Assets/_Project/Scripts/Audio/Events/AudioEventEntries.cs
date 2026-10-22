using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Audio
{
    /// <summary>Silah id'si → atış sesi (<see cref="AudioEventMap"/>).</summary>
    [Serializable]
    public sealed class WeaponSoundEntry
    {
        [SerializeField, Tooltip("Silahın id'si (WeaponData.Id; taret: turret). Ordinal, büyük/küçük harf duyarlı.")]
        private string _weaponId = string.Empty;

        [SerializeField, Tooltip("Atış sesi. Opsiyonel.")]
        private SoundData _sound;

        public WeaponSoundEntry()
        {
        }

        public WeaponSoundEntry(string weaponId, SoundData sound)
        {
            _weaponId = weaponId;
            _sound = sound;
        }

        public string WeaponId => _weaponId ?? string.Empty;

        public SoundData Sound => _sound;
    }

    /// <summary>Evre → geçiş sesi (<see cref="AudioEventMap"/>).</summary>
    [Serializable]
    public sealed class PhaseSoundEntry
    {
        [SerializeField, Tooltip("Bu evreye geçilince çalar.")]
        private GamePhase _phase;

        [SerializeField, Tooltip("Geçiş sesi. Opsiyonel.")]
        private SoundData _sound;

        public PhaseSoundEntry()
        {
        }

        public PhaseSoundEntry(GamePhase phase, SoundData sound)
        {
            _phase = phase;
            _sound = sound;
        }

        public GamePhase Phase => _phase;

        public SoundData Sound => _sound;
    }

    /// <summary>Müzik parçası (<see cref="MusicController"/>).</summary>
    [Serializable]
    public sealed class MusicTrackEntry
    {
        [SerializeField, Tooltip("Hangi durumda çalar.")]
        private MusicCue _cue;

        [SerializeField, Tooltip("Parça (klip listesinden biri seçilir; ses düzeyi buradan). Opsiyonel: yoksa o durumda sessizlik.")]
        private SoundData _sound;

        [SerializeField, Tooltip("Bitince başa sarar (gündüz/gece/boss). Kapalıysa bir kez çalar.")]
        private bool _loop = true;

        public MusicTrackEntry()
        {
        }

        public MusicTrackEntry(MusicCue cue, SoundData sound, bool loop)
        {
            _cue = cue;
            _sound = sound;
            _loop = loop;
        }

        public MusicCue Cue => _cue;

        public SoundData Sound => _sound;

        public bool Loop => _loop;
    }
}
