using LastHaven.Economy;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/Economy: cüzdan servisi (<see cref="WalletService"/>), kombo (<see cref="ComboService"/>) ve jeton düşürücü
    /// (<see cref="CoinDropper"/>; kombo servisi bağlı), hepsi <see cref="EconomyConfig"/> ile.
    /// </summary>
    public sealed class EconomySceneStep : ISceneBuildStep
    {
        public const string ObjectName = "Economy";

        private const string ConfigFieldName = "_config";
        private const string ComboFieldName = "_combo";

        public int Order => 70;

        public void Build(SceneBuildContext context)
        {
            EconomyConfig config = EconomyGenerator.LoadOrCreateAll();
            GameObject economyObject = context.CreateObject(ObjectName, context.Systems);
            Assign(economyObject.AddComponent<WalletService>(), ConfigFieldName, config);
            var combo = economyObject.AddComponent<ComboService>();
            Assign(combo, ConfigFieldName, config);
            var dropper = economyObject.AddComponent<CoinDropper>();
            Assign(dropper, ConfigFieldName, config);
            Assign(dropper, ComboFieldName, combo);
        }

        private static void Assign(Component component, string fieldName, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
