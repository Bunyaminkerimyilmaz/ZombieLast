using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Test haritasının yerleşimi (GDD §8, §17). Sadece sahne üreticisi kullanır; değiştirip Build Game Scene'i yeniden çalıştır.
    /// </summary>
    public sealed class MapLayoutConfig : ScriptableObject
    {
        [Header("Zemin")]
        [SerializeField, Min(10f), Tooltip("Kare zeminin kenar uzunluğu (m). Merkez (0,0,0).")]
        private float _mapSize = 80f;

        [Header("Üs alanı")]
        [SerializeField, Min(1f), Tooltip("Zeminde farklı renkle gösterilen üs dairesinin yarıçapı (m).")]
        private float _baseAreaRadius = 14f;

        [Header("Spawn bölgeleri")]
        [SerializeField, Min(0f), Tooltip("Spawn bölgesi merkezinin harita merkezine uzaklığı (m).")]
        private float _spawnZoneDistance = 33f;

        [SerializeField, Min(0.5f), Tooltip("Spawn bölgesinin yarıçapı (m).")]
        private float _spawnZoneRadius = 6f;

        [Header("Engeller")]
        [SerializeField, Tooltip("Aynı seed her zaman aynı yerleşimi verir.")]
        private int _obstacleSeed = 1337;

        [SerializeField, Min(0), Tooltip("Hedef engel sayısı. Yer kalmazsa daha azı yerleşir (uyarı loglanır).")]
        private int _obstacleCount = 25;

        [SerializeField, Min(0f), Tooltip("Engel halkasının iç yarıçapı (m).")]
        private float _obstacleRingMin = 18f;

        [SerializeField, Min(0f), Tooltip("Engel halkasının dış yarıçapı (m).")]
        private float _obstacleRingMax = 38f;

        [SerializeField, Min(0f), Tooltip("Engeller arası ve spawn bölgelerine en az boşluk (m). Düşmanların geçebileceği kadar olmalı.")]
        private float _obstacleSpacing = 2f;

        [SerializeField, Min(1), Tooltip("Engel başına en fazla deneme sayısı.")]
        private int _obstacleMaxAttempts = 50;

        [SerializeField, Tooltip("Engel şablonları (GDD §17: araba, kaya, duvar parçası).")]
        private ObstacleShape[] _obstacleShapes =
        {
            new ObstacleShape("Car", new Vector3(4f, 1.5f, 2f), 1f, 0f),
            new ObstacleShape("Rock", new Vector3(2f, 1.6f, 2f), 1f, 0.3f),
            new ObstacleShape("WallSegment", new Vector3(5f, 2f, 0.5f), 1f, 0f),
        };

        [Header("Sınır")]
        [SerializeField, Min(0.5f), Tooltip("Görünmez sınır duvarlarının yüksekliği (m).")]
        private float _boundaryWallHeight = 5f;

        [SerializeField, Min(0.1f), Tooltip("Görünmez sınır duvarlarının kalınlığı (m). Zemin kenarının dışına konur.")]
        private float _boundaryWallThickness = 1f;

        public float MapSize => _mapSize;

        public float BaseAreaRadius => _baseAreaRadius;

        public float SpawnZoneDistance => _spawnZoneDistance;

        public float SpawnZoneRadius => _spawnZoneRadius;

        public IReadOnlyList<ObstacleShape> ObstacleShapes => _obstacleShapes;

        public float BoundaryWallHeight => _boundaryWallHeight;

        public float BoundaryWallThickness => _boundaryWallThickness;

        public ObstacleLayoutSettings CreateObstacleSettings()
        {
            return new ObstacleLayoutSettings(
                _obstacleSeed,
                _obstacleCount,
                _obstacleRingMin,
                _obstacleRingMax,
                _obstacleSpacing,
                _obstacleMaxAttempts);
        }

        /// <summary>Spawn bölgesinin merkezi (yer seviyesinde).</summary>
        public Vector3 GetSpawnZoneCenter(CompassDirection direction)
        {
            return direction.ToVector3() * _spawnZoneDistance;
        }

        /// <summary>Engel konamayacak alanlar: üs alanı ve dört spawn bölgesi.</summary>
        public List<ExclusionZone> CreateObstacleExclusions()
        {
            var zones = new List<ExclusionZone>(CompassDirections.All.Count + 1)
            {
                new ExclusionZone(Vector2.zero, _baseAreaRadius),
            };

            for (int i = 0; i < CompassDirections.All.Count; i++)
            {
                Vector3 center = GetSpawnZoneCenter(CompassDirections.All[i]);
                zones.Add(new ExclusionZone(new Vector2(center.x, center.z), _spawnZoneRadius));
            }

            return zones;
        }
    }
}
