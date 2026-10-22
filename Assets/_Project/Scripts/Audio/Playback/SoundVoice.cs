using LastHaven.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace LastHaven.Audio
{
    /// <summary>
    /// Havuzlanan tek ses kaynağı (<c>Prefabs/Audio/SoundVoice.prefab</c>). <see cref="AudioService"/> havuzdan alır, bir klibi bir kez
    /// çalar ve süresi dolunca havuza iade eder. Azalma eğrisi (rolloff) ve diğer kaynak ayarları prefab'daki AudioSource'ta.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class SoundVoice : MonoBehaviour, IPoolable
    {
        [SerializeField, Tooltip("Aynı objedeki ses kaynağı.")]
        private AudioSource _source;

        public AudioSource Source => _source;

        /// <summary>Klibi bir kez çalar; ayarlar <paramref name="sound"/>'dan (ses düzeyi, mesafeler).</summary>
        public void Play(AudioClip clip, SoundData sound, float pitch, bool spatial, AudioMixerGroup output)
        {
            if (_source == null)
            {
                return;
            }

            _source.clip = clip;
            _source.loop = false;
            _source.volume = sound.Volume;
            _source.pitch = pitch;
            _source.spatialBlend = spatial ? 1f : 0f;
            _source.minDistance = sound.MinDistance;
            _source.maxDistance = sound.MaxDistance;
            _source.outputAudioMixerGroup = output;
            _source.Play();
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            if (_source != null)
            {
                _source.Stop();
                _source.clip = null;
            }
        }

        private void Reset()
        {
            _source = GetComponent<AudioSource>();
        }
    }
}
