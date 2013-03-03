using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebKitBrowser.Tests
{
    [TestClass]
    public class JavaScript
    {
        private static readonly TestHarness _testHarness;

        static JavaScript()
        {
            _testHarness = new TestHarness();
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
