using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Bir ses efektinin ya da müzik parçasının verisi (GDD §18): klip listesi (her çalışta rastgele biri), ses düzeyi, perde aralığı,
    /// 2D/3D, 3D mesafeleri ve tekrar sınırları. Klip yoksa ses sessizce atlanır (hata yok). Oyun olaylarına <c>AudioEventMap</c>
    /// bağlar; çalan <see cref="IAudioService"/>. Asset'leri üretici boş klip listesiyle oluşturur: <c>Data/Audio/</c>.
    /// </summary>
    public sealed class SoundData : ScriptableObject
    {
        /// <summary>Perdenin alt sınırı (0'a yakın perde süreyi sonsuza uzatır).</summary>
        public const float MinPitch = 0.1f;

        [Header("Klipler")]
        [SerializeField, Tooltip("Çalınacak klipler; her çalışta rastgele biri seçilir. Boş (veya hepsi boş eleman) ise ses sessizce atlanır.")]
        private List<AudioClip> _clips = new List<AudioClip>();

        [Header("Ses")]
        [SerializeField, Range(0f, 1f), Tooltip("Ses düzeyi (mixer grubundan önce).")]
        private float _volume = 1f;

        [SerializeField, Range(MinPitch, 3f), Tooltip("Perde aralığının alt ucu; her çalışta aralıktan rastgele. 1 = değişmez.")]
        private float _minPitch = 1f;

        [SerializeField, Range(MinPitch, 3f), Tooltip("Perde aralığının üst ucu.")]
        private float _maxPitch = 1f;

        [Header("Uzay")]
        [SerializeField, Tooltip("2D: her yerden aynı. 3D: olayın konumundan uzaklıkla zayıflar.")]
        private SoundSpatial _spatial = SoundSpatial.ThreeD;

        [SerializeField, Min(0.01f), Tooltip("3D: bu mesafeye kadar tam ses (m).")]
        private float _minDistance = 2f;

        [SerializeField, Min(0.01f), Tooltip("3D: bu mesafede ve ötesinde ses kısılmış (m; azalma eğrisi ses kaynağı prefab'ında).")]
        private float _maxDistance = 40f;

        [Header("Sınırlar")]
        [SerializeField, Min(0), Tooltip("Bu ses aynı anda en fazla kaç kez çalar; doluyken yeni istek atlanır. 0 = sınırsız.")]
        private int _maxSimultaneous = 4;

        [SerializeField, Min(0f), Tooltip("İki çalış arasındaki en kısa süre (sn, gerçek zaman); daha erken istek atlanır. 0 = yok.")]
        private float _cooldown;

        public IReadOnlyList<AudioClip> Clips => _clips;

        public float Volume => Mathf.Clamp01(_volume);

        public float MinPitchValue => Mathf.Max(MinPitch, Mathf.Min(_minPitch, _maxPitch));

        public float MaxPitchValue => Mathf.Max(MinPitch, Mathf.Max(_minPitch, _maxPitch));

        public SoundSpatial Spatial => _spatial;

        public bool IsSpatial => _spatial == SoundSpatial.ThreeD;

        public float MinDistance => Mathf.Max(0.01f, _minDistance);

        /// <summary>En az <see cref="MinDistance"/>.</summary>
        public float MaxDistance => Mathf.Max(MinDistance, _maxDistance);

        /// <summary>0 = sınırsız.</summary>
        public int MaxSimultaneous => Mathf.Max(0, _maxSimultaneous);

        public float Cooldown => Mathf.Max(0f, _cooldown);

        /// <summary>Çalınabilir (boş olmayan) en az bir klip var mı.</summary>
        public bool HasClips => CountClips() > 0;

        /// <summary>Boş olmayan klip sayısı.</summary>
        public int CountClips()
        {
            int count = 0;
            for (int i = 0; i < _clips.Count; i++)
            {
                if (_clips[i] != null)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Boş olmayan klipler arasından <paramref name="random01"/>'e (0..1) göre birini seçer; boş elemanlar atlanır. Klip yoksa null.
        /// </summary>
        public AudioClip PickClip(float random01)
        {
            int count = CountClips();
            if (count == 0)
            {
                return null;
            }

            int target = Mathf.Clamp((int)(Mathf.Clamp01(random01) * count), 0, count - 1);
            for (int i = 0; i < _clips.Count; i++)
            {
                if (_clips[i] == null)
                {
                    continue;
                }

                if (target == 0)
                {
                    return _clips[i];
                }

                target--;
            }

            return null;
        }

        /// <summary>Perde aralığından <paramref name="random01"/>'e (0..1) göre bir perde.</summary>
        public float PickPitch(float random01)
        {
            return Mathf.Lerp(MinPitchValue, MaxPitchValue, Mathf.Clamp01(random01));
        }

        /// <summary>Klibin bu perdeyle çalma süresi (sn, gerçek zaman). Klip yoksa 0.</summary>
        public static float GetPlayDuration(AudioClip clip, float pitch)
        {
            return clip != null ? GetPlayDuration(clip.length, pitch) : 0f;
        }

        /// <summary>Uzunluğu <paramref name="clipLength"/> olan klibin bu perdeyle çalma süresi (perde ≥ <see cref="MinPitch"/>).</summary>
        public static float GetPlayDuration(float clipLength, float pitch)
        {
            return Mathf.Max(0f, clipLength) / Mathf.Max(MinPitch, Mathf.Abs(pitch));
        }
    }
}
