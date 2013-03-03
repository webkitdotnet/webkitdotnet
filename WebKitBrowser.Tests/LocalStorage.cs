using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebKit.Tests
{
    [TestClass]
    public class LocalStorage
    {
        [TestMethod]
        public void TestLocalStorageDisabled()
        {
            var localStorageDirPath = Path.Combine(Environment.CurrentDirectory, "localstorage");
            
            var testHarness = new TestHarness(Browser => {
                Browser.LocalStorageDatabaseDirectory = localStorageDirPath;
                Browser.IsLocalStorageEnabled = false;
            });

            testHarness.Test(@"TestContent\LocalStorageDisabled.html");
            testHarness.Stop();
        }

        [TestMethod]
        public void TestLocalStorageEnabled()
        {
            var localStorageDirPath = Path.Combine(Environment.CurrentDirectory, "localstorage");
            
            if (Directory.Exists(localStorageDirPath))
                Directory.Delete(localStorageDirPath, true);

            var testHarness = new TestHarness(Browser => {
                Browser.LocalStorageDatabaseDirectory = localStorageDirPath;
                Browser.IsLocalStorageEnabled = true;
            });

            testHarness.Test(@"TestContent\LocalStorageEnabled.html");
            testHarness.Stop();
        }

        [TestMethod]
        public void TestLocalStoragePersistence()
        {
            // TODO: this needs to run in a separate process.
            // WebKit.dll is apparently not very thread safe.
            var localStorageDirPath = Path.Combine(Environment.CurrentDirectory, "localstorage");

            var testHarness = new TestHarness(Browser => {
                Browser.LocalStorageDatabaseDirectory = localStorageDirPath;
            });

            testHarness.Test(@"TestContent\LocalStoragePersistence.html");
            testHarness.Stop();
        }
    }
}
