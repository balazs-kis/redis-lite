using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client.CommandBuilders;
using System;
using System.Linq;

namespace RedisLite.Tests.UnitTests
{
    [TestClass]
    public class CommandBuilderTests
    {
        [TestMethod]
        public void Test_WithKey()
        {
            const string key = "KEY";

            var commandBuilder = new BasicCommandBuilder(RedisCommands.GET);
            commandBuilder.WithKey(key);
            var result = commandBuilder.ToString();

            Assert.IsTrue(result.Contains(key));
        }

        [TestMethod]
        public void TestEmptyKey_WithKeyThrowsException()
        {
            Exception thrownException = null;

            try
            {
                var commandBuilder = new BasicCommandBuilder(RedisCommands.GET);
                commandBuilder.WithKey(string.Empty);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(ArgumentException));
        }

        [TestMethod]
        public void TestNullKey_WithKeyThrowsException()
        {
            Exception thrownException = null;

            try
            {
                var commandBuilder = new BasicCommandBuilder(RedisCommands.GET);
                commandBuilder.WithKey(null);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(ArgumentException));
        }

        [TestMethod]
        public void TestInvalidKey_WithKeyThrowsException()
        {
            Exception thrownException = null;

            try
            {
                var commandBuilder = new BasicCommandBuilder(RedisCommands.GET);
                commandBuilder.WithKey("\t ");
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(ArgumentException));
        }

        [TestMethod]
        public void Test_WithStringParameter()
        {
            const string parameter = "V1";

            var commandBuilder = new BasicCommandBuilder(RedisCommands.GET);
            commandBuilder.WithParameter(parameter);
            var result = commandBuilder.ToString();

            Assert.IsTrue(result.Contains(parameter));
        }

        [TestMethod]
        public void Test_WithObjectParameter()
        {
            const string stringParameter = "V1";
            var parameter = stringParameter as object;

            var commandBuilder = new BasicCommandBuilder(RedisCommands.GET);
            commandBuilder.WithParameter(parameter);
            var result = commandBuilder.ToString();

            Assert.IsTrue(result.Contains(stringParameter));
        }

        [TestMethod]
        public void Test_WithStringParameters()
        {
            var parameters = new[] { "V1", "V2", "V3" };

            var commandBuilder = new BasicCommandBuilder(RedisCommands.GET);
            commandBuilder.WithParameters(parameters);
            var result = commandBuilder.ToString();

            foreach (var p in parameters)
            {
                Assert.IsTrue(result.Contains(p));
            }
        }

        [TestMethod]
        public void Test_WithObjectParameters()
        {
            var stringParameters = new[] { "V1", "V2", "V3" };
            var parameters = stringParameters.Cast<object>();

            var commandBuilder = new BasicCommandBuilder(RedisCommands.GET);
            commandBuilder.WithParameters(parameters);
            var result = commandBuilder.ToString();

            foreach (var p in stringParameters)
            {
                Assert.IsTrue(result.Contains(p));
            }
        }
    }
}