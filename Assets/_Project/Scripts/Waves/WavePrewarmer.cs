using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// [Systems]/WaveSpawner: oyun başında düşman havuzlarını gecelerin ihtiyacına göre ısıtır (<see cref="WavePrewarm"/>: tip başına
    /// bir gecedeki en kalabalık adet, canlı tavanıyla sınırlı, + gündüz gezginleri). <c>EnemySpawnerConfig</c>'teki sabit sayılar
    /// (ör. Yürüyen 40) 10. gecenin 90 Yürüyen'ini karşılamıyordu; oynanışta havuz büyümesi GC ayırır ve kareyi takar.
    /// <see cref="IPoolService.Prewarm"/> eksik olanı tamamlar; spawner'ın kendi ön ısıtmasıyla çakışmaz.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WavePrewarmer : MonoBehaviour
    {
        [SerializeField, Tooltip("Gece listesi ve canlı tavanı (WaveSpawner ile aynı). Sahne adımı bağlar.")]
        private WaveConfig _waves;

        [SerializeField, Tooltip("Gündüz gezginleri (opsiyonel): geceye katılanlar o tipin ihtiyacına eklenir. Sahne adımı bağlar.")]
        private DayWanderersConfig _wanderers;

        private void Start()
        {
            if (_waves == null || !ServiceLocator.TryGet(out IPoolService pool))
            {
                Debug.LogWarning("WavePrewarmer: WaveConfig veya IPoolService yok; düşman havuzları gecelere göre ısıtılmadı.", this);
                return;
            }

            var needs = new Dictionary<EnemyData, int>();
            var scratch = new Dictionary<EnemyData, int>();
            var groups = new List<WaveGroup>();
            for (int i = 0; i < _waves.Nights.Count; i++)
            {
                if (_waves.Nights[i] == null)
                {
                    continue;
                }

                groups.Clear();
                WavePlan.BuildGroups(_waves.Nights[i], groups);
                WavePrewarm.AddNight(groups, _waves.MaxAliveEnemies, needs, scratch);
            }

            if (_wanderers != null)
            {
                WavePrewarm.AddWanderers(_wanderers.Enemy, _wanderers.MaxCount, needs);
            }

            int total = 0;
            foreach (KeyValuePair<EnemyData, int> need in needs)
            {
                if (need.Key.Prefab != null)
                {
                    pool.Prewarm(need.Key.Prefab.gameObject, need.Value);
                    total += need.Value;
                }
            }

            Debug.Log($"[Waves] Düşman havuzları gecelere göre ısıtıldı ({needs.Count} tip, en çok {total} örnek).", this);
        }
    }
}
