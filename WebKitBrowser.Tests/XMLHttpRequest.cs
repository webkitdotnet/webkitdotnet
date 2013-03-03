using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebKit.Tests
{
    [TestClass]
    public class XMLHttpRequest
    {
        private static TestHarness _testHarness;

        [ClassInitialize]
        public static void Initialize(TestContext Context)
        {
            _testHarness = new TestHarness();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _testHarness.Stop();
        }

        [TestMethod]
        public void TestAllowLocalFiles()
        {
            _testHarness.InvokeOnBrowser((Browser) => {
                Browser.AllowFileAccessFromFileURLs = true;
            });
            _testHarness.Test(@"TestContent\XMLHttpRequestLocalFilesAllowed.html");
        }

        [TestMethod]
        public void TestDisallowLocalFiles()
        {
            _testHarness.InvokeOnBrowser((Browser) => {
                Browser.AllowFileAccessFromFileURLs = false;
            });
            _testHarness.Test(@"TestContent\XMLHttpRequestLocalFilesDisallowed.html");
        }
    }
}
