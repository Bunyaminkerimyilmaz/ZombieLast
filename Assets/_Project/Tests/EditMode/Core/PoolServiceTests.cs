using System.Text.RegularExpressions;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// PoolService: alma, iade, yeniden kullanım, ön ısıtma, IPoolable çağrıları ve PooledLifetime.
    /// EditMode'da Awake/OnEnable çalışmaz; servis gerektiğinde elle kaydedilir. "Prefab" sahnedeki bir objedir.
    /// </summary>
    public class PoolServiceTests
    {
        private GameObject _host;
        private PoolService _pool;
        private GameObject _prefab;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            _host = new GameObject("PoolServiceTests_Host");
            _pool = _host.AddComponent<PoolService>();
            _prefab = new GameObject("PoolServiceTests_Prefab");
            _prefab.AddComponent<PooledLifetime>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_prefab);
            ServiceLocator.Clear();
        }

        [Test]
        public void Get_ReturnsActiveCloneAtPose()
        {
            var position = new Vector3(1f, 2f, 3f);
            Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

            GameObject instance = _pool.Get(_prefab, position, rotation);

            Assert.IsNotNull(instance);
            Assert.AreNotSame(_prefab, instance);
            Assert.IsTrue(instance.activeSelf);
            Assert.AreEqual(position, instance.transform.position);
            Assert.That(Quaternion.Angle(rotation, instance.transform.rotation), Is.LessThan(0.01f));
            Assert.AreEqual(_prefab.name, instance.name);
            AssertStats(created: 1, active: 1, inactive: 0);
        }

        [Test]
        public void Release_DeactivatesInstance_AndGetReusesIt()
        {
            GameObject first = _pool.Get(_prefab, Vector3.zero, Quaternion.identity);

            _pool.Release(first);

            Assert.IsFalse(first.activeSelf);
            AssertStats(created: 1, active: 0, inactive: 1);

            GameObject second = _pool.Get(_prefab, Vector3.one, Quaternion.identity);
            Assert.AreSame(first, second);
            Assert.IsTrue(second.activeSelf);
            Assert.AreEqual(Vector3.one, second.transform.position);
            AssertStats(created: 1, active: 1, inactive: 0);
        }

        [Test]
        public void Release_MovesInstanceBackUnderPool()
        {
            GameObject instance = _pool.Get(_prefab, Vector3.zero, Quaternion.identity);
            Transform container = instance.transform.parent;
            instance.transform.SetParent(null);

            _pool.Release(instance);

            Assert.AreSame(container, instance.transform.parent);
            Assert.IsTrue(container.IsChildOf(_host.transform));
        }

        [Test]
        public void Prewarm_CreatesInactiveInstances_AndGetDoesNotCreateMore()
        {
            _pool.Prewarm(_prefab, 5);

            AssertStats(created: 5, active: 0, inactive: 5);

            _pool.Get(_prefab, Vector3.zero, Quaternion.identity);
            AssertStats(created: 5, active: 1, inactive: 4);
        }

        [Test]
        public void Prewarm_OnlyCreatesMissingInstances()
        {
            _pool.Prewarm(_prefab, 3);
            _pool.Prewarm(_prefab, 3);
            AssertStats(created: 3, active: 0, inactive: 3);

            _pool.Prewarm(_prefab, 5);
            AssertStats(created: 5, active: 0, inactive: 5);
        }

        [Test]
        public void Prewarm_DoesNotCallOnSpawned()
        {
            _pool.Prewarm(_prefab, 1);

            GameObject instance = _host.GetComponentInChildren<PooledLifetime>(true).gameObject;
            Assert.IsFalse(instance.activeSelf);
            Assert.IsFalse(instance.GetComponent<PooledLifetime>().IsRunning);
        }

        [Test]
        public void GetGeneric_ReturnsComponentOfInstance()
        {
            var prefabLifetime = _prefab.GetComponent<PooledLifetime>();

            PooledLifetime first = _pool.Get(prefabLifetime, Vector3.zero, Quaternion.identity);
            _pool.Release(first.gameObject);
            PooledLifetime second = _pool.Get(prefabLifetime, Vector3.zero, Quaternion.identity);

            Assert.AreNotSame(prefabLifetime, first);
            Assert.AreSame(first, second);
            Assert.AreSame(first, first.gameObject.GetComponent<PooledLifetime>());
        }

        [Test]
        public void GetAndRelease_CallPoolableCallbacks()
        {
            var lifetime = _pool.Get(_prefab.GetComponent<PooledLifetime>(), Vector3.zero, Quaternion.identity);
            Assert.IsTrue(lifetime.IsRunning, "OnSpawned süreyi başlatmalı.");
            Assert.AreEqual(0f, lifetime.Progress);

            _pool.Release(lifetime.gameObject);
            Assert.IsFalse(lifetime.IsRunning, "OnDespawned süreyi durdurmalı.");
        }

        [Test]
        public void PooledLifetime_ReleasesInstance_WhenExpired()
        {
            ServiceLocator.Register<IPoolService>(_pool);
            var lifetime = _pool.Get(_prefab.GetComponent<PooledLifetime>(), Vector3.zero, Quaternion.identity);

            lifetime.Advance(lifetime.Lifetime * 0.5f);
            Assert.IsTrue(lifetime.gameObject.activeSelf);
            Assert.AreEqual(0.5f, lifetime.Progress, 0.001f);

            lifetime.Advance(lifetime.Lifetime);
            Assert.IsFalse(lifetime.gameObject.activeSelf);
            AssertStats(created: 1, active: 0, inactive: 1);
        }

        [Test]
        public void Release_Twice_WarnsAndIgnoresSecond()
        {
            GameObject instance = _pool.Get(_prefab, Vector3.zero, Quaternion.identity);
            _pool.Release(instance);

            LogAssert.Expect(LogType.Warning, new Regex("zaten havuzda"));
            _pool.Release(instance);

            AssertStats(created: 1, active: 0, inactive: 1);
        }

        [Test]
        public void Release_ObjectNotFromPool_DestroysIt()
        {
            var stranger = new GameObject("PoolServiceTests_Stranger");

            LogAssert.Expect(LogType.Warning, new Regex("havuzdan gelmedi"));
            _pool.Release(stranger);

            Assert.IsTrue(stranger == null);
        }

        [Test]
        public void Get_SkipsInstanceDestroyedOutsidePool()
        {
            GameObject destroyed = _pool.Get(_prefab, Vector3.zero, Quaternion.identity);
            _pool.Release(destroyed);
            Object.DestroyImmediate(destroyed);

            LogAssert.Expect(LogType.Warning, new Regex("havuz dışında yok edilmiş"));
            GameObject fresh = _pool.Get(_prefab, Vector3.zero, Quaternion.identity);

            Assert.IsTrue(fresh != null);
            Assert.IsTrue(fresh.activeSelf);
        }

        [Test]
        public void Get_NullPrefab_LogsErrorAndReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("prefab null"));
            Assert.IsNull(_pool.Get(null, Vector3.zero, Quaternion.identity));

            LogAssert.Expect(LogType.Error, new Regex("prefab null"));
            Assert.IsNull(_pool.Get((PooledLifetime)null, Vector3.zero, Quaternion.identity));
        }

        [Test]
        public void GetStats_UnknownPrefab_IsEmpty()
        {
            PoolStats stats = _pool.GetStats(_prefab);

            Assert.AreEqual(0, stats.Created);
            Assert.AreEqual(0, stats.Active);
            Assert.AreEqual(0, stats.Inactive);
        }

        private void AssertStats(int created, int active, int inactive)
        {
            PoolStats stats = _pool.GetStats(_prefab);
            Assert.AreEqual(created, stats.Created, "Created");
            Assert.AreEqual(active, stats.Active, "Active");
            Assert.AreEqual(inactive, stats.Inactive, "Inactive");
        }
    }
}
