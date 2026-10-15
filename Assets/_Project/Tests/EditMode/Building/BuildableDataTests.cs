using LastHaven.Building;
using LastHaven.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Yapı verisi (GDD §11): ayak izi, seviyeler, inşa fiyatı, statlar; BuildConfig varsayılanları.</summary>
    public class BuildableDataTests
    {
        private const float Tolerance = 0.0001f;

        private BuildableData _data;
        private BuildConfig _config;

        [TearDown]
        public void TearDown()
        {
            if (_data != null)
            {
                Object.DestroyImmediate(_data);
            }

            if (_config != null)
            {
                Object.DestroyImmediate(_config);
            }
        }

        [Test]
        public void New_DefaultsToSingleCellWithoutLevels()
        {
            _data = ScriptableObject.CreateInstance<BuildableData>();

            Assert.AreEqual(Vector2Int.one, _data.Footprint);
            Assert.AreEqual(0, _data.LevelCount);
            Assert.AreEqual(0, _data.BuildCost);
            Assert.IsNull(_data.GetLevel(0));
            Assert.IsNull(_data.Prefab);
            Assert.IsNull(_data.Icon);
        }

        [Test]
        public void BuildCost_IsFirstLevelCost()
        {
            _data = Create(new Vector2Int(3, 1),
                new BuildableLevel("Ahşap", 50, 300f),
                new BuildableLevel("Metal", 120, 800f),
                new BuildableLevel("Beton", 300, 2000f));

            Assert.AreEqual(50, _data.BuildCost);
            Assert.AreEqual(3, _data.LevelCount);
            Assert.AreEqual("Metal", _data.GetLevel(1).DisplayName);
            Assert.AreEqual(2000f, _data.GetLevel(2).MaxHealth, Tolerance);
            Assert.IsNull(_data.GetLevel(3));
            Assert.IsNull(_data.GetLevel(-1));
        }

        [Test]
        public void Footprint_IsClampedToAtLeastOneCell()
        {
            _data = Create(new Vector2Int(0, -2), new BuildableLevel("Test", 10, 100f));

            Assert.AreEqual(Vector2Int.one, _data.Footprint);
        }

        [Test]
        public void Level_TryGetStat_FindsWrittenStatOnly()
        {
            var level = new BuildableLevel("1", 250, 250f,
                new BuildableStatValue(BuildableStat.Damage, 8f),
                new BuildableStatValue(BuildableStat.FireRate, 5f),
                new BuildableStatValue(BuildableStat.Range, 12f));

            Assert.IsTrue(level.TryGetStat(BuildableStat.Range, out float range));
            Assert.AreEqual(12f, range, Tolerance);
            Assert.IsFalse(level.TryGetStat(BuildableStat.SlowFraction, out _));
            Assert.AreEqual(0.4f, level.GetStat(BuildableStat.SlowFraction, 0.4f), Tolerance);
            Assert.AreEqual(8f, level.GetStat(BuildableStat.Damage, 0f), Tolerance);
        }

        [Test]
        public void Level_DuplicateStat_FirstWins()
        {
            var level = new BuildableLevel("1", 0, 1f,
                new BuildableStatValue(BuildableStat.Damage, 8f),
                new BuildableStatValue(BuildableStat.Damage, 99f));

            Assert.AreEqual(8f, level.GetStat(BuildableStat.Damage, 0f), Tolerance);
        }

        [Test]
        public void Level_IsSerializedWithStats()
        {
            _data = Create(new Vector2Int(2, 2),
                new BuildableLevel("1", 250, 250f, new BuildableStatValue(BuildableStat.Range, 12f)),
                new BuildableLevel("2", 300, 250f, new BuildableStatValue(BuildableStat.Range, 15f)));

            Assert.AreEqual(300, _data.GetLevel(1).Cost);
            Assert.AreEqual(15f, _data.GetLevel(1).GetStat(BuildableStat.Range, 0f), Tolerance);
        }

        [Test]
        public void Config_Defaults_MatchGdd()
        {
            _config = ScriptableObject.CreateInstance<BuildConfig>();

            Assert.AreEqual(1f, _config.CellSize, Tolerance, "1 m hücre.");
            Assert.AreEqual(3f, _config.GeneratorKeepOutHalfExtent, Tolerance, "Jeneratör yarı boyu 1 + 2 m.");
            Assert.Greater(_config.ValidColor.a, 0f);
            Assert.Less(_config.ValidColor.a, 1f, "Önizleme yarı saydam.");
            Assert.Less(_config.InvalidColor.a, 1f);
        }

        [Test]
        public void Config_BlockingLayers_AreBodiesNotStructures()
        {
            _config = ScriptableObject.CreateInstance<BuildConfig>();
            int mask = _config.BlockingLayers;

            Assert.IsTrue(GameLayers.Contains(mask, GameLayers.Player));
            Assert.IsTrue(GameLayers.Contains(mask, GameLayers.Enemy));
            Assert.IsFalse(GameLayers.Contains(mask, GameLayers.Structure), "Yapı çakışmasını ızgara (doluluk) yakalar.");
            Assert.IsFalse(GameLayers.Contains(mask, GameLayers.Ground));
        }

        [Test]
        public void New_TargetTypeDefaultsToBarricade()
        {
            _data = ScriptableObject.CreateInstance<BuildableData>();

            Assert.AreEqual(TargetType.Barricade, _data.TargetType);
        }

        [Test]
        public void Level_MaterialIsOptionalAndSerialized()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            try
            {
                _data = Create(new Vector2Int(3, 1),
                    new BuildableLevel("Ahşap", 50, 300f, material),
                    new BuildableLevel("Metal", 120, 800f));

                Assert.AreSame(material, _data.GetLevel(0).Material);
                Assert.IsNull(_data.GetLevel(1).Material);
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void Config_Highlight_IsTranslucentShellAroundStructure()
        {
            _config = ScriptableObject.CreateInstance<BuildConfig>();

            Assert.Greater(_config.HighlightColor.a, 0f);
            Assert.Less(_config.HighlightColor.a, 1f, "Seviye rengi kabuğun altından görünsün.");
            Assert.Greater(_config.HighlightScale, 1f, "Kabuk yapının yüzeylerinin dışında.");
        }

        private static BuildableData Create(Vector2Int footprint, params BuildableLevel[] levels)
        {
            var data = ScriptableObject.CreateInstance<BuildableData>();
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_footprint").vector2IntValue = footprint;
            SerializedProperty list = serialized.FindProperty("_levels");
            list.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
            {
                list.GetArrayElementAtIndex(i).boxedValue = levels[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }
    }
}
