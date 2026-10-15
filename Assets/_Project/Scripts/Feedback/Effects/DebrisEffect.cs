using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary>
    /// Yapı yıkılma efekti: yapının kutusundan yukarı ve yana saçılan, dönerek düşen moloz parçaları (GDD §11 "kısa yıkılma
    /// efekti"). ParticleSystem'in ortak ayarları (mesh, renk, ömür, yerçekimi) prefab'da (üretici kurar); kutu boyutu ve parça
    /// sayısı her yıkılmada <see cref="Play"/> ile ayarlanır. <see cref="PooledLifetime"/> dolunca havuza döner.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ParticleSystem), typeof(PooledLifetime))]
    public sealed class DebrisEffect : MonoBehaviour, IPoolable
    {
        [SerializeField, Min(0f), Tooltip("Yapının taban alanının m²'si başına parça (3×1 barikat → 3 × bu).")]
        private float _particlesPerSquareMeter = 8f;

        [SerializeField, Min(1), Tooltip("En az parça sayısı.")]
        private int _minParticles = 10;

        [SerializeField, Min(1), Tooltip("En fazla parça sayısı.")]
        private int _maxParticles = 40;

        private ParticleSystem _particles;

        /// <summary>Taban alanına göre parça sayısı: alan × yoğunluk, en yakın tam sayıya yuvarlanıp [min, max] aralığına kırpılır.</summary>
        public static int GetParticleCount(Vector3 size, float perSquareMeter, int min, int max)
        {
            float area = Mathf.Abs(size.x * size.z);
            int count = Mathf.RoundToInt(area * Mathf.Max(0f, perSquareMeter));
            return Mathf.Clamp(count, min, Mathf.Max(min, max));
        }

        /// <summary>Parçaları yapının kutusunun içinden bir kerede saçar. Obje kutunun merkezine konmuş olmalı.</summary>
        public void Play(Bounds bounds)
        {
            // Şekil prefab'da X'te -90° döndürülmüş kutu (+Z yukarı saçar): yerel (x, y, z) = dünya (x, z, y).
            ParticleSystem.ShapeModule shape = _particles.shape;
            Vector3 size = bounds.size;
            shape.scale = new Vector3(size.x, size.z, size.y);

            _particles.Play();
            _particles.Emit(GetParticleCount(size, _particlesPerSquareMeter, _minParticles, _maxParticles));
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
        }
    }
}
