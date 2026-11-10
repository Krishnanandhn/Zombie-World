using NUnit.Framework;
using Project.Core;

namespace Project.Tests
{
    public sealed class EventBusTests
    {
        struct TestEvent
        {
            public int Value;
        }

        [SetUp]
        public void SetUp() => EventBus.ClearAll();

        [Test]
        public void Publish_DeliversToSubscribers()
        {
            int received = 0;
            void Handler(TestEvent e) => received = e.Value;

            EventBus<TestEvent>.Subscribe(Handler);
            EventBus<TestEvent>.Publish(new TestEvent { Value = 7 });
            EventBus<TestEvent>.Unsubscribe(Handler);

            Assert.AreEqual(7, received);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            int calls = 0;
            void Handler(TestEvent e) => calls++;

            EventBus<TestEvent>.Subscribe(Handler);
            EventBus<TestEvent>.Unsubscribe(Handler);
            EventBus<TestEvent>.Publish(new TestEvent());

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void ClearAll_RemovesHandlers()
        {
            int calls = 0;
            EventBus<TestEvent>.Subscribe(_ => calls++);

            EventBus.ClearAll();
            EventBus<TestEvent>.Publish(new TestEvent());

            Assert.AreEqual(0, calls);
        }
    }
}
