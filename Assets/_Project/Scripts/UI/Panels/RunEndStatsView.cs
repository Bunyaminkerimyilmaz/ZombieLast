using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Oyun Bitti / Zafer panelinin istatistik bölümü (GDD §15): ulaşılan gece, toplam öldürme, kazanılan para (run boyunca toplanan
    /// jetonlar), en yüksek kombo ve alınan perk'ler (ilk alım sırasıyla; yoksa "Perk alınmadı"). Servis yoksa değerler 0 / boş.
    /// </summary>
    public sealed class RunEndStatsView
    {
        private readonly Label _night;
        private readonly Label _kills;
        private readonly Label _money;
        private readonly Label _bestCombo;
        private readonly VisualElement _perks;
        private readonly VisualElement _noPerks;

        public RunEndStatsView(Label night, Label kills, Label money, Label bestCombo, VisualElement perks, VisualElement noPerks)
        {
            _night = night;
            _kills = kills;
            _money = money;
            _bestCombo = bestCombo;
            _perks = perks;
            _noPerks = noPerks;
        }

        /// <summary>Kurulan perk sayısı (test/debug).</summary>
        public int PerkCount => _perks.childCount;

        /// <param name="nightNumber">1 tabanlı ulaşılan / atlatılan gece.</param>
        public void Show(int nightNumber, int totalNights, IRunStats stats, IPerkService perks)
        {
            _night.text = HudFormat.OfTotal(nightNumber, totalNights);
            _kills.text = HudFormat.Count(stats != null ? stats.TotalKills : 0);
            _money.text = HudFormat.Count(stats != null ? stats.TotalMoneyEarned : 0);
            _bestCombo.text = HudFormat.Count(stats != null ? stats.BestCombo : 0);

            _perks.Clear();
            IReadOnlyList<string> acquired = perks?.AcquiredPerkIds;
            for (int i = 0; acquired != null && i < acquired.Count; i++)
            {
                IPerkInfo perk = perks.GetPerk(acquired[i]);
                if (perk != null)
                {
                    _perks.Add(RunEndPerkChip.Create(perk, perks.GetStacks(perk.Id)));
                }
            }

            _perks.SetShown(_perks.childCount > 0);
            _noPerks.SetShown(_perks.childCount == 0);
        }
    }
}
