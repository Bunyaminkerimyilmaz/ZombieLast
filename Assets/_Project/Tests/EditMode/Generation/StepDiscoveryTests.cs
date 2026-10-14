using System.Collections.Generic;
using LastHaven.Editor;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Üretici ve sahne adımlarının TypeCache ile bulunup doğru sırada çalıştığını doğrular.</summary>
    public class StepDiscoveryTests
    {
        // Bilerek IGenerationStep/ISceneBuildStep uygulamaz; yoksa gerçek menüler bunları da çalıştırırdı.
        private class FakeStepA
        {
            public FakeStepA(int order) => Order = order;

            public int Order { get; }
        }

        private class FakeStepB : FakeStepA
        {
            public FakeStepB(int order) : base(order)
            {
            }
        }

        [Test]
        public void Sort_OrdersByOrderThenTypeName()
        {
            var b5 = new FakeStepB(5);
            var a5 = new FakeStepA(5);
            var b1 = new FakeStepB(1);
            var steps = new List<FakeStepA> { b5, a5, b1 };

            StepDiscovery.Sort(steps, step => step.Order);

            CollectionAssert.AreEqual(new FakeStepA[] { b1, a5, b5 }, steps);
        }

        [Test]
        public void CreateAll_FindsGenerationStepsInOrder()
        {
            List<IGenerationStep> steps = StepDiscovery.CreateAll<IGenerationStep>(step => step.Order);

            int materials = IndexOf<PlaceholderMaterialsGenerator, IGenerationStep>(steps);
            int map = IndexOf<MapGenerator, IGenerationStep>(steps);
            Assert.GreaterOrEqual(materials, 0);
            Assert.Greater(map, materials);
        }

        [Test]
        public void CreateAll_FindsSceneStepsInOrder()
        {
            List<ISceneBuildStep> steps = StepDiscovery.CreateAll<ISceneBuildStep>(step => step.Order);

            int map = IndexOf<MapSceneStep, ISceneBuildStep>(steps);
            int lighting = IndexOf<LightingSceneStep, ISceneBuildStep>(steps);
            int waves = IndexOf<WavesSceneStep, ISceneBuildStep>(steps);
            Assert.GreaterOrEqual(map, 0);
            Assert.Greater(lighting, map);
            Assert.Greater(waves, lighting);
        }

        private static int IndexOf<TStep, TInterface>(List<TInterface> steps)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i] is TStep)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
