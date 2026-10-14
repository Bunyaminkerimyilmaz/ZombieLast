using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Tek bir hasar uygulamasının bilgisi.</summary>
    public readonly struct DamageInfo
    {
        /// <summary>Uygulanmak istenen hasar miktarı.</summary>
        public float Amount { get; }

        public DamageType DamageType { get; }

        /// <summary>Hasarı veren nesne (oyuncu, düşman, taret...). Bilinmiyorsa null olabilir.</summary>
        public GameObject Source { get; }

        /// <summary>Vuruşun dünya konumu.</summary>
        public Vector3 HitPoint { get; }

        /// <summary>Hasarın geliş yönü (geri itme için). Normalize olması beklenir.</summary>
        public Vector3 HitDirection { get; }

        public DamageInfo(float amount, DamageType damageType, GameObject source, Vector3 hitPoint, Vector3 hitDirection)
        {
            Amount = amount;
            DamageType = damageType;
            Source = source;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
        }
    }
}
