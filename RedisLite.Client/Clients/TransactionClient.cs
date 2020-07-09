using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class TransactionClient : BaseClient
    {
        public async Task<Result> Watch(ISession session, IEnumerable<string> keys)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.WATCH)
                        .WithParameters(keys)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return string.Equals(responseString, RedisConstants.OkResult)
                    ? Result.Ok()
                    : Result.Fail(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result> Multi(ISession session)
        {
            try
            {
                var command = new BasicCommandBuilder(RedisCommands.MULTI).ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return string.Equals(responseString, RedisConstants.OkResult)
                    ? Result.Ok()
                    : Result.Fail(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result<object[]>> Exec(ISession session)
        {
            try
            {
                var command = new BasicCommandBuilder(RedisCommands.EXEC).ToString();

                var result = await SendCommandAndReadResponseAsync(session, command);

                return result == null
                    ? Result.Fail<object[]>(RedisConstants.TransactionAborted)
                    : Result.Ok(result);
            }
            catch (Exception ex)
            {
                return Result.Fail<object[]>(ex.Message, ex);
            }
        }

        public async Task<Result> Discard(ISession session)
        {
            try
            {
                var command = new BasicCommandBuilder(RedisCommands.DISCARD).ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return string.Equals(responseString, RedisConstants.OkResult)
                    ? Result.Ok()
                    : Result.Fail(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }
    }
}