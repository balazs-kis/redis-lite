using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedisLite.Client.Exceptions;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal abstract class BaseClient
    {
        private static readonly string ErrorPrefixString = RedisConstants.ErrorPrefix.ToString();
        private static readonly string StringPrefixString = RedisConstants.StringPrefix.ToString();
        private static readonly string IntegerPrefixString = RedisConstants.IntegerPrefix.ToString();
        private static readonly string BulkPrefixString = RedisConstants.BulkPrefix.ToString();
        private static readonly string ArrayPrefixString = RedisConstants.ArrayPrefix.ToString();

        protected bool IsResponseOk(object response)
        {
            return string.Equals(response.ToString(), RedisConstants.OkResult) ||
                   string.Equals(response.ToString(), RedisConstants.QueuedResult);
        }

        protected async Task<object[]> ParseToEndAsync(ISession session)
        {
            var firstLine = await ParseLineAsync(session);

            return IsArray(firstLine)
                ? await ParseArrayAsync(session, firstLine)
                : new object[] { await ParseSimpleContent(session, firstLine) };
        }

        protected async Task SendCommandAsync(ISession session, string command)
        {
            await session.StreamWriter.WriteAsync(command);
            await session.StreamWriter.FlushAsync();
        }

        protected async Task<object[]> SendCommandAndReadResponseAsync(ISession session, string command)
        {
            session.Locker.Obtain();

            try
            {
                await SendCommandAsync(session, command);
                return await ParseToEndAsync(session);
            }
            finally
            {
                session.Locker.Release();
            }
        }


        private async Task<object[]> ParseArrayAsync(ISession session, string firstLine)
        {
            if (!IsArray(firstLine))
            {
                throw new InvalidOperationException("Not an array type; the first line '{firstLine}' was not recognized");
            }

            if (string.Equals(firstLine, RedisConstants.NullList))
            {
                return null;
            }

            var result = new List<object>();
            var arrayLength = int.Parse(firstLine.TrimStart(RedisConstants.ArrayPrefix));

            for (var i = 0; i < arrayLength; i++)
            {
                var line = await ParseLineAsync(session);

                if (IsArray(line))
                {
                    var internalList = await ParseArrayAsync(session, line);

                    result.Add(internalList);
                }
                else
                {
                    result.Add(await ParseSimpleContent(session, line));
                }
            }

            return result.ToArray();
        }

        private async Task<string> ParseSimpleContent(ISession session, string firstLine)
        {
            if (IsError(firstLine))
            {
                throw new Exception(firstLine.TrimStart(RedisConstants.ErrorPrefix));
            }

            if (IsString(firstLine))
            {
                return firstLine.TrimStart(RedisConstants.StringPrefix);
            }

            if (IsInteger(firstLine))
            {
                return firstLine.TrimStart(RedisConstants.IntegerPrefix);
            }

            if (IsBulk(firstLine))
            {
                return string.Equals(firstLine, RedisConstants.NullBulk) ?
                     null :
                    await ParseBulkAsync(session, int.Parse(firstLine.TrimStart(RedisConstants.BulkPrefix)));
            }

            throw new InvalidOperationException($"Unknown type; the first line '{firstLine}' was not recognized");
        }

        private async Task<string> ParseLineAsync(ISession session)
        {
            var result = (await session.StreamReader.ReadLineAsync())?.TrimEnd();
            if (result == null)
                throw new RedisException("Stream has been closed", null);
            return result;
        }

        private async Task<string> ParseBulkAsync(ISession session, int length)
        {
            var fullLength = length + 2; // Add final \r\n
            var readVals = new char[fullLength];

            await session.StreamReader.ReadBlockAsync(readVals, 0, fullLength);

            return new string(readVals).TrimEnd();
        }


        private static bool IsError(string content)
        {
            return content.StartsWith(ErrorPrefixString);
        }

        private static bool IsString(string content)
        {
            return content.StartsWith(StringPrefixString);
        }

        private static bool IsInteger(string content)
        {
            return content.StartsWith(IntegerPrefixString);
        }

        private static bool IsBulk(string content)
        {
            return content.StartsWith(BulkPrefixString);
        }

        private static bool IsArray(string content)
        {
            return content.StartsWith(ArrayPrefixString);
        }
    }
}