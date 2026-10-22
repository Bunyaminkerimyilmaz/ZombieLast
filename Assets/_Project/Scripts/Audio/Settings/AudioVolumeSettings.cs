using LastHaven.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace LastHaven.Audio
{
    /// <summary>
    /// [Systems]/Audio: ayarlardaki genel / müzik / efekt ses düzeylerini (<see cref="ISettingsService"/>) mixer'ın açık
    /// parametrelerine desibel olarak yazar (<see cref="VolumeDecibels"/>; adlar <see cref="AudioConfig"/>'te). Başlangıçta Start'ta
    /// (mixer Awake'te yazılan değeri yok sayabiliyor), sonra her <see cref="SettingsChangedEvent"/>'te. Mixer veya parametre yoksa bir
    /// kez uyarır; sesler varsayılan düzeyde çalmaya devam eder.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioVolumeSettings : MonoBehaviour
    {
        [SerializeField, Tooltip("Mixer grupları ve ses düzeyi parametre adları. Sahne adımı bağlar.")]
        private AudioConfig _config;

        private bool _warned;

        private void OnEnable()
        {
            EventBus<SettingsChangedEvent>.Subscribe(OnSettingsChanged);
        }

        private void OnDisable()
        {
            EventBus<SettingsChangedEvent>.Unsubscribe(OnSettingsChanged);
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out ISettingsService settings))
            {
                Apply(settings.Current);
            }
        }

        private void OnSettingsChanged(SettingsChangedEvent changed)
        {
            Apply(changed.Settings);
        }

        private void Apply(GameSettings settings)
        {
            AudioMixer mixer = _config != null ? _config.Mixer : null;
            if (mixer == null)
            {
                Warn("AudioConfig veya mixer grupları yok");
                return;
            }

            float min = _config.MinVolumeDecibels;
            bool ok = mixer.SetFloat(_config.MasterVolumeParameter, VolumeDecibels.FromLinear(settings.MasterVolume, min));
            ok &= mixer.SetFloat(_config.MusicVolumeParameter, VolumeDecibels.FromLinear(settings.MusicVolume, min));
            ok &= mixer.SetFloat(_config.SfxVolumeParameter, VolumeDecibels.FromLinear(settings.SfxVolume, min));
            if (!ok)
            {
                Warn($"mixer'da ses düzeyi parametresi eksik ({_config.MasterVolumeParameter}, {_config.MusicVolumeParameter}, " +
                     $"{_config.SfxVolumeParameter})");
            }
        }

        private void Warn(string reason)
        {
            if (!_warned)
            {
                _warned = true;
                Debug.LogWarning($"AudioVolumeSettings: {reason}; ses ayarları uygulanmayacak. Tools/Last Haven/Generate Default Data çalıştır.", this);
            }
        }
    }
}
