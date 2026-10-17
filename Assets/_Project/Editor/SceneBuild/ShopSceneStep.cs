using LastHaven.Economy;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>[Systems]/Shop: market servisi (<see cref="ShopService"/>) ve <see cref="ShopCatalog"/>.</summary>
    public sealed class ShopSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "Shop";

        private const string CatalogFieldName = "_catalog";

        public int Order => 75;

        public void Build(SceneBuildContext context)
        {
            ShopCatalog catalog = ShopGenerator.LoadOrCreateAll();
            GameObject shopObject = context.CreateObject(ObjectName, context.Systems);
            var service = shopObject.AddComponent<ShopService>();
            var serialized = new SerializedObject(service);
            serialized.FindProperty(CatalogFieldName).objectReferenceValue = catalog;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
