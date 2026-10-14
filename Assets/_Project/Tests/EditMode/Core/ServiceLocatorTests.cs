using System;
using System.Text.RegularExpressions;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LastHaven.Tests.EditMode
{
    public class ServiceLocatorTests
    {
        private interface ITestService
        {
        }

        private sealed class TestService : ITestService
        {
        }

        [SetUp]
        [TearDown]
        public void ClearServices()
        {
            ServiceLocator.Clear();
        }

        [Test]
        public void Get_ReturnsRegisteredService()
        {
            var service = new TestService();
            ServiceLocator.Register<ITestService>(service);

            Assert.AreSame(service, ServiceLocator.Get<ITestService>());
        }

        [Test]
        public void Get_NotRegistered_ThrowsDescriptiveError()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => ServiceLocator.Get<ITestService>());

            StringAssert.Contains(nameof(ITestService), exception.Message);
        }

        [Test]
        public void TryGet_NotRegistered_ReturnsFalseAndNull()
        {
            bool found = ServiceLocator.TryGet(out ITestService service);

            Assert.IsFalse(found);
            Assert.IsNull(service);
        }

        [Test]
        public void TryGet_Registered_ReturnsTrueAndService()
        {
            var registered = new TestService();
            ServiceLocator.Register<ITestService>(registered);

            bool found = ServiceLocator.TryGet(out ITestService service);

            Assert.IsTrue(found);
            Assert.AreSame(registered, service);
        }

        [Test]
        public void Register_SameTypeTwice_WarnsAndReplaces()
        {
            var first = new TestService();
            var second = new TestService();
            ServiceLocator.Register<ITestService>(first);

            LogAssert.Expect(LogType.Warning, new Regex("zaten kayıtlı"));
            ServiceLocator.Register<ITestService>(second);

            Assert.AreSame(second, ServiceLocator.Get<ITestService>());
        }

        [Test]
        public void Register_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ServiceLocator.Register<ITestService>(null));
        }

        [Test]
        public void Unregister_RemovesService()
        {
            var service = new TestService();
            ServiceLocator.Register<ITestService>(service);

            ServiceLocator.Unregister<ITestService>(service);

            Assert.IsFalse(ServiceLocator.TryGet(out ITestService _));
        }

        [Test]
        public void Unregister_DifferentInstance_KeepsCurrentRegistration()
        {
            var current = new TestService();
            ServiceLocator.Register<ITestService>(current);

            ServiceLocator.Unregister<ITestService>(new TestService());

            Assert.AreSame(current, ServiceLocator.Get<ITestService>());
        }

        [Test]
        public void TryGet_DestroyedUnityObject_TreatedAsNotRegistered()
        {
            var gameObject = new GameObject("ServiceLocatorTest");
            var health = gameObject.AddComponent<Health>();
            ServiceLocator.Register<IDamageable>(health);

            UnityEngine.Object.DestroyImmediate(gameObject);

            Assert.IsFalse(ServiceLocator.TryGet(out IDamageable _));
        }
    }
}
