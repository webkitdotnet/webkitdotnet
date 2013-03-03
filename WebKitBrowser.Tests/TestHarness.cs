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
        private AutoResetEvent _scriptComplete;

        public TestHarness()
            : this(Browser => { })
        {
        }

        public TestHarness(Action<WebKit.WebKitBrowser> WebKitBrowserInit)
        {
            _webKitBrowserInit = WebKitBrowserInit;
            _scriptComplete = new AutoResetEvent(false);
            StartWebBrowser();
        }

        private void StartWebBrowser()
        {
            var ready = new AutoResetEvent(false);
            _thread = new Thread(() => {
                _form = new WebKitBrowserTestForm();
                _webKitBrowserInit(_form.Browser);
                _form.Browser.ObjectForScripting = this;
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

        public void Test(string TestFilePath, bool FinishedOnDocumentComplete = true)
        {
            var dir = Environment.CurrentDirectory;
            var filename = "file:///" + Uri.EscapeUriString(Path.Combine(dir, TestFilePath).Replace('\\', '/'));

            var documentContent = "";
            if (FinishedOnDocumentComplete)
            {
                var ready = new AutoResetEvent(false);
                _form.Invoke(new Action(() => {
                    _form.Browser.DocumentCompleted += (Sender, Args) => {
                        documentContent = _form.Browser.Document.GetElementById("output").TextContent;
                        ready.Set();
                    };
                    _form.Browser.Error += (Sender, Args) => {
                        documentContent = "ERROR " + Args.Description;
                        ready.Set();
                    };
                    _form.Browser.Navigate(filename);
                }));
                ready.WaitOne();
            }
            else
            {
                _form.Invoke(new Action(() => _form.Browser.Navigate(filename)));
                _scriptComplete.WaitOne();
                _form.Invoke(new Action(() => {
                    documentContent = _form.Browser.Document.GetElementById("output").TextContent;
                }));
            }

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

        public void Complete()
        {
            _scriptComplete.Set();
        }
    }
}
