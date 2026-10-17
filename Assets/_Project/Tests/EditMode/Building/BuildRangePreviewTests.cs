using LastHaven.Building;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>İnşa modundaki menzil dairesi: sadece menzilli seviyede görünür, yarıçap run çarpanlı menzil, merkez yapının pivotu.</summary>
    public class BuildRangePreviewTests
    {
        private const float Tolerance = 0.001f;

        private static readonly BuildableLevel Turret = new BuildableLevel("Standart", 250, 250f,
            new BuildableStatValue(BuildableStat.Damage, 8f),
            new BuildableStatValue(BuildableStat.Range, 12f));

        private static readonly BuildableLevel Barricade = new BuildableLevel("Ahşap", 50, 300f);

        private GameObject _object;
        private LineRenderer _line;

        [SetUp]
        public void SetUp()
        {
            _object = new GameObject("RangeIndicator");
            _object.transform.SetPositionAndRotation(new Vector3(0f, 0.05f, 0f), Quaternion.Euler(90f, 0f, 0f));
            _line = _object.AddComponent<LineRenderer>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_object);
        }

        [Test]
        public void New_IsHiddenAndLocalLoop()
        {
            var preview = Create(null);

            Assert.IsFalse(preview.IsVisible);
            Assert.IsFalse(_line.useWorldSpace);
            Assert.IsTrue(_line.loop);
            Assert.AreEqual(LineAlignment.TransformZ, _line.alignment);
            Assert.AreEqual(0.1f, _line.widthMultiplier, Tolerance);
        }

        [Test]
        public void Show_TurretLevel_DrawsRingAtCenter()
        {
            var preview = Create(null);

            Assert.IsTrue(preview.Show(Turret, new Vector3(3f, 0f, -4f)));

            Assert.IsTrue(preview.IsVisible);
            Assert.AreEqual(12f, preview.Radius, Tolerance);
            Assert.AreEqual(32, _line.positionCount);
            Assert.AreEqual(3f, _object.transform.position.x, Tolerance);
            Assert.AreEqual(-4f, _object.transform.position.z, Tolerance);
            Assert.AreEqual(0.05f, _object.transform.position.y, Tolerance, "Yükseklik korunur (zeminin hemen üstü).");

            // Yerel XY → dünya XZ (X'te 90° yatırılmış), merkezden 12 m.
            Vector3 world = _object.transform.TransformPoint(_line.GetPosition(8));
            Assert.AreEqual(12f, Vector2.Distance(new Vector2(world.x, world.z), new Vector2(3f, -4f)), Tolerance);
            Assert.AreEqual(0.05f, world.y, Tolerance, "Halka zeminde yatay.");
        }

        [Test]
        public void Show_LevelWithoutRange_Hides()
        {
            var preview = Create(null);
            preview.Show(Turret, Vector3.zero);

            Assert.IsFalse(preview.Show(Barricade, Vector3.zero));
            Assert.IsFalse(preview.IsVisible);
            Assert.IsFalse(preview.Show(null, Vector3.zero));
        }

        [Test]
        public void Show_UsesRunModifiers()
        {
            var modifiers = new RunModifiers();
            modifiers.AddModifier(StatModifier.Flat(StatType.TurretRange, 3f, "test"));
            var preview = Create(modifiers);

            preview.Show(Turret, Vector3.zero);

            Assert.AreEqual(15f, preview.Radius, Tolerance);
        }

        [Test]
        public void Hide_HidesRing()
        {
            var preview = Create(null);
            preview.Show(Turret, Vector3.zero);

            preview.Hide();

            Assert.IsFalse(preview.IsVisible);
        }

        [Test]
        public void NullLine_IsHarmless()
        {
            var preview = new BuildRangePreview(null, null, Color.cyan, 0.1f, 32);

            Assert.IsFalse(preview.Show(Turret, Vector3.zero));
            Assert.DoesNotThrow(preview.Hide);
        }

        [Test]
        public void FillCircle_PointsOnRadius()
        {
            var points = new Vector3[16];

            BuildRangePreview.FillCircle(points, 5f);

            foreach (Vector3 point in points)
            {
                Assert.AreEqual(5f, new Vector2(point.x, point.y).magnitude, Tolerance);
                Assert.AreEqual(0f, point.z);
            }
        }

        private BuildRangePreview Create(IRunModifiers modifiers)
        {
            return new BuildRangePreview(_line, modifiers, Color.cyan, 0.1f, 32);
        }
    }
}
