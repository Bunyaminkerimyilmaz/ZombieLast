using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.UI;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>HudPhaseRules (6.3): zamanlayıcı sadece gündüz, kalan düşman sadece gece, her evrenin kendi ad etiketi.</summary>
    public class HudPhaseRulesTests
    {
        [Test]
        public void DayTimer_OnlyInDay()
        {
            foreach (GamePhase phase in Enum.GetValues(typeof(GamePhase)))
            {
                Assert.AreEqual(phase == GamePhase.Day, HudPhaseRules.ShowsDayTimer(phase), phase.ToString());
            }
        }

        [Test]
        public void RemainingEnemies_OnlyInNight()
        {
            foreach (GamePhase phase in Enum.GetValues(typeof(GamePhase)))
            {
                Assert.AreEqual(phase == GamePhase.Night, HudPhaseRules.ShowsRemainingEnemies(phase), phase.ToString());
            }
        }

        [Test]
        public void PhaseLabelName_BootHasNone_OthersDistinctAndListed()
        {
            Assert.IsNull(HudPhaseRules.PhaseLabelName(GamePhase.Boot));

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (GamePhase phase in Enum.GetValues(typeof(GamePhase)))
            {
                if (phase == GamePhase.Boot)
                {
                    continue;
                }

                string label = HudPhaseRules.PhaseLabelName(phase);
                Assert.IsNotNull(label, phase.ToString());
                Assert.IsTrue(seen.Add(label), $"{phase}: etiket tekrar ediyor ({label}).");
                CollectionAssert.Contains(HudElementNames.PhaseLabels, label, $"{phase}: PhaseLabels listesinde yok.");
            }

            Assert.AreEqual(HudElementNames.PhaseLabels.Length, seen.Count, "Her etiket bir evreye ait olmalı.");
        }
    }
}
