using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Audio
{
    /// <summary>
    /// <see cref="IAudioService"/> ([Systems]/Audio). Ses kaynaklarını (<see cref="SoundVoice"/>) <see cref="IPoolService"/>'ten alır,
    /// klibi bir kez çalar ve süresi dolunca (gerçek zaman; duraklatmada da) havuza iade eder. Sınırlar saf <see cref="SoundGate"/>'te,
    /// süreler <see cref="VoiceTimers{TVoice}"/>'da. Efektler ve arayüz sesleri SFX mixer grubuna gider.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField, Tooltip("Mixer grupları, ses kaynağı prefab'ı, sınırlar.")]
        private AudioConfig _config;

        private readonly VoiceTimers<SoundVoice> _voices = new VoiceTimers<SoundVoice>();
        private readonly List<VoiceTimers<SoundVoice>.Entry> _expired = new List<VoiceTimers<SoundVoice>.Entry>();
        private SoundGate _gate;
        private IPoolService _pool;
        private bool _poolMissing;

        /// <summary>Şu an çalan efekt sayısı (debug/test).</summary>
        public int ActiveVoiceCount => _voices.Count;

        public void PlaySfx(SoundData sound, Vector3 position)
        {
            if (sound != null)
            {
                Play(sound, position, sound.IsSpatial);
            }
        }

        public void PlayUi(SoundData sound)
        {
            if (sound != null)
            {
                Play(sound, transform.position, false);
            }
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("AudioService: AudioConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _gate = new SoundGate(_config.MaxVoices);
            ServiceLocator.Register<IAudioService>(this);
        }

        private void Start()
        {
            if (_config.VoicePrefab == null)
            {
                Debug.LogError("AudioService: ses kaynağı prefab'ı (SoundVoice) yok; efektler çalmayacak. Generate Prefabs çalıştır.", this);
                return;
            }

            if (TryGetPool())
            {
                _pool.Prewarm(_config.VoicePrefab.gameObject, _config.PrewarmVoices);
            }
        }

        private void Update()
        {
            _voices.Tick(Time.unscaledDeltaTime, _expired);
            ReleaseExpired();
        }

        // Kaynaklar havuzun çocukları; sahne kapanırken onlarla birlikte yok olurlar (iade edilmez).
        private void OnDestroy()
        {
            ServiceLocator.Unregister<IAudioService>(this);
        }

        private void Play(SoundData sound, Vector3 position, bool spatial)
        {
            // Klip yoksa sessizce atla (CLAUDE.md: ses referansları opsiyonel).
            if (_gate == null || !sound.HasClips || _config.VoicePrefab == null || !TryGetPool())
            {
                return;
            }

            float now = Time.unscaledTime;
            if (!_gate.CanPlay(sound, sound.MaxSimultaneous, sound.Cooldown, now))
            {
                return;
            }

            AudioClip clip = sound.PickClip(Random.value);
            SoundVoice voice = _pool.Get(_config.VoicePrefab, position, Quaternion.identity);
            if (voice == null)
            {
                return;
            }

            float pitch = sound.PickPitch(Random.value);
            voice.Play(clip, sound, pitch, spatial, _config.SfxGroup);
            _gate.Started(sound, now);
            _voices.Add(voice, sound, SoundData.GetPlayDuration(clip, pitch));
        }

        private void ReleaseExpired()
        {
            for (int i = 0; i < _expired.Count; i++)
            {
                VoiceTimers<SoundVoice>.Entry entry = _expired[i];
                _gate?.Finished(entry.Sound);
                if (entry.Voice != null && _pool != null)
                {
                    _pool.Release(entry.Voice.gameObject);
                }
            }

            _expired.Clear();
        }

        private bool TryGetPool()
        {
            if (_pool != null)
            {
                return true;
            }

            if (_poolMissing)
            {
                return false;
            }

            if (!ServiceLocator.TryGet(out _pool))
            {
                _poolMissing = true;
                Debug.LogWarning("AudioService: IPoolService bulunamadı; efektler çalmayacak.", this);
                return false;
            }

            return true;
        }
    }
}
