using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Bir patlama olunca gönderilir (Patlayan, Zincirleme Son), alan hasarı uygulandıktan sonra. Kamera sarsıntısı (Feedback) ve
    /// ses dinler. Boss'un yere vurması patlama değildir (kendi sarsıntı isteğini gönderir).
    /// </summary>
    public readonly struct ExplosionEvent : IEvent
    {
        public ExplosionEvent(Vector3 position, float radius, GameObject source)
        {
            Position = position;
            Radius = radius;
            Source = source;
        }

        /// <summary>Patlamanın merkezi (dünya).</summary>
        public Vector3 Position { get; }

        /// <summary>Hasar yarıçapı (m).</summary>
        public float Radius { get; }

        /// <summary>Patlamanın sahibi (patlayan düşman; Zincirleme Son'da oyuncu). Null olabilir.</summary>
        public GameObject Source { get; }
    }

    /// <summary>Bir silah ateşlendiğinde gönderilir: tetik başına bir kez, saçma sayısından bağımsız.</summary>
    public readonly struct WeaponFiredEvent : IEvent
    {
        public WeaponFiredEvent(GameObject source, string weaponId, Vector3 origin, Vector3 direction, int pelletCount)
        {
            Source = source;
            WeaponId = weaponId;
            Origin = origin;
            Direction = direction;
            PelletCount = pelletCount;
        }

        /// <summary>Ateş eden (oyuncu, taret).</summary>
        public GameObject Source { get; }

        public string WeaponId { get; }

        /// <summary>Namlu konumu (görsel başlangıç noktası).</summary>
        public Vector3 Origin { get; }

        /// <summary>Saçılmadan önceki nişan yönü (normalize).</summary>
        public Vector3 Direction { get; }

        public int PelletCount { get; }
    }

    /// <summary>
    /// Hitscan mermisi bir collider'a isabet edince, isabet eden her saçma için gönderilir. Iskalayan saçma olay göndermez.
    /// Hasar (varsa) bu olaydan önce uygulanmıştır.
    /// </summary>
    public readonly struct ShotHitEvent : IEvent
    {
        public ShotHitEvent(
            GameObject source,
            GameObject target,
            HitTargetType targetType,
            Vector3 point,
            Vector3 normal,
            Vector3 direction,
            float damage)
        {
            Source = source;
            Target = target;
            TargetType = targetType;
            Point = point;
            Normal = normal;
            Direction = direction;
            Damage = damage;
        }

        /// <summary>Ateş eden (oyuncu, taret).</summary>
        public GameObject Source { get; }

        /// <summary>Vurulan obje: hasar alabiliyorsa onun kökü, değilse collider'ın objesi.</summary>
        public GameObject Target { get; }

        /// <summary>Vurulanın türü (vuruş efekti ve sesi buna göre seçilir).</summary>
        public HitTargetType TargetType { get; }

        public Vector3 Point { get; }

        public Vector3 Normal { get; }

        /// <summary>Saçmanın yönü (saçılma uygulanmış, normalize).</summary>
        public Vector3 Direction { get; }

        /// <summary>Uygulanan hasar isteği. Hasar alamayan yüzeyde veya ölü hedefte 0.</summary>
        public float Damage { get; }
    }

    /// <summary>
    /// Hitscan atışında her saçma için (isabet veya ıska) gönderilir; mermi izi çizmek için.
    /// İsabette o saçmanın <see cref="ShotHitEvent"/>'inden sonra gelir.
    /// </summary>
    public readonly struct ShotTracedEvent : IEvent
    {
        public ShotTracedEvent(GameObject source, string weaponId, Vector3 start, Vector3 end, Vector3 direction, bool isHit)
        {
            Source = source;
            WeaponId = weaponId;
            Start = start;
            End = end;
            Direction = direction;
            IsHit = isHit;
        }

        /// <summary>Ateş eden (oyuncu, taret).</summary>
        public GameObject Source { get; }

        /// <summary>Ateş eden silahın id'si (iz rengi buna göre); bilinmiyorsa null.</summary>
        public string WeaponId { get; }

        /// <summary>İzin görsel başlangıcı (namlu). Işın daha geriden başlamış olabilir.</summary>
        public Vector3 Start { get; }

        /// <summary>İsabet noktası; ıskaladıysa menzilin sonu.</summary>
        public Vector3 End { get; }

        /// <summary>Saçmanın yönü (saçılma uygulanmış, normalize).</summary>
        public Vector3 Direction { get; }

        public bool IsHit { get; }
    }

    /// <summary>Oyuncunun elindeki silah değişince (oyun başında da bir kez) gönderilir.</summary>
    public readonly struct WeaponChangedEvent : IEvent
    {
        public WeaponChangedEvent(int slotIndex, string weaponId, string displayName)
        {
            SlotIndex = slotIndex;
            WeaponId = weaponId;
            DisplayName = displayName;
        }

        /// <summary>0 tabanlı yuva indeksi (1 tuşu → 0).</summary>
        public int SlotIndex { get; }

        public string WeaponId { get; }

        public string DisplayName { get; }
    }

    /// <summary>Eldeki silahın mermisi veya şarjör kapasitesi değişince gönderilir.</summary>
    public readonly struct AmmoChangedEvent : IEvent
    {
        public AmmoChangedEvent(string weaponId, int ammo, int magSize, int reserve, bool hasInfiniteReserve)
        {
            WeaponId = weaponId;
            Ammo = ammo;
            MagSize = magSize;
            Reserve = reserve;
            HasInfiniteReserve = hasInfiniteReserve;
        }

        public string WeaponId { get; }

        /// <summary>Şarjördeki mermi.</summary>
        public int Ammo { get; }

        /// <summary>Etkili şarjör kapasitesi (yükseltme ve perk'ler dahil).</summary>
        public int MagSize { get; }

        /// <summary>Yedek mermi; <see cref="HasInfiniteReserve"/> true ise anlamsız (0).</summary>
        public int Reserve { get; }

        public bool HasInfiniteReserve { get; }
    }

    /// <summary>Şarjör doldurma başlayınca gönderilir.</summary>
    public readonly struct ReloadStartedEvent : IEvent
    {
        public ReloadStartedEvent(string weaponId, float duration)
        {
            WeaponId = weaponId;
            Duration = duration;
        }

        public string WeaponId { get; }

        /// <summary>Doldurma süresi (sn).</summary>
        public float Duration { get; }
    }

    /// <summary>Şarjör doldurma bitince veya iptal edilince (ör. silah değişti) gönderilir.</summary>
    public readonly struct ReloadFinishedEvent : IEvent
    {
        public ReloadFinishedEvent(string weaponId, bool completed)
        {
            WeaponId = weaponId;
            Completed = completed;
        }

        public string WeaponId { get; }

        /// <summary>true: şarjör doldu. false: doldurma iptal edildi, mermi değişmedi.</summary>
        public bool Completed { get; }
    }
}
