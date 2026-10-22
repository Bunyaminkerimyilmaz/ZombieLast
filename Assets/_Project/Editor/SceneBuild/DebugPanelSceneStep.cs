using LastHaven.DebugTools;
using LastHaven.Progression;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/DebugPanel (15.1): F12 debug paneli (<see cref="DebugPanelController"/>; sadece editör ve development build, sürüm
    /// build'inde obje kendini kapatır). Ortak PanelSettings, <c>UI/Debug/DebugPanel.uxml</c>, sıralama tüm ekranların üstünde.
    /// Düğmeler diğer debug adımlarının bileşenlerine (DebugSpawner, DebugWaves, DebugEconomy, DebugPerks) ve perk kataloğuna bağlanır;
    /// eksik olan uyarıyla boş kalır (o düğme bir şey yapmaz).
    /// </summary>
    public sealed class DebugPanelSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "DebugPanel";
        public const string LayoutPath = UIGenerator.Folder + "/Debug/DebugPanel.uxml";

        /// <summary>Duraklatma menüsü (30) ve ayarlar (40) dahil her ekranın üstünde.</summary>
        public const float SortingOrder = 100f;

        public int Order => 740; // Diğer debug adımlarından (700–730) sonra.

        public void Build(SceneBuildContext context)
        {
            GameObject panelObject = context.CreateObject(ObjectName, context.Systems);
            var document = panelObject.AddComponent<UIDocument>();
            document.panelSettings = UIGenerator.LoadOrCreatePanelSettings();
            document.visualTreeAsset = UIGenerator.LoadLayout(LayoutPath);
            document.sortingOrder = SortingOrder;

            var controller = panelObject.AddComponent<DebugPanelController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_document").objectReferenceValue = document;
            serialized.FindProperty("_spawner").objectReferenceValue = Find<DebugSpawner>(context, DebugSpawnerSceneStep.ObjectName);
            serialized.FindProperty("_waveControls").objectReferenceValue = Find<DebugWaveControls>(context, DebugWavesSceneStep.ObjectName);
            serialized.FindProperty("_economy").objectReferenceValue = Find<DebugEconomyControls>(context, DebugEconomySceneStep.ObjectName);
            serialized.FindProperty("_perkControls").objectReferenceValue = Find<DebugPerkControls>(context, DebugPerksSceneStep.ObjectName);
            serialized.FindProperty("_stress").objectReferenceValue = Find<DebugStressTest>(context, DebugStressSceneStep.ObjectName);
            serialized.FindProperty("_perkCatalog").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PerkCatalog>(ProgressionGenerator.CatalogPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T Find<T>(SceneBuildContext context, string objectName) where T : Component
        {
            Transform child = context.Systems.Find(objectName);
            T component = child != null ? child.GetComponent<T>() : null;
            if (component == null)
            {
                Debug.LogWarning($"[Last Haven] DebugPanel: [Systems]/{objectName} ({typeof(T).Name}) yok; ilgili düğmeler çalışmayacak.");
            }

            return component;
        }
    }
}
