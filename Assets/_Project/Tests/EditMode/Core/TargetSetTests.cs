using System;
using System.Collections.Generic;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Tests.EditMode
{
    /// <summary>TargetSet: tür bazlı kayıt, en yakın hedef sorgusu ve hedeflenemeyenlerin atlanması.</summary>
    public class TargetSetTests
    {
        private sealed class FakeTarget : ITargetable
        {
            public FakeTarget(Transform transform, TargetType type)
            {
                Transform = transform;
                TargetType = type;
            }

            public Transform Transform { get; }

            public TargetType TargetType { get; }

            public bool IsTargetable { get; set; } = true;
        }

        private readonly List<GameObject> _objects = new List<GameObject>();
        private TargetSet _set;

        [SetUp]
        public void SetUp()
        {
            _set = new TargetSet();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                if (_objects[i] != null)
                {
                    Object.DestroyImmediate(_objects[i]);
                }
            }

            _objects.Clear();
        }

        [Test]
        public void Register_AddsOnceAndGroupsByType()
        {
            FakeTarget player = CreateTarget(TargetType.Player, Vector3.zero);
            FakeTarget barricade = CreateTarget(TargetType.Barricade, Vector3.one);

            Assert.IsTrue(_set.Register(player));
            Assert.IsFalse(_set.Register(player));
            Assert.IsTrue(_set.Register(barricade));

            Assert.AreEqual(2, _set.Count);
            CollectionAssert.AreEqual(new[] { player }, _set.GetAll(TargetType.Player));
            CollectionAssert.AreEqual(new[] { barricade }, _set.GetAll(TargetType.Barricade));
            Assert.AreEqual(0, _set.GetAll(TargetType.Turret).Count);
        }

        [Test]
        public void Register_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _set.Register(null));
        }

        [Test]
        public void Unregister_RemovesAndReportsUnknown()
        {
            FakeTarget player = CreateTarget(TargetType.Player, Vector3.zero);
            _set.Register(player);

            Assert.IsTrue(_set.Unregister(player));
            Assert.IsFalse(_set.Unregister(player));
            Assert.IsFalse(_set.Unregister(null));
            Assert.AreEqual(0, _set.Count);
        }

        [Test]
        public void FindNearest_ReturnsClosestOfRequestedType()
        {
            FakeTarget far = Register(TargetType.Barricade, new Vector3(10f, 0f, 0f));
            FakeTarget near = Register(TargetType.Barricade, new Vector3(0f, 0f, 4f));
            Register(TargetType.Turret, new Vector3(1f, 0f, 0f));

            Assert.AreSame(near, _set.FindNearest(TargetType.Barricade, Vector3.zero, 100f));
            Assert.AreSame(far, _set.FindNearest(TargetType.Barricade, new Vector3(9f, 0f, 0f), 100f));
        }

        [Test]
        public void FindNearest_RespectsMaxDistanceInclusive()
        {
            FakeTarget target = Register(TargetType.Player, new Vector3(6f, 0f, 0f));

            Assert.AreSame(target, _set.FindNearest(TargetType.Player, Vector3.zero, 6f));
            Assert.IsNull(_set.FindNearest(TargetType.Player, Vector3.zero, 5.9f));
        }

        [Test]
        public void FindNearest_MeasuresOnHorizontalPlane()
        {
            FakeTarget high = Register(TargetType.Generator, new Vector3(3f, 50f, 0f));

            Assert.AreSame(high, _set.FindNearest(TargetType.Generator, Vector3.zero, 3f));
        }

        [Test]
        public void FindNearest_SkipsNonTargetable()
        {
            FakeTarget near = Register(TargetType.Player, new Vector3(1f, 0f, 0f));
            FakeTarget far = Register(TargetType.Player, new Vector3(5f, 0f, 0f));
            near.IsTargetable = false;

            Assert.AreSame(far, _set.FindNearest(TargetType.Player, Vector3.zero, 10f));

            far.IsTargetable = false;
            Assert.IsNull(_set.FindNearest(TargetType.Player, Vector3.zero, 10f));
        }

        [Test]
        public void FindNearest_SkipsDestroyedUnityTargets()
        {
            // Yok edilen hedef daha yakında: atlanmasaydı o seçilirdi.
            var destroyed = ScriptableObject.CreateInstance<TargetRegistryTestTarget>();
            destroyed.Initialize(CreateTarget(TargetType.Player, new Vector3(1f, 0f, 0f)).Transform);
            _set.Register(destroyed);
            FakeTarget alive = Register(TargetType.Player, new Vector3(5f, 0f, 0f));
            Object.DestroyImmediate(destroyed);

            Assert.AreSame(alive, _set.FindNearest(TargetType.Player, Vector3.zero, 10f));
        }

        private FakeTarget Register(TargetType type, Vector3 position)
        {
            FakeTarget target = CreateTarget(type, position);
            _set.Register(target);
            return target;
        }

        private FakeTarget CreateTarget(TargetType type, Vector3 position)
        {
            var gameObject = new GameObject($"Target_{type}");
            gameObject.transform.position = position;
            _objects.Add(gameObject);
            return new FakeTarget(gameObject.transform, type);
        }
    }
}
