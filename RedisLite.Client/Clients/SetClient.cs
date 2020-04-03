using System;
using System.Collections.Generic;
using System.Linq;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class SetClient : BaseClient
    {
        public Result<List<string>> SMembers(ISession session, string key)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SMEMBERS)
                        .WithKey(key)
                        .ToString();

                var result = SendCommandAndReadResponse(session, command);
                
                return Result.Ok(result.Select(i => i.ToString()).ToList());
            }
            catch (Exception ex)
            {
                return Result.Fail<List<string>>(ex.Message, ex);
            }
        }
    }
}