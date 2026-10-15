using System;
using System.Collections.Generic;

namespace LastHaven.Building
{
    /// <summary>
    /// Yapının seviye ve harcama durumu (saf C#, GDD §11): 0 tabanlı seviye ve toplam harcanan para (inşa + yükseltmeler).
    /// Satış iadesi toplam harcamadan hesaplanır (GDD §10).
    /// </summary>
    public sealed class StructureProgress
    {
        private IReadOnlyList<BuildableLevel> _levels = Array.Empty<BuildableLevel>();

        /// <summary>0 tabanlı seviye (0 = inşa edilen seviye).</summary>
        public int LevelIndex { get; private set; }

        public int LevelCount => _levels.Count;

        /// <summary>İnşa fiyatı + ödenen yükseltme fiyatları.</summary>
        public int TotalSpent { get; private set; }

        /// <summary>Şu anki seviye; seviye listesi boşsa null.</summary>
        public BuildableLevel Current => GetLevel(LevelIndex);

        /// <summary>Sonraki seviye; en üst seviyede null.</summary>
        public BuildableLevel Next => GetLevel(LevelIndex + 1);

        public bool CanUpgrade => Next != null;

        /// <summary>Sonraki seviyenin fiyatı; en üst seviyede 0.</summary>
        public int UpgradeCost => Next != null ? Next.Cost : 0;

        /// <summary>Yapı yerleşti: 1. seviye, harcama = inşa fiyatı (negatifse 0).</summary>
        public void Reset(IReadOnlyList<BuildableLevel> levels, int buildCost)
        {
            _levels = levels ?? throw new ArgumentNullException(nameof(levels));
            LevelIndex = 0;
            TotalSpent = Math.Max(0, buildCost);
        }

        /// <summary>Sonraki seviyeye geçer ve fiyatını harcamaya ekler. En üst seviyedeyse hiçbir şey yapmaz ve false döner.</summary>
        public bool Advance()
        {
            BuildableLevel next = Next;
            if (next == null)
            {
                return false;
            }

            LevelIndex++;
            TotalSpent += next.Cost;
            return true;
        }

        private BuildableLevel GetLevel(int index)
        {
            return index >= 0 && index < _levels.Count ? _levels[index] : null;
        }
    }
}
