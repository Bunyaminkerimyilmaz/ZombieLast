using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Gündüz istihbaratı (11.2, GDD §9): bilgi sadece gündüzde ve geçerli gecede okunur (<see cref="TonightIntel"/>); istihbarat
    /// satırı gerçek <c>HUD.uxml</c>'deki satırla "20 Yürüyen" ve boss etiketi yazar (<see cref="IntelRowView"/>).
    /// </summary>
    public class TonightIntelTests
    {
        private sealed class FakeWaves : IWaveService
        {
            public int TotalNights { get; set; } = 10;

            public bool IsNightRunning => false;

            public int AliveCount => 0;

            public List<int> Requested { get; } = new List<int>();

            public bool StartNight(int nightIndex) => false;

            public NightInfo GetTonightInfo(int nightIndex)
            {
                if (nightIndex < 0 || nightIndex >= TotalNights)
                {
                    throw new ArgumentOutOfRangeException(nameof(nightIndex));
                }

                Requested.Add(nightIndex);
                return new NightInfo(nightIndex, nightIndex + 1, new[] { CompassDirection.North }, null, null);
            }

            public bool TryGetSpawnZoneCenter(CompassDirection direction, out Vector3 center)
            {
                center = direction.ToVector3();
                return true;
            }
        }

        [Test]
        public void Get_InDay_ReturnsTonight()
        {
            var waves = new FakeWaves();

            NightInfo info = TonightIntel.Get(waves, GamePhase.Day, 2);

            Assert.IsNotNull(info);
            Assert.AreEqual(3, info.NightNumber);
            CollectionAssert.AreEqual(new[] { 2 }, waves.Requested);
        }

        [Test]
        public void Get_OutsideDay_IsNull_WithoutQuerying()
        {
            var waves = new FakeWaves();

            foreach (GamePhase phase in Enum.GetValues(typeof(GamePhase)))
            {
                if (phase != GamePhase.Day)
                {
                    Assert.IsNull(TonightIntel.Get(waves, phase, 0), phase.ToString());
                }
            }

            Assert.IsEmpty(waves.Requested);
        }

        [Test]
        public void Get_InvalidIndexOrNoService_IsNull()
        {
            var waves = new FakeWaves { TotalNights = 3 };

            Assert.IsNull(TonightIntel.Get(waves, GamePhase.Day, 3));
            Assert.IsNull(TonightIntel.Get(waves, GamePhase.Day, -1));
            Assert.IsNull(TonightIntel.Get(null, GamePhase.Day, 0));
        }

        [Test]
        public void IntelRow_ShowsCountAndName_BossTagOnlyForBoss()
        {
            VisualElement row = FirstRow();
            var view = new IntelRowView(row);
            Assert.IsTrue(view.IsValid);

            view.Show(new NightEnemyInfo("walker", "Yürüyen", 20, isBoss: false));

            Assert.IsFalse(row.ClassListContains(UIClassNames.Hidden));
            Assert.AreEqual("20", row.Q<Label>(className: IntelRowView.CountClass).text);
            Assert.AreEqual("Yürüyen", row.Q<Label>(className: IntelRowView.NameClass).text);
            Assert.IsTrue(row.Q(className: IntelRowView.BossClass).ClassListContains(UIClassNames.Hidden));

            view.Show(new NightEnemyInfo("brute", "Brute", 1, isBoss: true));
            Assert.IsFalse(row.Q(className: IntelRowView.BossClass).ClassListContains(UIClassNames.Hidden));

            view.Hide();
            Assert.IsTrue(row.ClassListContains(UIClassNames.Hidden));
        }

        private static VisualElement FirstRow()
        {
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIGenerator.HudPath);
            Assert.IsNotNull(layout, UIGenerator.HudPath);
            VisualElement row = layout.Instantiate().Q(className: IntelRowView.RowClass);
            Assert.IsNotNull(row);
            return row;
        }
    }
}
