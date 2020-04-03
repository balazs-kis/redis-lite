using System;
using System.Collections.Generic;
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

        protected object[] ParseToEnd(ISession session)
        {
            var firstLine = ParseLine(session);

            return IsArray(firstLine)
                ? ParseArray(session, firstLine)
                : new object[] { ParseSimpleContent(session, firstLine) };
        }

        protected void SendCommand(ISession session, string command)
        {
            session.StreamWriter.Write(command);
            session.StreamWriter.Flush();
        }

        protected object[] SendCommandAndReadResponse(ISession session, string command)
        {
            return session.Locker.Execute(() =>
            {
                SendCommand(session, command);
                return ParseToEnd(session);
            });
        }


        private object[] ParseArray(ISession session, string firstLine)
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
                var line = ParseLine(session);

                if (IsArray(line))
                {
                    var internalList = ParseArray(session, line);

                    result.Add(internalList);
                }
                else
                {
                    result.Add(ParseSimpleContent(session, line));
                }
            }

            return result.ToArray();
        }

        private string ParseSimpleContent(ISession session, string firstLine)
        {
            if (IsError(firstLine))
            {
                return firstLine.TrimStart(RedisConstants.ErrorPrefix);
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
                    ParseBulk(session, int.Parse(firstLine.TrimStart(RedisConstants.BulkPrefix)));
            }

            throw new InvalidOperationException($"Unknown type; the first line '{firstLine}' was not recognized");
        }

        private string ParseLine(ISession session)
        {
            var result = session.StreamReader.ReadLine()?.TrimEnd();
            if (result == null)
                throw new RedisException("Stream has been closed", null);
            return result;
        }

        private string ParseBulk(ISession session, int length)
        {
            var fullLength = length + 2; // Add final \r\n
            var readVals = new char[fullLength];

            session.StreamReader.ReadBlock(readVals, 0, fullLength);
            
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