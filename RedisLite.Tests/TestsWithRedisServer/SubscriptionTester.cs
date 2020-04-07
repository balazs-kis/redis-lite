using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

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
        public void Test_ConnectWithSubscriptionClient()
        {
            Exception thrownException = null;

            var dut = new RedisSubscriptionClient();

            try
            {
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNull(thrownException);
        }

        [TestMethod]
        public void TestUnknownHost_ConnectWithSubscriptionClientThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisSubscriptionClient();

            try
            {
                dut.Connect(UnknownHost.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(SocketException));
        }

        [TestMethod]
        public void TestKnownHostBadPort_ConnectWithSubscriptionClientThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisSubscriptionClient();

            try
            {
                dut.Connect(LocalHostPort7000.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(SocketException));
        }

        [TestMethod]
        public void TestConnectTwice_ConnectWithSubscriptionClientThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisSubscriptionClient();

            try
            {
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void TestCallingAfterDispose_SubscriptionClientThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisSubscriptionClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
            dut.Dispose();

            try
            {
                dut.Select(1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void Test_SelectWithSubscriptionClient()
        {
            Exception thrownException = null;
            var dut = new RedisSubscriptionClient();

            try
            {
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                dut.Select(1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNull(thrownException);
        }

        [TestMethod]
        public void TestWrongDbIndex_SelectWithSubscriptionClientThrowsException()
        {
            Exception thrownException = null;
            var dut = new RedisSubscriptionClient();

            try
            {
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                dut.Select(int.MaxValue);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public void TestEmptyChannelsList_SubscribeThrowsException()
        {
            Exception thrownException = null;
            var dut = new RedisSubscriptionClient();

            try
            {
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                dut.Subscribe(new string[] { });
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void TestAlreadySubscribed_SubscribeThrowsException()
        {
            Exception thrownException = null;
            var dut = new RedisSubscriptionClient();

            try
            {
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                dut.Subscribe("channel");
                dut.Subscribe("other-channel");
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void TestEmptyChannelsList_UnsubscribeThrowsException()
        {
            Exception thrownException = null;
            var dut = new RedisSubscriptionClient();

            try
            {
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                dut.Subscribe("channel");
                dut.Unsubscribe(new string[] { });
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void TestNotSubscribed_UnsubscribeThrowsException()
        {
            Exception thrownException = null;
            var dut = new RedisSubscriptionClient();

            try
            {
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                dut.Unsubscribe("channel");
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

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