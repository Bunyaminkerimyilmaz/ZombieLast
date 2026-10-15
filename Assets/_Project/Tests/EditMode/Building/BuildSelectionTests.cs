using System.Collections.Generic;
using LastHaven.Building;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>İnşa modunda yapı seçimi (1–5, D-pad sırası, iptal) ve döndürme; yerleşim noktası için zemin kesişimi.</summary>
    public class BuildSelectionTests
    {
        private readonly List<BuildableData> _created = new List<BuildableData>();

        [TearDown]
        public void TearDown()
        {
            foreach (BuildableData data in _created)
            {
                Object.DestroyImmediate(data);
            }

            _created.Clear();
        }

        [Test]
        public void New_HasNoSelectionAndNoRotation()
        {
            var selection = new BuildSelection(Slots(2));

            Assert.AreEqual(-1, selection.SelectedIndex);
            Assert.IsNull(selection.Selected);
            Assert.IsFalse(selection.HasSelection);
            Assert.AreEqual(0, selection.RotationSteps);
        }

        [Test]
        public void Select_FilledSlot_ChangesSelection()
        {
            List<BuildableData> slots = Slots(3);
            var selection = new BuildSelection(slots);

            Assert.IsTrue(selection.Select(1));
            Assert.AreEqual(1, selection.SelectedIndex);
            Assert.AreSame(slots[1], selection.Selected);
        }

        [Test]
        public void Select_SameSlotAgain_ReturnsFalse()
        {
            var selection = new BuildSelection(Slots(2));
            selection.Select(0);

            Assert.IsFalse(selection.Select(0));
            Assert.AreEqual(0, selection.SelectedIndex);
        }

        [Test]
        public void Select_EmptyOrMissingSlot_KeepsSelection()
        {
            var slots = new List<BuildableData> { Data(), null };
            var selection = new BuildSelection(slots);
            selection.Select(0);

            Assert.IsFalse(selection.Select(1), "Boş yuva.");
            Assert.IsFalse(selection.Select(4), "Listede olmayan yuva (5 tuşu).");
            Assert.IsFalse(selection.Select(-1));
            Assert.AreEqual(0, selection.SelectedIndex);
        }

        [Test]
        public void IsAvailable_OnlyFilledSlots()
        {
            var selection = new BuildSelection(new List<BuildableData> { Data(), null, Data() });

            Assert.IsTrue(selection.IsAvailable(0));
            Assert.IsFalse(selection.IsAvailable(1));
            Assert.IsTrue(selection.IsAvailable(2));
            Assert.IsFalse(selection.IsAvailable(3));
            Assert.IsFalse(selection.IsAvailable(-1));
        }

        [Test]
        public void Cycle_FromNone_ForwardPicksFirstBackwardPicksLast()
        {
            var forward = new BuildSelection(new List<BuildableData> { null, Data(), Data(), null });
            var backward = new BuildSelection(new List<BuildableData> { null, Data(), Data(), null });

            Assert.IsTrue(forward.Cycle(1));
            Assert.AreEqual(1, forward.SelectedIndex);
            Assert.IsTrue(backward.Cycle(-1));
            Assert.AreEqual(2, backward.SelectedIndex);
        }

        [Test]
        public void Cycle_SkipsEmptySlotsAndWraps()
        {
            var selection = new BuildSelection(new List<BuildableData> { Data(), null, Data(), null, Data() });
            selection.Select(0);

            selection.Cycle(1);
            Assert.AreEqual(2, selection.SelectedIndex);
            selection.Cycle(1);
            Assert.AreEqual(4, selection.SelectedIndex);
            selection.Cycle(1);
            Assert.AreEqual(0, selection.SelectedIndex, "Başa sarar.");
            selection.Cycle(-1);
            Assert.AreEqual(4, selection.SelectedIndex, "Geriye sarar.");
        }

        [Test]
        public void Cycle_SingleFilledSlot_NoChangeAfterFirst()
        {
            var selection = new BuildSelection(new List<BuildableData> { null, Data() });

            Assert.IsTrue(selection.Cycle(1));
            Assert.IsFalse(selection.Cycle(1));
            Assert.AreEqual(1, selection.SelectedIndex);
        }

        [Test]
        public void Cycle_NoFilledSlotsOrZeroDirection_ReturnsFalse()
        {
            Assert.IsFalse(new BuildSelection(new List<BuildableData> { null, null }).Cycle(1));
            Assert.IsFalse(new BuildSelection(new List<BuildableData>()).Cycle(-1));
            Assert.IsFalse(new BuildSelection(Slots(2)).Cycle(0));
        }

        [Test]
        public void Clear_RemovesSelectionOnce()
        {
            var selection = new BuildSelection(Slots(2));
            selection.Select(1);

            Assert.IsTrue(selection.Clear());
            Assert.IsFalse(selection.HasSelection);
            Assert.IsFalse(selection.Clear());
        }

        [Test]
        public void Rotate_StepsThroughFourQuarterTurns()
        {
            var selection = new BuildSelection(Slots(1));

            for (int expected = 1; expected <= 4; expected++)
            {
                selection.Rotate();
                Assert.AreEqual(expected % 4, selection.RotationSteps);
            }
        }

        [Test]
        public void Rotate_IsKeptWhenSelectionChanges()
        {
            var selection = new BuildSelection(Slots(2));
            selection.Select(0);
            selection.Rotate();

            selection.Select(1);
            selection.Clear();

            Assert.AreEqual(1, selection.RotationSteps);
        }

        [Test]
        public void TryIntersectGround_RayHitsPlaneAtHeight()
        {
            var ray = new Ray(new Vector3(0f, 10f, -10f), new Vector3(0f, -1f, 1f));

            Assert.IsTrue(BuildPointer.TryIntersectGround(ray, 2f, out Vector3 point));
            Assert.AreEqual(2f, point.y, 0.0001f);
            Assert.AreEqual(-2f, point.z, 0.0001f);
        }

        [Test]
        public void TryIntersectGround_ParallelOrAwayRay_ReturnsFalse()
        {
            Assert.IsFalse(BuildPointer.TryIntersectGround(new Ray(new Vector3(0f, 5f, 0f), Vector3.forward), 0f, out _));
            Assert.IsFalse(BuildPointer.TryIntersectGround(new Ray(new Vector3(0f, 5f, 0f), Vector3.up), 0f, out _));
        }

        private List<BuildableData> Slots(int count)
        {
            var slots = new List<BuildableData>(count);
            for (int i = 0; i < count; i++)
            {
                slots.Add(Data());
            }

            return slots;
        }

        private BuildableData Data()
        {
            var data = ScriptableObject.CreateInstance<BuildableData>();
            _created.Add(data);
            return data;
        }
    }
}
