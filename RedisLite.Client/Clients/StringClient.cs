using System;
using System.Threading.Tasks;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class StringClient : BaseClient
    {
        public async Task<Result> Ping(ISession session)
        {
            try
            {
                var command = new BasicCommandBuilder(RedisCommands.PING).ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return IsResponsePong(responseString) ? Result.Ok() : Result.Fail(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result> Set(ISession session, string key, string value)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SET)
                        .WithKey(key)
                        .WithParameter(value)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return IsResponseOk(responseString) ? Result.Ok() : Result.Fail(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result<string>> Get(ISession session, string key)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.GET)
                        .WithKey(key)
                        .ToString();

                var result = await SendCommandAndReadResponseAsync(session, command);

                return Result.Ok(result[0]?.ToString());
            }
            catch (Exception ex)
            {
                return Result.Fail<string>(ex.Message, ex);
            }
        }


        private static bool IsResponsePong(string response)
        {
            return string.Equals(response, RedisConstants.Pong);
        }
    }
}