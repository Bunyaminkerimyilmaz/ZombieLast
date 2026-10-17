using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Tüm taretlerin ortak davranış ayarları (GDD §11). Hasar, atış hızı ve menzil seviyede (<see cref="BuildableLevel"/>
    /// statları), burada değil. Asset'i üretici oluşturur (<c>Data/Building/TurretConfig.asset</c>); taret prefab'ındaki
    /// <see cref="Turret"/> bağlıdır.
    /// </summary>
    public sealed class TurretConfig : ScriptableObject
    {
        [Header("Hedefleme")]
        [SerializeField, Min(0.02f), Tooltip("Menzildeki düşmanların taranma aralığı (sn). Her taramada en yakın canlı düşman seçilir.")]
        private float _scanInterval = 0.2f;

        [SerializeField, Tooltip("Taranan layer'lar (düşmanlar).")]
        private LayerMask _targetLayers = GameLayers.EnemyMask;

        [SerializeField, Min(1), Tooltip("Bir taramada okunan en fazla collider. Kalabalıkta fazlası görülmez (en yakın atlanabilir).")]
        private int _scanBufferSize = 64;

        [Header("Nişan")]
        [SerializeField, Min(0f), Tooltip("Kafanın dönüş hızı (derece/sn). Sadece Y ekseninde döner.")]
        private float _turnSpeed = 360f;

        [SerializeField, Range(0f, 45f), Tooltip("Kafa hedefe bu açıdan (derece) yakınsa ateş eder.")]
        private float _aimTolerance = 5f;

        [Header("Atış")]
        [SerializeField, Tooltip("Mermi ışınının çarptığı layer'lar. Yapılar yok: taret barikatın üstünden ateş eder.")]
        private LayerMask _hitMask = GameLayers.GroundMask | GameLayers.ObstacleMask | GameLayers.EnemyMask;

        [SerializeField, Tooltip("Olaylara yazılan silah id'si (mermi izi rengi; bilinmeyen id varsayılan renk alır).")]
        private string _weaponId = "turret";

        public float ScanInterval => _scanInterval;

        public LayerMask TargetLayers => _targetLayers;

        public int ScanBufferSize => Mathf.Max(1, _scanBufferSize);

        public float TurnSpeed => _turnSpeed;

        public float AimTolerance => _aimTolerance;

        public LayerMask HitMask => _hitMask;

        public string WeaponId => _weaponId;
    }
}
