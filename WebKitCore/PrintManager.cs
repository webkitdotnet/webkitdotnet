using System;
using System.Drawing.Printing;
using System.ComponentModel;
using WebKit.Interop;
using System.Drawing;

namespace WebKit
{
    internal class PrintManager
    {
        private readonly PrintDocument _document;
        private readonly IWebFramePrivate _webFramePrivate;
        private readonly IWebKitBrowserHost _owner;
        private readonly IWebKitBrowser _browser;
        private Graphics _printGfx;
        private uint _nPages;
        private uint _page;
        private int _hDC;        
        private readonly bool _preview;
        private bool _printing;

        public PrintManager(PrintDocument Document, IWebKitBrowserHost Owner, IWebKitBrowser Browser, bool Preview)
        {
            this._document = Document;
            this._owner = Owner;
            this._browser = Browser;
            this._webFramePrivate =
                (IWebFramePrivate)((IWebView)_browser.GetWebView()).mainFrame();
            this._preview = Preview;
        }

        public void Print()
        {
            // potential concurrency issues with shared state variable
            if (_printing)
                return;
            _printing = true;

            var worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object Sender, DoWorkEventArgs Args)
        {
            _document.PrintPage += Document_PrintPage;
            if (!_preview)
                _document.Print();

            _printing = false;
        }

        private void Document_PrintPage(object Sender, PrintPageEventArgs Args)
        {
            // running on a seperate thread, so we invoke _webFramePrivate
            // methods on the owners ui thread

            if (_printGfx == null)
            {
                // initialise printing
                _printGfx = Args.Graphics;
                _hDC = _printGfx.GetHdc().ToInt32();

                OwnerInvoke(() => _webFramePrivate.setInPrintingMode(1, _hDC));

                _nPages = OwnerInvoke(() => _webFramePrivate.getPrintedPageCount(_hDC));

                _page = 1;
            } else {
                _printGfx = Args.Graphics;
                _hDC = _printGfx.GetHdc().ToInt32();
            }

            OwnerInvoke(() => _webFramePrivate.spoolPages(_hDC, _page, _page, IntPtr.Zero));

            ++_page;
            if (_page <= _nPages)
            {
                Args.HasMorePages = true;
            }
            else
            {
                OwnerInvoke(() => _webFramePrivate.setInPrintingMode(0, _hDC));
                Args.HasMorePages = false;
                _printGfx = null;
                _nPages = 0;
            }         
        }

        private delegate TResult Fn<out TResult>();
        private delegate void Fn();

        private TResult OwnerInvoke<TResult>(Fn<TResult> Method)
        {
            if (_owner.InvokeRequired)
                return (TResult)_owner.Invoke(Method);
            return Method();
        }

        private void OwnerInvoke(Fn Method)
        {
            if (_owner.InvokeRequired)
                _owner.Invoke(Method);
            else
                Method();
        }
    }
}
