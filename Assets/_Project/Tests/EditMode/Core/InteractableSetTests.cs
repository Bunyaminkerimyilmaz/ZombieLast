using System;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>InteractableSet (8.3): kayıt ve menzildeki en yakın etkileşilebilir nesne.</summary>
    public class InteractableSetTests
    {
        private InteractableSet _set;

        [SetUp]
        public void SetUp()
        {
            _set = new InteractableSet();
        }

        [Test]
        public void Register_AddsOnceAndUnregisterRemoves()
        {
            var item = new FakeInteractable(Vector3.zero);

            Assert.IsTrue(_set.Register(item));
            Assert.IsFalse(_set.Register(item), "İkinci kayıt yok sayılır.");
            Assert.AreEqual(1, _set.Count);

            Assert.IsTrue(_set.Unregister(item));
            Assert.IsFalse(_set.Unregister(item));
            Assert.IsFalse(_set.Unregister(null));
            Assert.AreEqual(0, _set.Count);
            Assert.Throws<ArgumentNullException>(() => _set.Register(null));
        }

        [Test]
        public void FindNearest_PicksClosestWithinItsOwnRange()
        {
            var far = Register(new Vector3(1.5f, 0f, 0f));
            var near = Register(new Vector3(0f, 0f, -1f));
            Register(new Vector3(5f, 0f, 0f));

            Assert.AreSame(near, _set.FindNearest(Vector3.zero));

            near.CanInteract = false;
            Assert.AreSame(far, _set.FindNearest(Vector3.zero), "Etkileşim sunmayan atlanır.");
        }

        [Test]
        public void FindNearest_RangeIsInclusiveAndPerItem()
        {
            var item = Register(new Vector3(2f, 0f, 0f));
            var wide = Register(new Vector3(-4f, 0f, 0f), range: 5f);

            Assert.AreSame(item, _set.FindNearest(Vector3.zero), "2 m sınırı dahil.");
            Assert.AreSame(wide, _set.FindNearest(new Vector3(-0.1f, 0f, 0f)), "2.1 m: ilki menzil dışı, ikincinin menzili 5 m.");
            Assert.IsNull(_set.FindNearest(new Vector3(10f, 0f, 10f)));
        }

        [Test]
        public void FindNearest_MeasuresOnHorizontalPlane()
        {
            var item = Register(new Vector3(1f, 30f, 0f));

            Assert.AreSame(item, _set.FindNearest(Vector3.zero));
        }

        [Test]
        public void FindNearest_SkipsDestroyedUnityObjects()
        {
            var destroyed = ScriptableObject.CreateInstance<DestroyableInteractable>();
            _set.Register(destroyed);
            UnityEngine.Object.DestroyImmediate(destroyed);
            var alive = Register(new Vector3(1f, 0f, 0f));

            Assert.AreSame(alive, _set.FindNearest(Vector3.zero));
            Assert.IsTrue(InteractableSet.IsDestroyed(destroyed));
            Assert.IsTrue(InteractableSet.IsDestroyed(null));
            Assert.IsFalse(InteractableSet.IsDestroyed(alive));
        }

        private FakeInteractable Register(Vector3 position, float range = 2f)
        {
            var item = new FakeInteractable(position, range);
            _set.Register(item);
            return item;
        }
    }

    /// <summary>Yok edilebilen Unity nesnesi etkileşimi (ScriptableObject: test assembly'sinin MonoBehaviour'u GameObject'e eklenmez).</summary>
    public sealed class DestroyableInteractable : ScriptableObject, IInteractable
    {
        public Vector3 PromptPosition => Vector3.zero;

        public string Prompt => string.Empty;

        public bool CanInteract => true;

        public float HoldDuration => 1f;

        public float InteractionRange => 100f;

        public float GetDistance(Vector3 point) => 0f;

        public void OnHoldProgress(float progress)
        {
        }

        public bool OnInteract() => true;
    }
}
