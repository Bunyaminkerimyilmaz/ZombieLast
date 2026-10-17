using LastHaven.Economy;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Systems]/Loot: ganimet sandıkları (<see cref="LootCrateSpawner"/>) ve ödül dağıtıcısı (<see cref="LootDispenser"/>), ikisi de
    /// <see cref="LootConfig"/> ile; dağıtıcıya [Systems]/Economy'deki <see cref="CoinDropper"/> bağlanır (Economy adımından sonra).
    /// </summary>
    public sealed class LootSceneStep : ISceneBuildStep
    {
        public const string ObjectName = "Loot";

        private const string ConfigFieldName = "_config";
        private const string CoinsFieldName = "_coins";
        private const string DispenserFieldName = "_dispenser";

        public int Order => 77;

        public void Build(SceneBuildContext context)
        {
            LootConfig config = LootGenerator.LoadOrCreateAll();
            GameObject lootObject = context.CreateObject(ObjectName, context.Systems);

            var dispenser = lootObject.AddComponent<LootDispenser>();
            Assign(dispenser, ConfigFieldName, config);
            var coins = context.Systems.GetComponentInChildren<CoinDropper>(true);
            if (coins == null)
            {
                Debug.LogWarning("[Last Haven] Loot: sahnede CoinDropper yok; sandıktan para çıkmayacak.");
            }

            Assign(dispenser, CoinsFieldName, coins);

            var spawner = lootObject.AddComponent<LootCrateSpawner>();
            Assign(spawner, ConfigFieldName, config);
            Assign(spawner, DispenserFieldName, dispenser);
        }

        private static void Assign(Component component, string fieldName, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
