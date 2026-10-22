using UnityEngine;
using UnityEngine.Audio;

namespace LastHaven.Audio
{
    /// <summary>
    /// Ses altyapısının ayarları (<c>Data/Audio/AudioConfig.asset</c>; üretici oluşturur ve mixer gruplarını, ses kaynağı prefab'ını
    /// bağlar): mixer grupları (Master altında Music ve SFX), ayarların yazdığı ses düzeyi parametreleri (14.3), havuzlanan ses kaynağı,
    /// toplam kaynak sınırı, müzik geçişi ve dinleyici.
    /// </summary>
    public sealed class AudioConfig : ScriptableObject
    {
        [Header("Mixer (Master → Music / SFX)")]
        [SerializeField, Tooltip("Efektler ve arayüz sesleri bu gruba gider.")]
        private AudioMixerGroup _sfxGroup;

        [SerializeField, Tooltip("Müzik bu gruba gider.")]
        private AudioMixerGroup _musicGroup;

        [Header("Ses düzeyleri (ayarlar → mixer'ın açık parametreleri)")]
        [SerializeField, Tooltip("Master grubunun ses düzeyi parametresi (dB). Üretici mixer'da bu adla açar.")]
        private string _masterVolumeParameter = "MasterVolume";

        [SerializeField, Tooltip("Music grubunun ses düzeyi parametresi (dB).")]
        private string _musicVolumeParameter = "MusicVolume";

        [SerializeField, Tooltip("SFX grubunun ses düzeyi parametresi (dB).")]
        private string _sfxVolumeParameter = "SfxVolume";

        [SerializeField, Tooltip("Ayarda 0 olan düzeyin desibeli (sessiz).")]
        private float _minVolumeDecibels = -80f;

        [Header("Ses kaynakları (havuz)")]
        [SerializeField, Tooltip("Havuzlanan ses kaynağı prefab'ı (AudioSource + SoundVoice; azalma eğrisi burada).")]
        private SoundVoice _voicePrefab;

        [SerializeField, Min(0), Tooltip("Oyun başında hazırlanan ses kaynağı sayısı.")]
        private int _prewarmVoices = 16;

        [SerializeField, Min(1), Tooltip("Aynı anda çalan efekt sınırı; doluyken yeni istek atlanır.")]
        private int _maxVoices = 32;

        [Header("Müzik")]
        [SerializeField, Min(0f), Tooltip("Evre değişince eski parçadan yenisine geçiş süresi (sn, gerçek zaman).")]
        private float _musicCrossfadeDuration = 1.5f;

        [Header("Dinleyici")]
        [SerializeField, Min(0f), Tooltip("Dinleyici oyuncunun ayağının bu kadar üstünde durur (m). Kamera uzakta olduğu için 3D sesler oyuncuya göre duyulur.")]
        private float _listenerHeight = 1.5f;

        public AudioMixerGroup SfxGroup => _sfxGroup;

        public AudioMixerGroup MusicGroup => _musicGroup;

        /// <summary>Grupların mixer'ı (SFX, yoksa Music grubundan); grup yoksa null.</summary>
        public AudioMixer Mixer => _sfxGroup != null ? _sfxGroup.audioMixer : _musicGroup != null ? _musicGroup.audioMixer : null;

        public string MasterVolumeParameter => _masterVolumeParameter;

        public string MusicVolumeParameter => _musicVolumeParameter;

        public string SfxVolumeParameter => _sfxVolumeParameter;

        public float MinVolumeDecibels => _minVolumeDecibels;

        public SoundVoice VoicePrefab => _voicePrefab;

        public int PrewarmVoices => Mathf.Max(0, _prewarmVoices);

        public int MaxVoices => Mathf.Max(1, _maxVoices);

        public float MusicCrossfadeDuration => Mathf.Max(0f, _musicCrossfadeDuration);

        public float ListenerHeight => Mathf.Max(0f, _listenerHeight);
    }
}
