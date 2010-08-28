using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Printing;
using System.ComponentModel;
using WebKit.Interop;
using System.Drawing;
using System.Windows.Forms;

namespace WebKit
{
    internal class PrintManager
    {
        private PrintDocument _document;
        private IWebFramePrivate _webFramePrivate;
        private WebKitBrowser _owner;
        private Graphics _printGfx;
        private uint _nPages;
        private uint _page;
        private int _hDC;
        private bool _preview;
        private bool _printing = false;

        public PrintManager(PrintDocument Document, WebKitBrowser Owner, bool Preview)
        {
            this._document = Document;
            this._owner = Owner;
            this._webFramePrivate = 
                (IWebFramePrivate)((IWebView)_owner.GetWebView()).mainFrame();
            this._preview = Preview;
        }

        public void Print()
        {
            // potential concurrency issues with shared state variable
            if (_printing)
                return;
            _printing = true;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _document.PrintPage += new PrintPageEventHandler(_document_PrintPage);
            if (!_preview)
                _document.Print();

            _printing = false;
        }

        private delegate uint GetPrintedPageCountDelegate();

        private void _document_PrintPage(object sender, PrintPageEventArgs e)
        {
            // running on a seperate thread, so we invoke _webFramePrivate
            // methods on the owners ui thread
            
            if (_printGfx == null)
            {
                // initialise printing
                _printGfx = e.Graphics;
                _hDC = _printGfx.GetHdc().ToInt32();

                _owner.Invoke(new MethodInvoker(delegate() {
                    _webFramePrivate.setInPrintingMode(1, _hDC);
                }));

                _nPages = (uint)_owner.Invoke(
                    new GetPrintedPageCountDelegate(delegate() {
                    return _webFramePrivate.getPrintedPageCount(_hDC);
                }));

                _page = 1;
            }

            _owner.Invoke(new MethodInvoker(delegate() {
                _webFramePrivate.spoolPages(_hDC, _page, _page, IntPtr.Zero);
            }));

            ++_page;
            if (_page <= _nPages)
            {
                e.HasMorePages = true;
            }
            else
            {
                _owner.Invoke(new MethodInvoker(delegate() {
                    _webFramePrivate.setInPrintingMode(0, _hDC);
                }));
                e.HasMorePages = false;
                _printGfx = null;
                _nPages = 0;
            }
        }
    }
}
