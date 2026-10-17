using System.Collections.Generic;
using System.Text;
using LastHaven.Waves;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Dalgalar: GDD §8'in 10 gecesi (<see cref="NightDefinitions"/> → <c>Data/Waves/Nights/NightXX.asset</c>) ve
    /// <see cref="WaveConfig"/> (<c>Data/Waves/WaveConfig.asset</c>). "Eksikse ekle": var olan gece ve gruplara dokunulmaz;
    /// eksik gruplar, boş boss/olay referansları ve config'in boş gece satırları doldurulur (<see cref="NightDataFiller"/>).
    /// Projede henüz olmayan içerik (Gün 12: Tükürücü, Patlayan, Brute, gece olayları) atlanır ve TODO olarak loglanır.
    /// </summary>
    public sealed class WavesGenerator : IGenerationStep
    {
        public const string DataFolder = "Assets/_Project/Data/Waves";
        public const string NightsFolder = DataFolder + "/Nights";
        public const string ConfigPath = DataFolder + "/WaveConfig.asset";

        /// <summary>Gecelere bağlanacak EnemyData ve NightModifierData'nın arandığı klasör.</summary>
        public const string ContentFolder = "Assets/_Project/Data";

        private const string NightsField = "_nights";

        public int Order => 900; // EnemiesGenerator'dan (700) sonra: gece grupları düşman verilerine bağlanır.

        public string Name => "Waves";

        public static string GetNightPath(NightDefinition definition)
        {
            return $"{NightsFolder}/{definition.AssetName}.asset";
        }

        /// <summary>Config'i yükler; yoksa GDD §8 varsayılanlarıyla (boş gece listesi) oluşturur. Sahne adımı buradan alır.</summary>
        public static WaveConfig LoadOrCreateConfig()
        {
            var config = EditorAssetUtility.LoadOrCreate<WaveConfig>(ConfigPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Waves: {ConfigPath} oluşturuldu.");
            }

            return config;
        }

        public void GenerateData()
        {
            WaveConfig config = LoadOrCreateConfig();
            WaveContentLookup lookup = WaveContentLookup.FromProject(ContentFolder);
            var nights = new List<NightData>(NightDefinitions.All.Count);
            var todo = new StringBuilder();

            foreach (NightDefinition definition in NightDefinitions.All)
            {
                nights.Add(LoadOrCreateNight(definition, lookup, todo));
            }

            int linked = LinkNights(config, nights);
            if (linked > 0)
            {
                Debug.Log($"[Last Haven] Waves: {ConfigPath} listesine {linked} gece bağlandı.");
            }

            if (todo.Length > 0)
            {
                Debug.Log("[Last Haven] Waves: TODO (Gün 12) — projede henüz olmayan içerik gecelere eklenmedi; "
                          + "asset'ler gelince Generate Default Data eksikleri ekler:" + todo);
            }
        }

        /// <summary>Spawn bölgelerinin istihbarat ışık sütunu materyali (sütunları sahne adımı kurar).</summary>
        public void GeneratePrefabs()
        {
            IntelBeaconMaterial.GetOrCreate();
        }

        private static NightData LoadOrCreateNight(NightDefinition definition, WaveContentLookup lookup, StringBuilder todo)
        {
            string path = GetNightPath(definition);
            var night = EditorAssetUtility.LoadOrCreate<NightData>(path, out bool created);
            if (created)
            {
                NightDataFiller.ApplyNew(night, definition);
            }

            NightFillResult result = NightDataFiller.Fill(night, definition, lookup);
            if (created || result.Changed)
            {
                AssetDatabase.SaveAssetIfDirty(night);
            }

            if (created)
            {
                Debug.Log($"[Last Haven] Waves: {path} oluşturuldu ({result.AddedGroups} grup).");
            }
            else if (result.Changed)
            {
                Debug.Log($"[Last Haven] Waves: {path} eksikleri eklendi ({result.AddedGroups} grup"
                          + (result.BossAssigned ? ", boss" : string.Empty)
                          + (result.ModifierAssigned ? ", olay" : string.Empty) + ").");
            }

            if (result.Missing.Count > 0)
            {
                todo.Append($"\n  Gece {definition.Number}: {string.Join(", ", result.Missing)}");
            }

            return night;
        }

        /// <summary>
        /// Config'in gece listesini doldurur: <c>i</c>. satır boşsa <c>i</c>. geceyi yazar; liste kısaysa eksik geceleri sona ekler.
        /// Listede (herhangi bir sırada) zaten olan gece tekrar eklenmez. Dolu satırlara dokunulmaz: kullanıcının yerine koyduğu
        /// veya bilerek çıkardığı gece geri gelmez. Bağlanan gece sayısını döndürür.
        /// </summary>
        public static int LinkNights(WaveConfig config, IReadOnlyList<NightData> nights)
        {
            var serialized = new SerializedObject(config);
            SerializedProperty list = serialized.FindProperty(NightsField);
            int linked = 0;
            for (int i = 0; i < nights.Count; i++)
            {
                bool isOpenSlot = i >= list.arraySize || list.GetArrayElementAtIndex(i).objectReferenceValue == null;
                if (!isOpenSlot || Contains(list, nights[i]))
                {
                    continue;
                }

                if (i >= list.arraySize)
                {
                    list.arraySize++;
                }

                list.GetArrayElementAtIndex(Mathf.Min(i, list.arraySize - 1)).objectReferenceValue = nights[i];
                linked++;
            }

            if (linked > 0)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                if (EditorUtility.IsPersistent(config))
                {
                    AssetDatabase.SaveAssetIfDirty(config);
                }
            }

            return linked;
        }

        private static bool Contains(SerializedProperty list, NightData night)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == night)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
