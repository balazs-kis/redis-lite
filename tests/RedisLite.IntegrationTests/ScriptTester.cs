namespace RedisLite.IntegrationTests
{
    [TestClass]
    public class ScriptTester : TestBase
    {
        private const int ShaLength = 40;

        private const string ScriptWithoutParameter = "redis.call('set','foo', 'it works') " +
                                                      "return redis.call('get','foo')";

        private const string ScriptWithListResult = "local t = {} " +
                                                    "table.insert(t, 'one') " +
                                                    "table.insert(t, 'two') " +
                                                    "return t";

        private const string ScriptWithListInListResult = "redis.call('select', 9) " +
                                                          "redis.call('del', 'tmp01') " +
                                                          "redis.call('rpush', 'tmp01', '10', '20', '30', '40') " +
                                                          "local p = redis.call('lrange', 'tmp01', 0, 100) " +
                                                          "local t = {} " +
                                                          "table.insert(t, p) " +
                                                          "table.insert(t, p) " +
                                                          "return t";

        [ClassInitialize]
        public static async Task Setup(TestContext context) => await SetupTestContainerAsync();

        [ClassCleanup]
        public static async Task ClassCleanup() => await DisposeTestContainerAsync();

        [TestMethod]
        public async Task TestScriptLoadAndRun_SingleString()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            var sha = await underTest.LoadScript(ScriptWithoutParameter);

            Assert.AreEqual(ShaLength, sha.Length);

            var res = (await underTest.EvalSha(sha, Array.Empty<string>())).ToList();
            var resString = res[0];

            Assert.AreEqual("it works", resString);
        }

        [TestMethod]
        public async Task TestScriptLoadAndRun_List()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            var sha = await underTest.LoadScript(ScriptWithListResult);

            Console.WriteLine(sha);

            var res = await underTest.EvalSha(sha, Array.Empty<string>());
            var resList = res as object[];

            Assert.AreEqual(2, resList?.Length);
            Assert.AreEqual("one", resList?[0] as string);
            Assert.AreEqual("two", resList?[1] as string);
        }

        [TestMethod]
        public async Task TestScriptLoadAndRun_ListInList()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            var sha = await underTest.LoadScript(ScriptWithListInListResult);

            Console.WriteLine(sha);

            var result = (await underTest.EvalSha(sha, Array.Empty<string>())).ToArray();

            Assert.AreEqual(2, result.Length);

            var embeddedArray1 = result[0] as object[];
            var embeddedArray2 = result[1] as object[];

            Assert.AreEqual("10", embeddedArray1?[0].ToString());
            Assert.AreEqual("20", embeddedArray1?[1].ToString());
            Assert.AreEqual("30", embeddedArray1?[2].ToString());
            Assert.AreEqual("40", embeddedArray1?[3].ToString());

            Assert.AreEqual("10", embeddedArray2?[0].ToString());
            Assert.AreEqual("20", embeddedArray2?[1].ToString());
            Assert.AreEqual("30", embeddedArray2?[2].ToString());
            Assert.AreEqual("40", embeddedArray2?[3].ToString());
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            try
            {
                var client = await CreateAndConnectRedisClientAsync();

                await client.Del("foo");

                await client.Select(9);
                await client.Del("tmp01");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}