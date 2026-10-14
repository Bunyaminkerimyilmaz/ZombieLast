using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>CameraPresets.Next: F9 kısayolu preset'leri sırayla dolaşır.</summary>
    public class CameraPresetsTests
    {
        [Test]
        public void Next_CyclesThroughPresets()
        {
            Assert.AreEqual(CameraPreset.TopDown, CameraPresets.Next(CameraPreset.Isometric));
            Assert.AreEqual(CameraPreset.Isometric, CameraPresets.Next(CameraPreset.TopDown));
        }
    }
}
