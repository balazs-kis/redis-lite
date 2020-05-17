using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client.CommandBuilders;
using TestLite;
using System;
using System.Linq;

namespace RedisLite.Tests.UnitTests
{
    [TestClass]
    public class CommandBuilderTests
    {
        private const string Key = "Key";
        private const string Parameter = "V1";
        private static readonly string[] Parameters = { "V1", "V2", "V3" };

        [TestMethod]
        public void WithKey_ResultContainsKey() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithKey(Key).ToString())
            .Assert().Validate(result => result.Should().Contain(Key));

        [TestMethod]
        public void WithKeyCalledWithEmptyKey_ThrowsException() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithKey(string.Empty))
            .Assert().ThrewException<ArgumentException>();

        [TestMethod]
        public void WithKeyCalledWithNullKey_ThrowsException() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithKey(null))
            .Assert().ThrewException<ArgumentException>();

        [TestMethod]
        public void WithKeyCalledWithInvalidKey_ThrowsException() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithKey("\t "))
            .Assert().ThrewException<ArgumentException>();

        [TestMethod]
        public void WithStringParameter_ResultContainsParameter() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithParameter(Parameter).ToString())
            .Assert().Validate(result => result.Should().Contain(Parameter));
        
        [TestMethod]
        public void WithObjectParameter_ResultContainsParameter() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithParameter(Parameter as object).ToString())
            .Assert().Validate(result => result.Should().Contain(Parameter));

        [TestMethod]
        public void WithStringParameters_ResultContainsParameters() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithParameters(Parameters).ToString())
            .Assert().Validate(result => result.Should().ContainAll(Parameters));

        [TestMethod]
        public void WithObjectParameters_ResultContainsParameters() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithParameters(Parameters.Cast<object>()).ToString())
            .Assert().Validate(result => result.Should().ContainAll(Parameters));
    }
}