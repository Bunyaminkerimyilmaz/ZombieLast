using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>Zincirleme patlamaya katılan (canlı, patlamamış) patlayıcı.</summary>
    public interface IChainExplosive
    {
        /// <summary>Zemindeki konumu.</summary>
        Vector3 Position { get; }

        /// <summary>Gecikmeli patlamayı kurar; kurulduysa true (zaten tetiklendiyse veya patladıysa false).</summary>
        bool TryPrime(float delay);
    }

    /// <summary>
    /// Zincirleme patlama (GDD §7 Patlayan: "yakındakileri zincirleme tetikler"): patlayan, yatay yarıçapındaki üyeleri gecikmeyle
    /// tetikler; onlar patlayınca kendi çevrelerini tetikler. Üyeler doğunca eklenir, patlayınca/havuza dönünce çıkar.
    /// Saf; oyunda <see cref="Shared"/> kullanılır, testler kendi örneğini kurar.
    /// </summary>
    public sealed class ExplosionChain
    {
        private readonly List<IChainExplosive> _members = new List<IChainExplosive>();

        /// <summary>Oyundaki ortak zincir. Play Mode girişinde sıfırlanır.</summary>
        public static ExplosionChain Shared { get; private set; } = new ExplosionChain();

        public int Count => _members.Count;

        public void Add(IChainExplosive member)
        {
            if (member != null && !_members.Contains(member))
            {
                _members.Add(member);
            }
        }

        public void Remove(IChainExplosive member)
        {
            _members.Remove(member);
        }

        public void Clear()
        {
            _members.Clear();
        }

        /// <summary>
        /// <paramref name="center"/>'a yatay uzaklığı <paramref name="radius"/> içinde (dahil) olan üyeleri <paramref name="delay"/> sn
        /// gecikmeyle tetikler. Yeni tetiklenen üye sayısını döndürür.
        /// </summary>
        public int PrimeAround(Vector3 center, float radius, float delay)
        {
            int primed = 0;
            for (int i = 0; i < _members.Count; i++)
            {
                IChainExplosive member = _members[i];
                if (PlanarDistance.Between(center, member.Position) <= radius && member.TryPrime(delay))
                {
                    primed++;
                }
            }

            return primed;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Shared = new ExplosionChain();
        }
    }
}
