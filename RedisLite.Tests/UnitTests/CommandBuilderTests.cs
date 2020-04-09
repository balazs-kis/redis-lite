using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client.CommandBuilders;
using RedisLite.Tests.TestHelpers;
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
            .Assert(result => result.Should().Contain(Key));

        [TestMethod]
        public void WithKeyCalledWithEmptyKey_ThrowsException() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => Test.ForException(() => underTest.WithKey(string.Empty)))
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<ArgumentException>();
            });

        [TestMethod]
        public void WithKeyCalledWithNullKey_ThrowsException() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => Test.ForException(() => underTest.WithKey(null)))
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<ArgumentException>();
            });

        [TestMethod]
        public void WithKeyCalledWithInvalidKey_ThrowsException() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => Test.ForException(() => underTest.WithKey("\t ")))
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<ArgumentException>();
            });

        [TestMethod]
        public void WithStringParameter_ResultContainsParameter() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithParameter(Parameter).ToString())
            .Assert(result => result.Should().Contain(Parameter));
        
        [TestMethod]
        public void WithObjectParameter_ResultContainsParameter() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithParameter(Parameter as object).ToString())
            .Assert(result => result.Should().Contain(Parameter));

        [TestMethod]
        public void WithStringParameters_ResultContainsParameters() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithParameters(Parameters).ToString())
            .Assert(result => result.Should().ContainAll(Parameters));

        [TestMethod]
        public void WithObjectParameters_ResultContainsParameters() => Test
            .Arrange(() => new BasicCommandBuilder(RedisCommands.GET))
            .Act(underTest => underTest.WithParameters(Parameters.Cast<object>()).ToString())
            .Assert(result => result.Should().ContainAll(Parameters));
    }
}