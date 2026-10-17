using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Varlık prefab'larının iki katmanlı yapısı (CLAUDE.md "Görsel ayrımı"): kökte mantık, kökün doğrudan çocuğu
    /// <see cref="VisualRoot.Name"/>'da mesh. Validate Project ve EditMode testleri aynı kuralları kullanır.
    /// </summary>
    public static class PrefabVisualValidator
    {
        /// <summary>Hata: kökün doğrudan çocuğu Visual yok. Uyarı: Visual'ın altında hiç Renderer yok (varlık görünmez).</summary>
        public static void ValidatePrefab(GameObject prefab, ValidationReport report)
        {
            Transform visual = VisualRoot.Find(prefab.transform);
            if (visual == null)
            {
                report.Error($"{prefab.name}: kökün doğrudan çocuğu '{VisualRoot.Name}' yok (mesh ve animasyon bu objenin altında olmalı).", prefab);
                return;
            }

            if (visual.GetComponentInChildren<Renderer>(true) == null)
            {
                report.Warning($"{prefab.name}: '{VisualRoot.Name}' altında Renderer yok; varlık oyunda görünmez.", prefab);
            }
        }
    }
}
