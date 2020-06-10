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
    public class SubscriptionTester
    {
        private const string Channel = "TestChannel";
        private const string Message = "TestMessage";
        private const string SecondMessage = "SecondTestMessage";

        private const string OtherChannel = "OtherTestChannel";
        private const string OtherMessage = "OtherTestMessage";

        [TestMethod]
        public async Task Test_ConnectWithSubscriptionClient()
        {
            Exception thrownException = null;

            var dut = new AsyncRedisSubscriptionClient();

            try
            {
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
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

            var dut = new AsyncRedisSubscriptionClient();

            try
            {
                await dut.Connect(UnknownHost.AsConnectionSettings());
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

            var dut = new AsyncRedisSubscriptionClient();

            try
            {
                await dut.Connect(LocalHostPort7000.AsConnectionSettings());
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

            var dut = new AsyncRedisSubscriptionClient();

            try
            {
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
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

            var dut = new AsyncRedisSubscriptionClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
            dut.Dispose();

            try
            {
                await dut.Select(1);
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
            var dut = new AsyncRedisSubscriptionClient();

            try
            {
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                await dut.Select(1);
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
            var dut = new AsyncRedisSubscriptionClient();

            try
            {
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                await dut.Select(int.MaxValue);
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
            var dut = new AsyncRedisSubscriptionClient();

            try
            {
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                await dut.Subscribe(new string[] { });
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
            var dut = new AsyncRedisSubscriptionClient();

            try
            {
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                await dut.Subscribe("channel");
                await dut.Subscribe("other-channel");
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
            var dut = new AsyncRedisSubscriptionClient();

            try
            {
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                await dut.Subscribe("channel");
                await dut.Unsubscribe(new string[] { });
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
            var dut = new AsyncRedisSubscriptionClient();

            try
            {
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                await dut.Unsubscribe("channel");
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
            var dutPublisher = new AsyncRedisClient();
            var dutSubscriber = new AsyncRedisSubscriptionClient();

            await dutPublisher.Connect(LocalHostDefaultPort.AsConnectionSettings());
            await dutSubscriber.Connect(LocalHostDefaultPort.AsConnectionSettings());

            string receivedChannel = null;
            string receivedMessage = null;

            dutSubscriber.OnMessageReceived += (ch, msg) =>
            {
                receivedChannel = ch;
                receivedMessage = msg;
            };

            await dutSubscriber.Subscribe(Channel);

            await dutPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            Assert.AreEqual(Channel, receivedChannel);
            Assert.AreEqual(Message, receivedMessage);
        }

        [TestMethod]
        public async Task Test_Unsubscribe()
        {
            var dutPublisher = new AsyncRedisClient();
            var dutSubscriber = new AsyncRedisSubscriptionClient();

            await dutPublisher.Connect(LocalHostDefaultPort.AsConnectionSettings());
            await dutSubscriber.Connect(LocalHostDefaultPort.AsConnectionSettings());

            string receivedChannel = null;
            string receivedMessage = null;

            dutSubscriber.OnMessageReceived += (ch, msg) =>
            {
                receivedChannel = ch;
                receivedMessage = msg;
            };

            await dutSubscriber.Subscribe(Channel);

            await dutPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            await dutSubscriber.Unsubscribe(Channel);

            await dutPublisher.Publish(Channel, SecondMessage);
            Thread.Sleep(100);

            Assert.AreEqual(Channel, receivedChannel);
            Assert.AreEqual(Message, receivedMessage);
        }

        [TestMethod]
        public async Task Test_MultipleSubscribe()
        {
            var dutPublisher = new AsyncRedisClient();
            var dutSubscriber = new AsyncRedisSubscriptionClient();

            await dutPublisher.Connect(LocalHostDefaultPort.AsConnectionSettings());
            await dutSubscriber.Connect(LocalHostDefaultPort.AsConnectionSettings());

            var received = new List<Tuple<string, string>>();

            dutSubscriber.OnMessageReceived += (ch, msg) =>
            {
                received.Add(Tuple.Create(ch, msg));
            };

            await dutSubscriber.Subscribe(new[] { Channel, OtherChannel });

            await dutPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            await dutPublisher.Publish(OtherChannel, OtherMessage);
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
            var dutPublisher = new AsyncRedisClient();
            var dutSubscriber = new AsyncRedisSubscriptionClient();

            await dutPublisher.Connect(LocalHostDefaultPort.AsConnectionSettings());
            await dutSubscriber.Connect(LocalHostDefaultPort.AsConnectionSettings());

            var received = new List<Tuple<string, string>>();

            dutSubscriber.OnMessageReceived += (ch, msg) =>
            {
                received.Add(Tuple.Create(ch, msg));
            };

            await dutSubscriber.Subscribe(new[] { Channel, OtherChannel });

            await dutPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            await dutPublisher.Publish(OtherChannel, OtherMessage);
            Thread.Sleep(100);

            await dutSubscriber.Unsubscribe(new[] { Channel, OtherChannel });

            await dutPublisher.Publish(Channel, SecondMessage);
            Thread.Sleep(100);

            await dutPublisher.Publish(OtherChannel, SecondMessage);
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
            var dutPublisher = new AsyncRedisClient();
            var dutSubscriber = new AsyncRedisSubscriptionClient();

            await dutPublisher.Connect(LocalHostDefaultPort.AsConnectionSettings());
            await dutSubscriber.Connect(LocalHostDefaultPort.AsConnectionSettings());

            string receivedChannel = null;
            string receivedMessage = null;

            dutSubscriber.OnMessageReceived += (ch, msg) =>
            {
                receivedChannel = ch;
                receivedMessage = msg;
            };

            await dutSubscriber.Subscribe(Channel);
            Thread.Sleep(100);

            await dutSubscriber.Unsubscribe(Channel);
            Thread.Sleep(100);
            await dutSubscriber.Unsubscribe(Channel);
            Thread.Sleep(100);

            await dutPublisher.Publish(Channel, Message);
            Thread.Sleep(100);

            Assert.IsNull(receivedChannel);
            Assert.IsNull(receivedMessage);
        }
    }
}