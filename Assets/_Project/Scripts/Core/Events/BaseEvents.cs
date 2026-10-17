using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Jeneratör yıkıldığında bir kez gönderilir. Oyun akışı bunu yenilgi (GDD §15) olarak dinler.</summary>
    public readonly struct GeneratorDestroyedEvent : IEvent
    {
        public GeneratorDestroyedEvent(GameObject generator, Vector3 position, DamageInfo killingBlow)
        {
            Generator = generator;
            Position = position;
            KillingBlow = killingBlow;
        }

        public GameObject Generator { get; }

        /// <summary>Jeneratörün konumu (yer seviyesi, dünya).</summary>
        public Vector3 Position { get; }

        public DamageInfo KillingBlow { get; }
    }

    /// <summary>Üssün güç durumu değiştiğinde gönderilir (<see cref="IPowerService.IsPowered"/>). Taretler, üs ışıkları ve HUD dinler.</summary>
    public readonly struct PowerChangedEvent : IEvent
    {
        public PowerChangedEvent(bool isPowered)
        {
            IsPowered = isPowered;
        }

        public bool IsPowered { get; }
    }

    /// <summary>Gücün neden kesildiği.</summary>
    public enum PowerLossReason
    {
        /// <summary>Yakıt bitti (GDD §12).</summary>
        OutOfFuel = 0,

        /// <summary>Jeneratör yıkıldı.</summary>
        GeneratorDestroyed = 1,
    }

    /// <summary>
    /// Güç kesildiğinde bir kez gönderilir, aynı değişimin <see cref="PowerChangedEvent"/>'inden (false) sonra. Ses ve uyarılar
    /// (Gün 14) dinler. Geri gelişi <see cref="PowerChangedEvent"/> (true) duyurur.
    /// </summary>
    public readonly struct PowerLostEvent : IEvent
    {
        public PowerLostEvent(Vector3 position, PowerLossReason reason)
        {
            Position = position;
            Reason = reason;
        }

        /// <summary>Jeneratörün konumu (yer seviyesi, dünya).</summary>
        public Vector3 Position { get; }

        public PowerLossReason Reason { get; }
    }
}
