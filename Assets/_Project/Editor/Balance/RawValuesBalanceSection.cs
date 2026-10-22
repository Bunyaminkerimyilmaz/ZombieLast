using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Denge tablosu: oynanış verisinin tüm ham değerleri (<see cref="SerializedValueTable"/>), klasör ve yol sırasıyla, asset başına
    /// katlanır blok. Sunum verisi (ses, ışık, kamera, UI, efekt, harita, debug) dengeyi etkilemediği için dahil edilmez.
    /// </summary>
    public sealed class RawValuesBalanceSection : IBalanceSection
    {
        /// <summary>Dahil edilen <c>Data/</c> alt klasörleri (oynanış).</summary>
        public static readonly string[] Folders =
        {
            "Player", "Weapons", "Enemies", "Waves", "Base", "Building", "Economy", "Shop", "Loot", "Progression", "GameFlow",
        };

        public int Order => 900;

        public void Append(BalanceSheetContext context, StringBuilder text)
        {
            BalanceFormat.Heading(text, "Tüm değerler (ham)");
            text.Append("Oynanış SO'larının inspector'da görünen tüm alanları (`Data/` altında: ")
                .Append(string.Join(", ", Folders)).Append("). Sunum verisi (ses, ışık, kamera, UI, efekt, harita, debug) hariç.\n");

            foreach (string folder in Folders)
            {
                List<ScriptableObject> assets = LoadFolder(BalanceSheetContext.DataFolder + "/" + folder);
                if (assets.Count == 0)
                {
                    continue;
                }

                text.Append('\n').Append("### ").Append(folder).Append('\n');
                foreach (ScriptableObject asset in assets)
                {
                    text.Append('\n').Append("<details><summary><b>").Append(asset.name).Append("</b> (")
                        .Append(asset.GetType().Name).Append(")</summary>\n\n");
                    SerializedValueTable.Build(asset).AppendTo(text);
                    text.Append("\n</details>\n");
                }
            }
        }

        private static List<ScriptableObject> LoadFolder(string folder)
        {
            var paths = new List<string>();
            if (AssetDatabase.IsValidFolder(folder))
            {
                foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { folder }))
                {
                    paths.Add(AssetDatabase.GUIDToAssetPath(guid));
                }
            }

            paths.Sort(StringComparer.Ordinal);
            var assets = new List<ScriptableObject>(paths.Count);
            foreach (string path in paths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
    }
}
