using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary>
    /// Vuruş efekti: küçük bir parçacık patlaması. Düşmanda kırmızı (et), yüzeyde ve yapıda kıvılcım.
    /// ParticleSystem'in ortak ayarları prefab'da (üretici kurar); türe göre değişenler <see cref="ImpactProfile"/>'larda.
    /// Obje isabet normaline bakar (+Z); <see cref="PooledLifetime"/> dolunca havuza döner.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ParticleSystem), typeof(PooledLifetime))]
    public sealed class ImpactEffect : MonoBehaviour, IPoolable
    {
        [SerializeField, Tooltip("Düşman (ve oyuncu) isabeti.")]
        private ImpactProfile _flesh = new ImpactProfile(
            new Color(0.85f, 0.08f, 0.08f), new Color(0.45f, 0.02f, 0.02f), 12, 1.5f, 4.5f, 0.08f, 0.18f, 2f);

        [SerializeField, Tooltip("Yüzey ve yapı isabeti.")]
        private ImpactProfile _spark = new ImpactProfile(
            new Color(1f, 0.92f, 0.6f), new Color(1f, 0.55f, 0.15f), 10, 3f, 8f, 0.05f, 0.12f, 1.5f);

        private ParticleSystem _particles;
        private ParticleSystemRenderer _renderer;

        /// <summary>Et (kırmızı) efekti alan isabet türleri: düşman ve oyuncu. Diğerleri kıvılcım.</summary>
        public static bool IsFlesh(HitTargetType targetType)
        {
            return targetType == HitTargetType.Enemy || targetType == HitTargetType.Player;
        }

        /// <summary>Türe göre profili uygular ve parçacıkları bir kerede saçar.</summary>
        public void Play(HitTargetType targetType)
        {
            ImpactProfile profile = IsFlesh(targetType) ? _flesh : _spark;

            ParticleSystem.MainModule main = _particles.main;
            main.startColor = new ParticleSystem.MinMaxGradient(profile.ColorA, profile.ColorB);
            main.startSpeed = new ParticleSystem.MinMaxCurve(profile.MinSpeed, profile.MaxSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(profile.MinSize, profile.MaxSize);
            main.gravityModifier = profile.Gravity;
            if (profile.Material != null)
            {
                _renderer.sharedMaterial = profile.Material;
            }

            _particles.Play();
            _particles.Emit(profile.Count);
        }

        public void OnSpawned()
        {
            _particles.Clear();
        }

        public void OnDespawned()
        {
            _particles.Clear();
        }

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
            _renderer = GetComponent<ParticleSystemRenderer>();
        }
    }
}
