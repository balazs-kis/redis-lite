using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.TestConfigurations;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class SubscriptionTester
    {
        private const string Channel = "TestChannel";
        private const string Message = "TestMessage";
        private const string SecondMessage = "SecondTestMessage";

        private const string OtherChannel = "OtherTestChannel";
        private const string OtherMessage = "OtherTestMessage";

        [TestMethod]
        public void Test_Subscribe()
        {
            var dutPublisher = new RedisClient();
            var dutSubscriber = new RedisSubscriptionClient();

            dutPublisher.Connect(LocalHostDefaultPort.AsConnectionSettings());
            dutSubscriber.Connect(LocalHostDefaultPort.AsConnectionSettings());

            string receivedChannel = null;
            string receivedMessage = null;

            dutSubscriber.OnMessageReceived += (ch, msg) =>
            {
                receivedChannel = ch;
                receivedMessage = msg;
            };

            dutSubscriber.Subscribe(Channel);

            dutPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            Assert.AreEqual(Channel, receivedChannel);
            Assert.AreEqual(Message, receivedMessage);
        }

        [TestMethod]
        public void Test_Unsubscribe()
        {
            var dutPublisher = new RedisClient();
            var dutSubscriber = new RedisSubscriptionClient();

            dutPublisher.Connect(LocalHostDefaultPort.AsConnectionSettings());
            dutSubscriber.Connect(LocalHostDefaultPort.AsConnectionSettings());

            string receivedChannel = null;
            string receivedMessage = null;

            dutSubscriber.OnMessageReceived += (ch, msg) =>
            {
                receivedChannel = ch;
                receivedMessage = msg;
            };

            dutSubscriber.Subscribe(Channel);

            dutPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            dutSubscriber.Unsubscribe(Channel);

            dutPublisher.Publish(Channel, SecondMessage);
            Thread.Sleep(100);

            Assert.AreEqual(Channel, receivedChannel);
            Assert.AreEqual(Message, receivedMessage);
        }

        [TestMethod]
        public void Test_MultipleSubscribe()
        {
            var dutPublisher = new RedisClient();
            var dutSubscriber = new RedisSubscriptionClient();

            dutPublisher.Connect(LocalHostDefaultPort.AsConnectionSettings());
            dutSubscriber.Connect(LocalHostDefaultPort.AsConnectionSettings());

            var received = new List<Tuple<string, string>>();

            dutSubscriber.OnMessageReceived += (ch, msg) =>
            {
                received.Add(Tuple.Create(ch, msg));
            };

            dutSubscriber.Subscribe(new[] { Channel, OtherChannel });

            dutPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            dutPublisher.Publish(OtherChannel, OtherMessage);
            Thread.Sleep(100);

            Assert.AreEqual(2, received.Count);
            Assert.AreEqual(Channel, received[0].Item1);
            Assert.AreEqual(Message, received[0].Item2);
            Assert.AreEqual(OtherChannel, received[1].Item1);
            Assert.AreEqual(OtherMessage, received[1].Item2);
        }

        [TestMethod]
        public void Test_MultipleUnsubscribe()
        {
            // TODO: find out why this fails sometimes on CI build

            var dutPublisher = new RedisClient();
            var dutSubscriber = new RedisSubscriptionClient();

            dutPublisher.Connect(LocalHostDefaultPort.AsConnectionSettings());
            dutSubscriber.Connect(LocalHostDefaultPort.AsConnectionSettings());

            var received = new List<Tuple<string, string>>();

            dutSubscriber.OnMessageReceived += (ch, msg) =>
            {
                received.Add(Tuple.Create(ch, msg));
            };

            dutSubscriber.Subscribe(new[] { Channel, OtherChannel });

            dutPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            dutPublisher.Publish(OtherChannel, OtherMessage);
            Thread.Sleep(100);

            dutSubscriber.Unsubscribe(new[] { Channel, OtherChannel });

            dutPublisher.Publish(Channel, SecondMessage);
            Thread.Sleep(100);

            dutPublisher.Publish(OtherChannel, SecondMessage);
            Thread.Sleep(100);

            Assert.AreEqual(2, received.Count);
            Assert.AreEqual(Channel, received[0].Item1);
            Assert.AreEqual(Message, received[0].Item2);
            Assert.AreEqual(OtherChannel, received[1].Item1);
            Assert.AreEqual(OtherMessage, received[1].Item2);
        }

        [TestMethod]
        public void Test_RedundantUnsubscribe()
        {
            var dutPublisher = new RedisClient();
            var dutSubscriber = new RedisSubscriptionClient();

            dutPublisher.Connect(LocalHostDefaultPort.AsConnectionSettings());
            dutSubscriber.Connect(LocalHostDefaultPort.AsConnectionSettings());

            string receivedChannel = null;
            string receivedMessage = null;

            dutSubscriber.OnMessageReceived += (ch, msg) =>
            {
                receivedChannel = ch;
                receivedMessage = msg;
            };

            dutSubscriber.Subscribe(Channel);
            Thread.Sleep(100);

            dutSubscriber.Unsubscribe(Channel);
            Thread.Sleep(100);
            dutSubscriber.Unsubscribe(Channel);
            Thread.Sleep(100);

            dutPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            Assert.IsNull(receivedChannel);
            Assert.IsNull(receivedMessage);
        }
    }
}