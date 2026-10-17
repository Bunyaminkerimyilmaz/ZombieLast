using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// GroundSlamAttack.GetCenter: darbe merkezi gövdenin önünde, hedefe doğru (yatay); yön yoksa ileri. Menzildeki (4 m) hedef 4 m'lik
    /// dairenin içinde kalır.
    /// </summary>
    public class GroundSlamAttackTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void Center_IsBodyRadiusAheadTowardTarget_OnGround()
        {
            var feet = new Vector3(2f, 0.08f, 1f);

            Vector3 center = GroundSlamAttack.GetCenter(feet, new Vector3(0f, 3f, 4f), Vector3.right, 1.25f, 0f);

            Assert.AreEqual(2f, center.x, Tolerance);
            Assert.AreEqual(0.08f, center.y, Tolerance, "Yükseklik sayılmaz, merkez yerde.");
            Assert.AreEqual(2.25f, center.z, Tolerance);
        }

        [Test]
        public void Center_NoDirection_UsesFallback_ThenFeet()
        {
            Vector3 feet = Vector3.zero;

            Assert.AreEqual(2f, GroundSlamAttack.GetCenter(feet, Vector3.zero, Vector3.left, 1.25f, 0.75f).magnitude, Tolerance);
            Assert.AreEqual(-2f, GroundSlamAttack.GetCenter(feet, Vector3.zero, Vector3.left, 1.25f, 0.75f).x, Tolerance);
            Assert.AreEqual(feet, GroundSlamAttack.GetCenter(feet, Vector3.up, Vector3.down, 1.25f, 0f), "Yatay yön yoksa ayak noktası.");
        }

        [Test]
        public void TargetAtAttackRange_IsInsideSlam()
        {
            // GDD §7 Brute: menzil 4 m (merkezden hedef yüzeyine), alan 4 m. Gövde yarıçapı 1.25 (×2.5 kapsül).
            Vector3 center = GroundSlamAttack.GetCenter(Vector3.zero, Vector3.forward, Vector3.forward, 1.25f, 0f);
            var targetSurface = new Vector3(0f, 0f, 4f);
            var targetBehind = new Vector3(0f, 0f, -2.5f);

            Assert.Less(Vector3.Distance(center, targetSurface), 4f, "Menzil sınırındaki hedef dairenin içinde.");
            Assert.Less(Vector3.Distance(center, targetBehind), 4f, "Arkadaki 2.5 m de içinde.");
        }
    }
}
