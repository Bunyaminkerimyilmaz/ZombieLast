using System;
using System.Text.RegularExpressions;
using LastHaven.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LastHaven.Tests.EditMode
{
    public class EventBusTests
    {
        private readonly struct TestEvent : IEvent
        {
            public TestEvent(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct OtherEvent : IEvent
        {
        }

        [SetUp]
        [TearDown]
        public void ClearBuses()
        {
            EventBus<TestEvent>.Clear();
            EventBus<OtherEvent>.Clear();
        }

        [Test]
        public void Raise_CallsSubscriberWithPayload()
        {
            int received = 0;
            EventBus<TestEvent>.Subscribe(e => received = e.Value);

            EventBus<TestEvent>.Raise(new TestEvent(42));

            Assert.AreEqual(42, received);
        }

        [Test]
        public void Raise_WithoutSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => EventBus<TestEvent>.Raise(new TestEvent(1)));
        }

        [Test]
        public void Unsubscribe_StopsFurtherCalls()
        {
            int calls = 0;
            Action<TestEvent> handler = _ => calls++;
            EventBus<TestEvent>.Subscribe(handler);
            EventBus<TestEvent>.Raise(new TestEvent(1));

            EventBus<TestEvent>.Unsubscribe(handler);
            EventBus<TestEvent>.Raise(new TestEvent(2));

            Assert.AreEqual(1, calls);
            Assert.AreEqual(0, EventBus<TestEvent>.SubscriberCount);
        }

        [Test]
        public void Unsubscribe_NotSubscribedHandler_DoesNothing()
        {
            Assert.DoesNotThrow(() => EventBus<TestEvent>.Unsubscribe(_ => { }));
            Assert.DoesNotThrow(() => EventBus<TestEvent>.Unsubscribe(null));
        }

        [Test]
        public void Subscribe_SameHandlerTwice_CalledOnce()
        {
            int calls = 0;
            Action<TestEvent> handler = _ => calls++;
            EventBus<TestEvent>.Subscribe(handler);
            EventBus<TestEvent>.Subscribe(handler);

            EventBus<TestEvent>.Raise(new TestEvent(1));

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void Subscribe_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => EventBus<TestEvent>.Subscribe(null));
        }

        [Test]
        public void Raise_OnlyReachesSameEventType()
        {
            int otherCalls = 0;
            EventBus<OtherEvent>.Subscribe(_ => otherCalls++);

            EventBus<TestEvent>.Raise(new TestEvent(1));

            Assert.AreEqual(0, otherCalls);
        }

        [Test]
        public void Raise_SubscriberUnsubscribesItself_OthersStillCalledAndItIsNotCalledAgain()
        {
            int selfCalls = 0;
            int otherCalls = 0;
            Action<TestEvent> self = null;
            self = _ =>
            {
                selfCalls++;
                EventBus<TestEvent>.Unsubscribe(self);
            };
            EventBus<TestEvent>.Subscribe(self);
            EventBus<TestEvent>.Subscribe(_ => otherCalls++);

            EventBus<TestEvent>.Raise(new TestEvent(1));
            EventBus<TestEvent>.Raise(new TestEvent(2));

            Assert.AreEqual(1, selfCalls);
            Assert.AreEqual(2, otherCalls);
        }

        [Test]
        public void Raise_SubscriberUnsubscribesLaterSubscriber_LaterOneIsSkipped()
        {
            int laterCalls = 0;
            Action<TestEvent> later = _ => laterCalls++;
            EventBus<TestEvent>.Subscribe(_ => EventBus<TestEvent>.Unsubscribe(later));
            EventBus<TestEvent>.Subscribe(later);

            EventBus<TestEvent>.Raise(new TestEvent(1));

            Assert.AreEqual(0, laterCalls);
        }

        [Test]
        public void Raise_SubscribeDuringRaise_NewSubscriberCalledFromNextRaise()
        {
            int lateCalls = 0;
            Action<TestEvent> late = _ => lateCalls++;
            EventBus<TestEvent>.Subscribe(_ => EventBus<TestEvent>.Subscribe(late));

            EventBus<TestEvent>.Raise(new TestEvent(1));
            Assert.AreEqual(0, lateCalls);

            EventBus<TestEvent>.Raise(new TestEvent(2));
            Assert.AreEqual(1, lateCalls);
        }

        [Test]
        public void Raise_SubscriberThrows_ExceptionLoggedAndOthersStillCalled()
        {
            int otherCalls = 0;
            EventBus<TestEvent>.Subscribe(_ => throw new InvalidOperationException("dinleyici patladı"));
            EventBus<TestEvent>.Subscribe(_ => otherCalls++);

            LogAssert.Expect(LogType.Exception, new Regex("dinleyici patladı"));
            EventBus<TestEvent>.Raise(new TestEvent(1));

            Assert.AreEqual(1, otherCalls);
        }

        [Test]
        public void ResetAll_ClearsEveryUsedBus()
        {
            EventBus<TestEvent>.Subscribe(_ => { });
            EventBus<OtherEvent>.Subscribe(_ => { });

            EventBusRegistry.ResetAll();

            Assert.AreEqual(0, EventBus<TestEvent>.SubscriberCount);
            Assert.AreEqual(0, EventBus<OtherEvent>.SubscriberCount);
        }
    }
}
