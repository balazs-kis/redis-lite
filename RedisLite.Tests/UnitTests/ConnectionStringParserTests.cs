using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client.Contracts;
using RedisLite.Client.Utils;
using TestLite;

namespace RedisLite.Tests.UnitTests
{
    [TestClass]
    public class ConnectionStringParserTests
    {
        private const string Localhost = "localhost";
        private const string SslHost = "my.ssl-host.com";
        private const int CustomPort = 4320;
        private const string Password = "MyPassword1";
        private const int Timeout = 2500;

        [TestMethod]
        public void NullConnectionString_ThrowsException() => Test
            .Arrange(() => (string)null)
            .Act(ConnectionStringParser.ParseConnectionString)
            .Assert().ThrewException<ArgumentException>();
        
        [TestMethod]
        public void EmptyConnectionString_ThrowsException() => Test
            .Arrange(() => string.Empty)
            .Act(ConnectionStringParser.ParseConnectionString)
            .Assert().ThrewException<ArgumentException>();
        
        [TestMethod]
        public void InvalidConnectionString_ThrowsException() => Test
            .Arrange(() => "\t  ")
            .Act(ConnectionStringParser.ParseConnectionString)
            .Assert().ThrewException<ArgumentException>();
        
        [TestMethod]
        public void ConnectionStringIncludingHostOnly_ParsedAndRestIsFilledWithDefaultValues() => Test
            .Arrange(() => Localhost)
            .Act(ConnectionStringParser.ParseConnectionString)
            .Assert(r =>
            {
                var settingFromCtor = new ConnectionSettings(Localhost);
                r.Value.Should().BeEquivalentTo(settingFromCtor);
            });
        
        [TestMethod]
        public void ConnectionStringWithHostAndPort_ParsedAndRestIsFilledWithDefaultValues() => Test
            .Arrange(() => $"{Localhost}:{CustomPort}")
            .Act(ConnectionStringParser.ParseConnectionString)
            .Assert(r =>
            {
                var settingFromCtor = new ConnectionSettings(Localhost, CustomPort);
                r.Value.Should().BeEquivalentTo(settingFromCtor);
            });
        
        [TestMethod]
        public void ConnectionStringWithAllParameters_ParsedAllValuesCorrectly() => Test
            .Arrange(() => 
                $"{Localhost}:{CustomPort},password={Password},asynctimeout={Timeout},ssl=true,sslhost={SslHost},disableconcurrencycheck=true")
            .Act(ConnectionStringParser.ParseConnectionString)
            .Assert(r =>
            {
                var settingFromCtor = new ConnectionSettings(
                    Localhost,
                    CustomPort,
                    Password,
                    Timeout,
                    true,
                    SslOptions.UseSslWithServerName(SslHost));
                
                r.Value.Should().BeEquivalentTo(settingFromCtor);
            });
        
        [TestMethod]
        public void ConnectionStringWithCamelCaseParameterNames_ParsedAllValuesCorrectly() => Test
            .Arrange(() => 
                $"{Localhost}:{CustomPort},password={Password},asyncTimeout={Timeout},ssl=True,sslHost={SslHost},disableConcurrencyCheck=True")
            .Act(ConnectionStringParser.ParseConnectionString)
            .Assert(r =>
            {
                var settingFromCtor = new ConnectionSettings(
                    Localhost,
                    CustomPort,
                    Password,
                    Timeout,
                    true,
                    SslOptions.UseSslWithServerName(SslHost));
                
                r.Value.Should().BeEquivalentTo(settingFromCtor);
            });
    }
}