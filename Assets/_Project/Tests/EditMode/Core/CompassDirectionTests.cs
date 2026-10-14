using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    public class CompassDirectionTests
    {
        [Test]
        public void ToVector3_MatchesWorldAxes()
        {
            Assert.AreEqual(Vector3.forward, CompassDirection.North.ToVector3());
            Assert.AreEqual(Vector3.right, CompassDirection.East.ToVector3());
            Assert.AreEqual(Vector3.back, CompassDirection.South.ToVector3());
            Assert.AreEqual(Vector3.left, CompassDirection.West.ToVector3());
        }

        [Test]
        public void ToVector3_OppositeDirectionsCancel()
        {
            Assert.AreEqual(Vector3.zero, CompassDirection.North.ToVector3() + CompassDirection.South.ToVector3());
            Assert.AreEqual(Vector3.zero, CompassDirection.East.ToVector3() + CompassDirection.West.ToVector3());
        }

        [Test]
        public void All_ContainsFourDirectionsInEnumOrder()
        {
            CollectionAssert.AreEqual(
                new[] { CompassDirection.North, CompassDirection.East, CompassDirection.South, CompassDirection.West },
                CompassDirections.All);
        }
    }
}
