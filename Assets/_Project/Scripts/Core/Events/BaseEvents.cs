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

    /// <summary>Üssün güç durumu değiştiğinde gönderilir (<see cref="IPowerService.IsPowered"/>). Taretler ve üs ışıkları dinler.</summary>
    public readonly struct PowerChangedEvent : IEvent
    {
        public PowerChangedEvent(bool isPowered)
        {
            IsPowered = isPowered;
        }

        public bool IsPowered { get; }
    }
}
