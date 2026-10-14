using System;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>Harita engeli şablonu: kutu boyutu, seçilme ağırlığı ve boyut sapması.</summary>
    [Serializable]
    public struct ObstacleShape
    {
        [SerializeField, Tooltip("Sahnedeki obje adında kullanılır (ör. Car).")]
        private string _name;

        [SerializeField, Tooltip("Kutu boyutu (m): X uzunluk, Y yükseklik, Z genişlik.")]
        private Vector3 _size;

        [SerializeField, Min(0f), Tooltip("Seçilme ağırlığı. 0 ise bu şekil hiç kullanılmaz.")]
        private float _weight;

        [SerializeField, Range(0f, 0.5f), Tooltip("Boyutun rastgele ± oranı (0.2 = %80–%120).")]
        private float _sizeJitter;

        public ObstacleShape(string name, Vector3 size, float weight, float sizeJitter)
        {
            _name = name;
            _size = size;
            _weight = weight;
            _sizeJitter = sizeJitter;
        }

        public string Name => _name;

        public Vector3 Size => _size;

        public float Weight => _weight;

        public float SizeJitter => _sizeJitter;
    }
}
