using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebKit.Tests
{
    [TestClass]
    public class Cookies
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
        public void TestCookieAcceptPolicyAlways()
        {
            // TODO: WebKit does not allow cookies to be set by file:// origin URLs.
            _testHarness.InvokeOnBrowser((Browser) => {
                Browser.CookieAcceptPolicy = CookieAcceptPolicy.Always;
            });
            _testHarness.Test(@"TestContent\CookieAcceptPolicyAlways.html");
        }

        [TestMethod]
        public void TestCookieAcceptPolicyNever()
        {
            _testHarness.InvokeOnBrowser((Browser) => {
                Browser.CookieAcceptPolicy = CookieAcceptPolicy.Never;
            });
            _testHarness.Test(@"TestContent\CookieAcceptPolicyNever.html");
        }
    }
}
