using System;
using System.Collections.Generic;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class TransactionClient : BaseClient
    {
        public Result Watch(ISession session, string key)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.WATCH)
                        .WithKey(key)
                        .ToString();

                var response = SendCommandAndReadResponse(session, command);
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

        public Result Watch(ISession session, IEnumerable<string> keys)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.WATCH)
                        .WithParameters(keys)
                        .ToString();

                var response = SendCommandAndReadResponse(session, command);
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

        public Result Multi(ISession session)
        {
            try
            {
                var command = new BasicCommandBuilder(RedisCommands.MULTI).ToString();

                var response = SendCommandAndReadResponse(session, command);
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

        public Result Exec(ISession session)
        {
            try
            {
                var command = new BasicCommandBuilder(RedisCommands.EXEC).ToString();

                var resultCode = SendCommandAndReadResponse(session, command);

                if (resultCode == null)
                {
                    return Result.Fail(RedisConstants.TransactionAborted);
                }

                return resultCode.Length >= 1 ? Result.Ok() : Result.Fail("Result was an empty array");
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public Result Discard(ISession session)
        {
            try
            {
                var command = new BasicCommandBuilder(RedisCommands.DISCARD).ToString();

                var response = SendCommandAndReadResponse(session, command);
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