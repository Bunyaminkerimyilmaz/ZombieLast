using System;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary><see cref="ImpactEffect"/>'in bir isabet türü için parçacık ayarları.</summary>
    [Serializable]
    public struct ImpactProfile
    {
        [SerializeField, Tooltip("Parçacık rengi bu iki renk arasında rastgele.")]
        private Color _colorA;

        [SerializeField]
        private Color _colorB;

        [SerializeField, Min(0), Tooltip("İsabet başına parçacık sayısı.")]
        private int _count;

        [SerializeField, Min(0f), Tooltip("Başlangıç hızı aralığı (m/sn).")]
        private float _minSpeed;

        [SerializeField, Min(0f)]
        private float _maxSpeed;

        [SerializeField, Min(0f), Tooltip("Başlangıç boyutu aralığı (m).")]
        private float _minSize;

        [SerializeField, Min(0f)]
        private float _maxSize;

        [SerializeField, Tooltip("Yerçekimi çarpanı.")]
        private float _gravity;

        [SerializeField, Tooltip("Parçacık materyali. Boşsa renderer'daki materyal kalır.")]
        private Material _material;

        public ImpactProfile(Color colorA, Color colorB, int count, float minSpeed, float maxSpeed, float minSize, float maxSize, float gravity)
        {
            _colorA = colorA;
            _colorB = colorB;
            _count = count;
            _minSpeed = minSpeed;
            _maxSpeed = maxSpeed;
            _minSize = minSize;
            _maxSize = maxSize;
            _gravity = gravity;
            _material = null;
        }

        public Color ColorA => _colorA;

        public Color ColorB => _colorB;

        public int Count => _count;

        public float MinSpeed => _minSpeed;

        public float MaxSpeed => _maxSpeed;

        public float MinSize => _minSize;

        public float MaxSize => _maxSize;

        public float Gravity => _gravity;

        /// <summary>Null olabilir.</summary>
        public Material Material => _material;
    }
}
