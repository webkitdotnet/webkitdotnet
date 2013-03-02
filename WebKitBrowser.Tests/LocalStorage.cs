using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebKitBrowser.Tests
{
    [TestClass]
    public class LocalStorage
    {
        private Thread _thread;
        private WebKitBrowserTestForm _form;
        private AutoResetEvent _formClosedEvent;

        private void StartWebBrowser()
        {
            _formClosedEvent = new AutoResetEvent(false);
            var ready = new AutoResetEvent(false);
            _thread = new Thread(() => {
                _form = new WebKitBrowserTestForm();
                var localStorageDirPath = Path.Combine(Environment.CurrentDirectory, "localstorage");
                _form.Browser.LocalStorageDatabaseDirectory = localStorageDirPath;
                _form.Shown += (Sender, Args) => ready.Set();
                Application.Run(_form);
                _formClosedEvent.Set();
            });
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
            ready.WaitOne();
        }

        private void StopWebBrowser()
        {
            _form.Invoke(new Action(() => _form.Close()));
            _formClosedEvent.WaitOne();
            _thread.Abort();
            _thread.Join();
        }

        public LocalStorage()
        {
        }

        [TestMethod]
        public void TestLocalStorage()
        {
            var localStorageDirPath = Path.Combine(Environment.CurrentDirectory, "localstorage");
            Directory.Delete(localStorageDirPath, true);

            StartWebBrowser();
            var dir = Environment.CurrentDirectory;
            var filename = "file:///" + Uri.EscapeUriString(Path.Combine(dir, @"TestContent\LocalStorage.html").Replace('\\', '/'));

            var documentContent = "";
            var ready = new AutoResetEvent(false);
            _form.Invoke(new Action(() => {
                _form.Browser.DocumentCompleted += (Sender, Args) => {
                    documentContent = _form.Browser.Document.GetElementById("output").TextContent;
                    ready.Set();
                };
                _form.Browser.Navigate(filename);
            }));
            ready.WaitOne();
            StopWebBrowser();

            Assert.AreEqual("SUCCESS", documentContent);
        }

        [TestMethod]
        public void TestLocalStoragePersistence()
        {
            StartWebBrowser();
            var dir = Environment.CurrentDirectory;
            var filename = "file:///" + Uri.EscapeUriString(Path.Combine(dir, @"TestContent\LocalStoragePersistence.html").Replace('\\', '/'));

            var documentContent = "";
            var ready = new AutoResetEvent(false);
            _form.Invoke(new Action(() => {
                _form.Browser.DocumentCompleted += (Sender, Args) => {
                    documentContent = _form.Browser.Document.GetElementById("output").TextContent;
                    ready.Set();
                };
                _form.Browser.Error += (Sender, Args) => {
                    documentContent = Args.Description;
                    ready.Set();
                };
                _form.Browser.Navigate(filename);
            }));
            ready.WaitOne();
            StopWebBrowser();

            Assert.AreEqual("SUCCESS", documentContent);
        }
    }
}
