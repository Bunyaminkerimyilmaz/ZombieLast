using UnityEngine;

namespace LastHaven.Combat
{
    /// <summary>Tek bir tetik çekişinin hitscan parametreleri.</summary>
    public readonly struct HitscanRequest
    {
        /// <param name="direction">Saçılmadan önceki yön; normalize edilir.</param>
        /// <param name="spreadAngle">Saçılma konisinin toplam açısı (derece); her saçma ±yarısı kadar sapar.</param>
        /// <param name="pelletCount">Saçma sayısı; en az 1.</param>
        /// <param name="damage">Saçma başına hasar.</param>
        /// <param name="source">Ateş eden; DamageInfo ve olaylara kaynak olarak yazılır.</param>
        /// <param name="weaponId">Olaylara yazılan silah id'si (ör. iz rengi için); opsiyonel.</param>
        /// <param name="tracerOrigin">Mermi izinin görsel başlangıcı (namlu); verilmezse <paramref name="origin"/>.</param>
        public HitscanRequest(
            Vector3 origin,
            Vector3 direction,
            float spreadAngle,
            int pelletCount,
            float range,
            LayerMask hitMask,
            float damage,
            GameObject source,
            string weaponId = null,
            Vector3? tracerOrigin = null)
        {
            Origin = origin;
            Direction = direction.normalized;
            SpreadAngle = spreadAngle;
            PelletCount = Mathf.Max(1, pelletCount);
            Range = range;
            HitMask = hitMask;
            Damage = damage;
            Source = source;
            WeaponId = weaponId;
            TracerOrigin = tracerOrigin ?? origin;
        }

        /// <summary>Işının başladığı nokta.</summary>
        public Vector3 Origin { get; }

        public Vector3 Direction { get; }

        public float SpreadAngle { get; }

        public int PelletCount { get; }

        public float Range { get; }

        public LayerMask HitMask { get; }

        public float Damage { get; }

        public GameObject Source { get; }

        /// <summary>Null olabilir.</summary>
        public string WeaponId { get; }

        /// <summary>Mermi izinin görsel başlangıcı (<see cref="LastHaven.Core.ShotTracedEvent.Start"/>).</summary>
        public Vector3 TracerOrigin { get; }
    }
}
