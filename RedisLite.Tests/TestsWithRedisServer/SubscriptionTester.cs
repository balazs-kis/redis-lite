using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class SubscriptionTester : TestBase
    {
        private const string Channel = "TestChannel";
        private const string Message = "TestMessage";
        private const string SecondMessage = "SecondTestMessage";

        private const string OtherChannel = "OtherTestChannel";
        private const string OtherMessage = "OtherTestMessage";

        [ClassInitialize]
        public static async Task Setup(TestContext context) => await SetupTestContainerAsync();

        [ClassCleanup]
        public static async Task ClassCleanup() => await DisposeTestContainerAsync();

        [TestMethod]
        public async Task Test_ConnectWithSubscriptionClient()
        {
            Exception thrownException = null;

            var underTest = new AsyncRedisSubscriptionClient();

            try
            {
                await underTest.Connect(RedisConnectionSettings);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNull(thrownException);
        }

        [TestMethod]
        public async Task TestUnknownHost_ConnectWithSubscriptionClientThrowsException()
        {
            Exception thrownException = null;

            var underTest = new AsyncRedisSubscriptionClient();

            try
            {
                await underTest.Connect(UnknownHost.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(SocketException));
        }

        [TestMethod]
        public async Task TestKnownHostBadPort_ConnectWithSubscriptionClientThrowsException()
        {
            Exception thrownException = null;

            var underTest = new AsyncRedisSubscriptionClient();

            try
            {
                await underTest.Connect(LocalHostPort7000.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(SocketException));
        }

        [TestMethod]
        public async Task TestConnectTwice_ConnectWithSubscriptionClientThrowsException()
        {
            Exception thrownException = null;

            var underTest = new AsyncRedisSubscriptionClient();

            try
            {
                await underTest.Connect(RedisConnectionSettings);
                await underTest.Connect(RedisConnectionSettings);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public async Task TestCallingAfterDispose_SubscriptionClientThrowsException()
        {
            Exception thrownException = null;

            var underTest = new AsyncRedisSubscriptionClient();
            await underTest.Connect(RedisConnectionSettings);
            underTest.Dispose();

            try
            {
                await underTest.Select(1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public async Task Test_SelectWithSubscriptionClient()
        {
            Exception thrownException = null;
            var underTest = new AsyncRedisSubscriptionClient();

            try
            {
                await underTest.Connect(RedisConnectionSettings);
                await underTest.Select(1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNull(thrownException);
        }

        [TestMethod]
        public async Task TestWrongDbIndex_SelectWithSubscriptionClientThrowsException()
        {
            Exception thrownException = null;
            var underTest = new AsyncRedisSubscriptionClient();

            try
            {
                await underTest.Connect(RedisConnectionSettings);
                await underTest.Select(int.MaxValue);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public async Task TestEmptyChannelsList_SubscribeThrowsException()
        {
            Exception thrownException = null;
            var underTest = new AsyncRedisSubscriptionClient();

            try
            {
                await underTest.Connect(RedisConnectionSettings);
                await underTest.Subscribe(new string[] { });
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public async Task TestAlreadySubscribed_SubscribeThrowsException()
        {
            Exception thrownException = null;
            var underTest = new AsyncRedisSubscriptionClient();

            try
            {
                await underTest.Connect(RedisConnectionSettings);
                await underTest.Subscribe("channel");
                await underTest.Subscribe("other-channel");
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public async Task TestEmptyChannelsList_UnsubscribeThrowsException()
        {
            Exception thrownException = null;
            var underTest = new AsyncRedisSubscriptionClient();

            try
            {
                await underTest.Connect(RedisConnectionSettings);
                await underTest.Subscribe("channel");
                await underTest.Unsubscribe(new string[] { });
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public async Task TestNotSubscribed_UnsubscribeThrowsException()
        {
            Exception thrownException = null;
            var underTest = new AsyncRedisSubscriptionClient();

            try
            {
                await underTest.Connect(RedisConnectionSettings);
                await underTest.Unsubscribe("channel");
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public async Task Test_Subscribe()
        {
            var underTestPublisher = new AsyncRedisClient();
            var underTestSubscriber = new AsyncRedisSubscriptionClient();

            await underTestPublisher.Connect(RedisConnectionSettings);
            await underTestSubscriber.Connect(RedisConnectionSettings);

            string receivedChannel = null;
            string receivedMessage = null;

            underTestSubscriber.OnMessageReceived += (ch, msg) =>
            {
                receivedChannel = ch;
                receivedMessage = msg;
            };

            await underTestSubscriber.Subscribe(Channel);

            await underTestPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            Assert.AreEqual(Channel, receivedChannel);
            Assert.AreEqual(Message, receivedMessage);
        }

        [TestMethod]
        public async Task Test_Unsubscribe()
        {
            var underTestPublisher = new AsyncRedisClient();
            var underTestSubscriber = new AsyncRedisSubscriptionClient();

            await underTestPublisher.Connect(RedisConnectionSettings);
            await underTestSubscriber.Connect(RedisConnectionSettings);

            string receivedChannel = null;
            string receivedMessage = null;

            underTestSubscriber.OnMessageReceived += (ch, msg) =>
            {
                receivedChannel = ch;
                receivedMessage = msg;
            };

            await underTestSubscriber.Subscribe(Channel);

            await underTestPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            await underTestSubscriber.Unsubscribe(Channel);

            await underTestPublisher.Publish(Channel, SecondMessage);
            Thread.Sleep(100);

            Assert.AreEqual(Channel, receivedChannel);
            Assert.AreEqual(Message, receivedMessage);
        }

        [TestMethod]
        public async Task Test_MultipleSubscribe()
        {
            var underTestPublisher = new AsyncRedisClient();
            var underTestSubscriber = new AsyncRedisSubscriptionClient();

            await underTestPublisher.Connect(RedisConnectionSettings);
            await underTestSubscriber.Connect(RedisConnectionSettings);

            var received = new List<Tuple<string, string>>();

            underTestSubscriber.OnMessageReceived += (ch, msg) =>
            {
                received.Add(Tuple.Create(ch, msg));
            };

            await underTestSubscriber.Subscribe(new[] { Channel, OtherChannel });

            await underTestPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            await underTestPublisher.Publish(OtherChannel, OtherMessage);
            Thread.Sleep(100);

            Assert.AreEqual(2, received.Count);
            Assert.AreEqual(Channel, received[0].Item1);
            Assert.AreEqual(Message, received[0].Item2);
            Assert.AreEqual(OtherChannel, received[1].Item1);
            Assert.AreEqual(OtherMessage, received[1].Item2);
        }

        [TestMethod]
        public async Task Test_MultipleUnsubscribe()
        {
            var underTestPublisher = new AsyncRedisClient();
            var underTestSubscriber = new AsyncRedisSubscriptionClient();

            await underTestPublisher.Connect(RedisConnectionSettings);
            await underTestSubscriber.Connect(RedisConnectionSettings);

            var received = new List<Tuple<string, string>>();

            underTestSubscriber.OnMessageReceived += (ch, msg) =>
            {
                received.Add(Tuple.Create(ch, msg));
            };

            await underTestSubscriber.Subscribe(new[] { Channel, OtherChannel });

            await underTestPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            await underTestPublisher.Publish(OtherChannel, OtherMessage);
            Thread.Sleep(100);

            await underTestSubscriber.Unsubscribe(new[] { Channel, OtherChannel });

            await underTestPublisher.Publish(Channel, SecondMessage);
            Thread.Sleep(100);

            await underTestPublisher.Publish(OtherChannel, SecondMessage);
            Thread.Sleep(100);

            Assert.AreEqual(2, received.Count);
            Assert.AreEqual(Channel, received[0].Item1);
            Assert.AreEqual(Message, received[0].Item2);
            Assert.AreEqual(OtherChannel, received[1].Item1);
            Assert.AreEqual(OtherMessage, received[1].Item2);
        }

        [TestMethod]
        public async Task Test_RedundantUnsubscribe()
        {
            var underTestPublisher = new AsyncRedisClient();
            var underTestSubscriber = new AsyncRedisSubscriptionClient();

            await underTestPublisher.Connect(RedisConnectionSettings);
            await underTestSubscriber.Connect(RedisConnectionSettings);

            string receivedChannel = null;
            string receivedMessage = null;

            underTestSubscriber.OnMessageReceived += (ch, msg) =>
            {
                receivedChannel = ch;
                receivedMessage = msg;
            };

            await underTestSubscriber.Subscribe(Channel);
            Thread.Sleep(100);

            await underTestSubscriber.Unsubscribe(Channel);
            Thread.Sleep(100);
            await underTestSubscriber.Unsubscribe(Channel);
            Thread.Sleep(100);

            await underTestPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            Assert.IsNull(receivedChannel);
            Assert.IsNull(receivedMessage);
        }
    }
}