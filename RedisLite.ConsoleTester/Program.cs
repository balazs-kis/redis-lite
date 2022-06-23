using RedisLite.Client;
using RedisLite.Client.Contracts;
using System;
using System.Threading.Tasks;

namespace RedisLite.ConsoleTester
{
    public static class Program
    {
        private const string Key = "apple";
        private const string Value = "red";

        public static void Main()
        {
            var client = new AsyncRedisClient();

            var connectionSettings = ConnectionSettings.FromConnectionString(
                "127.0.0.1:6379,ssl=false,asyncTimeout=3500,disableConcurrencyCheck=false");

            FailIfException(async () =>
            {
                Console.WriteLine($"Trying to connect to {connectionSettings.Address}:{connectionSettings.Port}...");
                await client.Connect(connectionSettings);
                Pass();
            }).GetAwaiter().GetResult();

            FailIfException(async () =>
            {
                Console.WriteLine("Trying simple write...");
                await client.Set(Key, Value);
                Pass();
            }).GetAwaiter().GetResult();

            FailIfException(async () =>
            {
                Console.WriteLine("Trying simple read...");
                var result = await client.Get(Key);
                if (result == Value)
                {
                    Pass();
                }
                else
                {
                    Fail("Read incorrect value from the server");
                }
            }).GetAwaiter().GetResult();

            FailIfException(async () =>
            {
                Console.WriteLine("Trying DbSize...");
                var result = await client.DbSize();

                if (result == 1)
                {
                    Pass();
                }
                else
                {
                    Fail("Read incorrect db size from the server");
                }
            }).GetAwaiter().GetResult();

            FailIfException(async () =>
            {
                Console.WriteLine("Trying delete...");
                await client.Del(Key);
                Pass();
            }).GetAwaiter().GetResult();
        }

        private static Task FailIfException(Func<Task> a)
        {
            try
            {
                return a.Invoke();
            }
            catch (Exception ex)
            {
                Fail(ex);
                return Task.FromResult(false);
            }
        }

        private static void Pass()
        {
            var c = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"PASS{Environment.NewLine}");
            Console.ForegroundColor = c;
        }

        private static void Fail(Exception ex)
        {
            Fail($"{ex.GetType().Name}: {ex.Message}");
        }

        private static void Fail(string message)
        {
            var c = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"FAIL ({message}){Environment.NewLine}");
            Console.ForegroundColor = c;
        }
    }
}