using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Audio
{
    /// <summary>
    /// Evre bazlı müzik ([Systems]/Audio/Music): <see cref="PhaseChangedEvent"/> ve boss olaylarından parçayı seçer
    /// (<see cref="MusicSelector"/>), iki kaynak arasında geçişle değiştirir (<see cref="MusicCrossfade"/>, gerçek zaman; duraklatmada da).
    /// Parçalar <see cref="AudioEventMap"/>'te; parçası veya klibi olmayan durum sessizliğe geçer. Kaynaklar Music mixer grubuna gider.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MusicController : MonoBehaviour
    {
        [SerializeField, Tooltip("Geçiş süresi ve Music mixer grubu.")]
        private AudioConfig _config;

        [SerializeField, Tooltip("Durum → parça eşlemesi.")]
        private AudioEventMap _map;

        [SerializeField, Tooltip("Birinci müzik kaynağı (2D).")]
        private AudioSource _deckA;

        [SerializeField, Tooltip("İkinci müzik kaynağı (2D); geçişte biri açılırken öbürü kısılır.")]
        private AudioSource _deckB;

        private readonly MusicSelector _selector = new MusicSelector();
        private readonly float[] _deckVolumes = new float[2];
        private MusicCrossfade _fade;

        /// <summary>Şu an seçili parça (geçiş sürüyor olabilir).</summary>
        public MusicCue CurrentCue { get; private set; } = MusicCue.None;

        private void Awake()
        {
            if (_config == null || _map == null || _deckA == null || _deckB == null)
            {
                Debug.LogError("MusicController: config, eşleme veya müzik kaynakları atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _fade = new MusicCrossfade(_config.MusicCrossfadeDuration);
            PrepareDeck(_deckA);
            PrepareDeck(_deckB);
        }

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            EventBus<BossSpawnedEvent>.Subscribe(OnBossSpawned);
            EventBus<BossDefeatedEvent>.Subscribe(OnBossDefeated);
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            EventBus<BossSpawnedEvent>.Unsubscribe(OnBossSpawned);
            EventBus<BossDefeatedEvent>.Unsubscribe(OnBossDefeated);
        }

        private void Update()
        {
            _fade.Tick(Time.unscaledDeltaTime);
            ApplyVolumes();
            if (!_fade.IsFading)
            {
                AudioSource outgoing = GetDeck(1 - _fade.ActiveDeck);
                if (outgoing.isPlaying)
                {
                    outgoing.Stop();
                }
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            _selector.SetPhase(changed.Phase);
            Refresh();
        }

        private void OnBossSpawned(BossSpawnedEvent spawned)
        {
            _selector.BossSpawned(spawned.Boss);
            Refresh();
        }

        private void OnBossDefeated(BossDefeatedEvent defeated)
        {
            _selector.BossDefeated(defeated.Boss);
            Refresh();
        }

        private void Refresh()
        {
            MusicCue cue = _selector.Current;
            if (cue == CurrentCue)
            {
                return;
            }

            CurrentCue = cue;
            int deck = _fade.Begin();
            AudioSource source = GetDeck(deck);
            source.Stop();
            source.clip = null;
            _deckVolumes[deck] = 0f;

            if (_map.TryGetMusic(cue, out MusicTrackEntry track) && track.Sound != null && track.Sound.HasClips)
            {
                SoundData sound = track.Sound;
                source.clip = sound.PickClip(Random.value);
                source.pitch = sound.PickPitch(Random.value);
                source.loop = track.Loop;
                _deckVolumes[deck] = sound.Volume;
                source.volume = 0f;
                source.Play();
                Debug.Log($"[Audio] Müzik: {cue} ({sound.name}).");
            }

            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            _deckA.volume = _deckVolumes[0] * _fade.GetWeight(0);
            _deckB.volume = _deckVolumes[1] * _fade.GetWeight(1);
        }

        private AudioSource GetDeck(int deck)
        {
            return deck == 0 ? _deckA : _deckB;
        }

        private void PrepareDeck(AudioSource source)
        {
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = _config.MusicGroup;
            source.Stop();
            source.volume = 0f;
        }
    }
}
