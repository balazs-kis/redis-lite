using RedisLite.Client;
using RedisLite.Client.Contracts;
using System;

namespace RedisLite.ConsoleTester
{
    public static class Program
    {
        private const string Key = "apple";
        private const string Value = "red";

        public static void Main()
        {
            var client = new RedisClient();
            var connectionSettings = new ConnectionSettings("127.0.0.1", 6379);

            FailIfException(() =>
            {
                Console.WriteLine($"Trying to connect to {connectionSettings.Address}:{connectionSettings.Port}...");
                client.Connect(connectionSettings);
                Pass();
            });

            FailIfException(() =>
            {
                Console.WriteLine("Trying simple write...");
                client.Set(Key, Value);
                Pass();
            });

            FailIfException(() =>
            {
                Console.WriteLine("Trying simple read...");
                var result = client.Get(Key);
                if (result == Value)
                {
                    Pass();
                }
                else
                {
                    Fail("Read incorrect value from the server");
                }
            });

            FailIfException(() =>
            {
                Console.WriteLine("Trying DbSize...");
                var result = client.DbSize();

                if (result == 1)
                {
                    Pass();
                }
                else
                {
                    Fail("Read incorrect db size from the server");
                }
            });

            FailIfException(() =>
            {
                Console.WriteLine("Trying delete...");
                client.Del(Key);
                Pass();
            });
        }

        private static void FailIfException(Action a)
        {
            try
            {
                a.Invoke();
            }
            catch (Exception ex)
            {
                Fail(ex);
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