using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebKit.Tests
{
    class TestHarness
    {
        private Thread _thread;
        private WebKitBrowserTestForm _form;
        private readonly Action<WebKitBrowser> _webKitBrowserInit;

        public TestHarness()
            : this(Browser => { })
        {
        }

        public TestHarness(Action<WebKit.WebKitBrowser> WebKitBrowserInit)
        {
            _webKitBrowserInit = WebKitBrowserInit;
            StartWebBrowser();
        }

        private void StartWebBrowser()
        {
            var ready = new AutoResetEvent(false);
            _thread = new Thread(() => {
                _form = new WebKitBrowserTestForm();
                _webKitBrowserInit(_form.Browser);
                _form.Shown += (Sender, Args) => ready.Set();
                Application.Run(_form);
            });
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
            ready.WaitOne();
        }

        private void StopWebBrowser()
        {
            _form.Invoke(new Action(() => _form.Close()));
            _thread.Abort();
            _thread.Join();
        }

        public void Test(string TestFilePath)
        {
            var dir = Environment.CurrentDirectory;
            var filename = "file:///" + Uri.EscapeUriString(Path.Combine(dir, TestFilePath).Replace('\\', '/'));

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

            Assert.AreEqual("SUCCESS", documentContent);
        }

        public void Stop()
        {
            StopWebBrowser();
        }

        public void InvokeOnBrowser(Action<WebKit.WebKitBrowser> Action)
        {
            _form.Invoke(new Action(() => Action(_form.Browser)));
        }
    }
}
