using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>HitTargetTypes.FromLayer: layer → vuruş türü.</summary>
    public class HitTargetTypesTests
    {
        [TestCase(GameLayers.Enemy, HitTargetType.Enemy)]
        [TestCase(GameLayers.Structure, HitTargetType.Structure)]
        [TestCase(GameLayers.Player, HitTargetType.Player)]
        [TestCase(GameLayers.Ground, HitTargetType.Surface)]
        [TestCase(GameLayers.Obstacle, HitTargetType.Surface)]
        [TestCase(0, HitTargetType.Surface)]
        public void FromLayer_MapsProjectLayers(int layer, HitTargetType expected)
        {
            Assert.AreEqual(expected, HitTargetTypes.FromLayer(layer));
        }
    }
}
