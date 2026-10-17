using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Taretin hedef taraması (GDD §11 "menzildeki en yakın düşman"): <c>Physics.OverlapSphereNonAlloc</c> ile hedef layer'larını
    /// tarar, collider'ları <see cref="DamageableLookup"/> ile hasar alabilen nesneye çevirir ve en yakın canlıyı seçer. Mesafe
    /// yatay düzlemde, nesnenin kökünden ölçülür. Tampon bir kez ayrılır; tarama bellek ayırmaz. Trigger'lar yok sayılır.
    /// </summary>
    public sealed class TurretTargetScanner
    {
        private readonly Collider[] _buffer;
        private readonly LayerMask _layers;

        public TurretTargetScanner(int bufferSize, LayerMask layers)
        {
            _buffer = new Collider[Mathf.Max(1, bufferSize)];
            _layers = layers;
        }

        /// <summary>Menzildeki en yakın canlı hedef; yoksa false.</summary>
        /// <param name="center">Tarama merkezi (taretin ekseni, namlu yüksekliğinde).</param>
        public bool TryFindNearest(Vector3 center, float range, out TurretTarget target)
        {
            target = default;
            if (range <= 0f)
            {
                return false;
            }

            int count = Physics.OverlapSphereNonAlloc(center, range, _buffer, _layers, QueryTriggerInteraction.Ignore);
            float bestSqr = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                IDamageable damageable = DamageableLookup.Find(_buffer[i]);
                if (damageable is not Component component || !damageable.IsAlive || !component.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector3 offset = component.transform.position - center;
                float sqr = offset.x * offset.x + offset.z * offset.z;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    target = new TurretTarget(damageable, component.transform);
                }
            }

            // Bir sonraki taramaya kadar ölü collider referansı tutulmasın.
            System.Array.Clear(_buffer, 0, count);
            return target.IsValid;
        }
    }

    /// <summary>Taretin hedefi: hasar alabilen nesne ve kök transform'u.</summary>
    public readonly struct TurretTarget
    {
        public TurretTarget(IDamageable damageable, Transform transform)
        {
            Damageable = damageable;
            Transform = transform;
        }

        public IDamageable Damageable { get; }

        public Transform Transform { get; }

        /// <summary>Hedef var, canlı ve sahnede etkin mi (havuza dönen düşman geçersiz).</summary>
        public bool IsValid => Transform != null && Damageable != null && Damageable.IsAlive && Transform.gameObject.activeInHierarchy;
    }
}
