using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class ListClient : BaseClient
    {
        public async Task<Result> RPush(ISession session, string key, params string[] values)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.RPUSH)
                        .WithKey(key)
                        .WithParameters(values)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return int.TryParse(responseString, out _) ||
                       string.Equals(responseString, RedisConstants.QueuedResult)
                    ? Result.Ok()
                    : Result.Fail(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result<List<string>>> LRange(ISession session, string key, int start, int stop)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.LRANGE)
                        .WithKey(key)
                        .WithParameter(start)
                        .WithParameter(stop)
                        .ToString();

                var result = await SendCommandAndReadResponseAsync(session, command);
                
                return Result.Ok(result.Select(i => i.ToString()).ToList());
            }
            catch (Exception ex)
            {
                return Result.Fail<List<string>>(ex.Message, ex);
            }
        }
    }
}