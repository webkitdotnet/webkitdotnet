using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebKit.Tests
{
    [TestClass]
    public class JavaScript
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
        public void TestScriptingEnabled()
        {
            _testHarness.InvokeOnBrowser((Browser) => {
                Browser.IsScriptingEnabled = true;
            });
            _testHarness.Test(@"TestContent\ScriptingEnabled.html");
        }

        [TestMethod]
        public void TestScriptingDisabled()
        {
            _testHarness.InvokeOnBrowser((Browser) => {
                Browser.IsScriptingEnabled = false;
            });
            _testHarness.Test(@"TestContent\ScriptingDisabled.html");
        }
    }
}
