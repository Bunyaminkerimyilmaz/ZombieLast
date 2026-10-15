using System.Collections.Generic;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>AvoidancePriority (8.3): kalabalık barikat önünde kilitlenmesin diye doğuşta rastgele kaçınma önceliği.</summary>
    public class AvoidancePriorityTests
    {
        [TestCase(0f, 30)]
        [TestCase(0.999f, 70)]
        [TestCase(1f, 70)]
        [TestCase(0.5f, 50)]
        public void Pick_MapsRandomOntoInclusiveRange(float random01, int expected)
        {
            Assert.AreEqual(expected, AvoidancePriority.Pick(30, 70, random01));
        }

        [Test]
        public void Pick_OrderDoesNotMatterAndClampsToAgentLimits()
        {
            Assert.AreEqual(30, AvoidancePriority.Pick(70, 30, 0f));
            Assert.AreEqual(0, AvoidancePriority.Pick(-20, -5, 0.7f));
            Assert.AreEqual(99, AvoidancePriority.Pick(150, 200, 0.3f));
            Assert.AreEqual(42, AvoidancePriority.Pick(42, 42, 0.9f), "Tek değerli aralık.");
        }

        [Test]
        public void Pick_CoversWholeRange()
        {
            var seen = new HashSet<int>();
            for (int i = 0; i < 1000; i++)
            {
                seen.Add(AvoidancePriority.Pick(30, 70, i / 1000f));
            }

            Assert.AreEqual(41, seen.Count, "30..70 dahil, her değer çıkabilir.");
        }

        [Test]
        public void EnemyData_DefaultRangeIsValidAndPicksInside()
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            try
            {
                Assert.That(data.MinAvoidancePriority, Is.InRange(AvoidancePriority.Min, AvoidancePriority.Max));
                Assert.That(data.MaxAvoidancePriority, Is.InRange(data.MinAvoidancePriority, AvoidancePriority.Max));
                Assert.Less(data.MinAvoidancePriority, data.MaxAvoidancePriority, "Aralık tek değer olursa kilitlenme önlenmez.");
                for (int i = 0; i < 20; i++)
                {
                    Assert.That(data.PickAvoidancePriority(), Is.InRange(data.MinAvoidancePriority, data.MaxAvoidancePriority));
                }
            }
            finally
            {
                Object.DestroyImmediate(data);
            }
        }
    }
}
