using System;
using System.Collections.Generic;
using LastHaven.Enemies;

namespace LastHaven.Editor
{
    /// <summary>
    /// Düşman verisi ↔ prefab ↔ spawner bağlantıları (GDD §7). Validate Project ve EditMode testleri aynı kuralları kullanır.
    /// Boş <c>EnemyData._prefab</c> ve boş config satırı "Zorunlu referanslar" bölümünde raporlanır; burada atlanır.
    /// </summary>
    public static class EnemyLinksValidator
    {
        /// <summary>
        /// Hatalar: id boş veya iki veride aynı; prefab'ın <see cref="Enemy"/> verisi boş ya da başka veri (havuzdan çıkan düşman
        /// prefab'ın verisiyle kurulur); config'te aynı id iki kez (spawner ikincisini atlar); gecede/gezginde kullanılan tip
        /// config'te yok (doğamaz). Uyarı: kullanılmayan tip config'te yok.
        /// </summary>
        /// <param name="enemies">Projedeki tüm düşman verileri.</param>
        /// <param name="config">Spawner config'i; null ise hata.</param>
        /// <param name="usedBy">Gece/gezgin verisinde kullanılan tipler → kullanan asset adları.</param>
        public static void Validate(IReadOnlyList<EnemyData> enemies, EnemySpawnerConfig config,
            IReadOnlyDictionary<EnemyData, string> usedBy, ValidationReport report)
        {
            ValidateIds(enemies, report);
            foreach (EnemyData data in enemies)
            {
                ValidatePrefab(data, report);
            }

            if (config == null)
            {
                report.Error($"{EnemiesGenerator.ConfigPath} yok; hiçbir düşman doğurulamaz. Tools/Last Haven/Generate Default Data çalıştır.");
                return;
            }

            var registered = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < config.Enemies.Count; i++)
            {
                EnemyData data = config.Enemies[i].Data;
                if (data != null && !string.IsNullOrEmpty(data.Id) && !registered.Add(data.Id))
                {
                    report.Error($"{config.name}: '{data.Id}' id'si birden fazla satırda ([{i}]); spawner sonrakini atlar.", config);
                }
            }

            foreach (EnemyData data in enemies)
            {
                if (string.IsNullOrEmpty(data.Id) || registered.Contains(data.Id))
                {
                    continue;
                }

                if (usedBy != null && usedBy.TryGetValue(data, out string users))
                {
                    report.Error($"{data.name}: {users} kullanıyor ama {config.name} listesinde yok; doğurulamaz.", data);
                }
                else
                {
                    report.Warning($"{data.name}: {config.name} listesinde yok; doğurulamaz (henüz kullanan gece yok).", data);
                }
            }
        }

        private static void ValidateIds(IReadOnlyList<EnemyData> enemies, ValidationReport report)
        {
            var owners = new Dictionary<string, EnemyData>(StringComparer.Ordinal);
            foreach (EnemyData data in enemies)
            {
                if (string.IsNullOrEmpty(data.Id))
                {
                    report.Error($"{data.name}: id boş; dalga ve spawner bu tipi bulamaz.", data);
                }
                else if (owners.TryGetValue(data.Id, out EnemyData owner))
                {
                    report.Error($"{data.name}: id '{data.Id}' {owner.name} ile aynı; id'ler benzersiz olmalı.", data);
                }
                else
                {
                    owners.Add(data.Id, data);
                }
            }
        }

        private static void ValidatePrefab(EnemyData data, ValidationReport report)
        {
            Enemy prefab = data.Prefab;
            if (prefab == null)
            {
                return;
            }

            if (prefab.Data == null)
            {
                report.Error($"{data.name}: prefab '{prefab.name}' kökündeki Enemy'nin verisi boş; Tools/Last Haven/Generate Prefabs çalıştır.", data);
            }
            else if (prefab.Data != data)
            {
                report.Error($"{data.name}: prefab '{prefab.name}' başka veriye ({prefab.Data.name}) bağlı; havuzdan çıkan düşman " +
                             $"{prefab.Data.name} değerleriyle kurulur. Her tipin kendi prefab'ı olmalı.", data);
            }
        }
    }
}
