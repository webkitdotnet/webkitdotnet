/*
 * Copyright (c) 2009, Peter Nelson (charn.opcode@gmail.com)
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * * Redistributions of source code must retain the above copyright notice, 
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice, 
 *   this list of conditions and the following disclaimer in the documentation 
 *   and/or other materials provided with the distribution.
 *   
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
*/

// TODO: dispose / finalize stuff
//       design time support for properties etc..

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WebKit;
using WebKit.Interop;
using System.Diagnostics;

namespace WebKit
{
    /// <summary>
    /// WebKit Browser Control.
    /// </summary>
    public partial class WebKitBrowser : UserControl
    {
        // static variables
        private static ActivationContext activationContext;
        private static int actCtxRefCount = 0;

        // private member variables...
        private IWebView webView;
        private IntPtr webViewHWND;
        private Dictionary<WebDownload, WebKitDownload> downloads = new Dictionary<WebDownload, WebKitDownload>();
        private string url = "";
        private bool disposed = false;

        // delegates for WebKit events
        private WebFrameLoadDelegate frameLoadDelegate;
        private WebDownloadDelegate downloadDelegate;
        private WebPolicyDelegate policyDelegate;
        private WebUIDelegate uiDelegate;
                
        // public events, roughly the same as in WebBrowser class
        // using the null object pattern to avoid null tests
        public event EventHandler DocumentTitleChanged = delegate { };
        public event WebBrowserDocumentCompletedEventHandler DocumentCompleted = delegate { };
        public event WebBrowserNavigatedEventHandler Navigated = delegate { };
        public event WebBrowserNavigatingEventHandler Navigating = delegate { };
        public event WebKitBrowserErrorEventHandler Error = delegate { };
        public event FileDownloadBeginEventHandler DownloadBegin = delegate { };

        // redirects relevant key strokes to the webkit control
        // bit more work is needed here perhaps
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = (Keys)msg.WParam.ToInt32();
            if (key == Keys.Left || key == Keys.Right || key == Keys.Up || 
                key == Keys.Down || key == Keys.Tab)
            {
                W32API.SendMessage(webViewHWND, msg.Msg, msg.WParam, msg.LParam);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region Public properties

        /// <summary>
        /// Gets the title of the current document.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DocumentTitle { get; private set; }

        /// <summary>
        /// Gets or sets the current Url.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Uri Url
        {
            get
            {
                if (webView != null)
                {
                    // TODO: find out what's supposed to happen here...
                    string url = webView.mainFrame().dataSource().request().url();
                    if (url == "")
                        return null;
                    return new Uri(webView.mainFrame().dataSource().request().url());
                }
                else
                    return new Uri(this.url);
            }
            set
            {
                if (webView != null)
                {
                    if (Url != null && LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                        Navigate(Url.AbsoluteUri);
                }
                else
                    url = value.AbsoluteUri;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a web page is currently being loaded.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBusy
        {
            get
            {
                if (webView != null)
                    return (webView.isLoading() > 0);
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets or sets the HTML content of the current document.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DocumentText
        {
            get
            {
                // TODO: more...
                try
                {
                    return webView.mainFrame().dataSource().representation().documentSource();
                }
                catch (Exception)
                {
                    return "";
                }
            }
            set
            {
                if (webView != null)
                    webView.mainFrame().loadHTMLString(value, null);
            }
        }

        /// <summary>
        /// Returns the currently selected text.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get
            {
                // TODO: error handling
                if (webView != null)
                    return webView.selectedText();
                else
                    return "";
            }
        }

        /// <summary>
        /// Gets or sets the application name for user agent.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ApplicationName
        {
            get
            {
                if (webView != null)
                    return webView.applicationNameForUserAgent();
                else
                    return "";
            }
            set
            {
                if (webView != null)
                    webView.setApplicationNameForUserAgent(value);
            }
        }

        /// <summary>
        /// Gets or sets the user agent.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UserAgent
        {
            get
            {
                if (webView != null)
                    return webView.userAgentForURL("");
                else
                    return "";
            }
            set
            {
                if (webView != null)
                    webView.setCustomUserAgent(value);
            }
        }

        /// <summary>
        /// Text size multiplier (1.0 is normal size).
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float TextSize
        {
            get
            {
                if (webView != null)
                    return webView.textSizeMultiplier();
                else
                    return 1.0f;
            }
            set
            {
                if (webView != null)
                    webView.setTextSizeMultiplier(value);
            }
        }

        public readonly string Version = "0.2.1";

        #endregion

        #region Constructors / initialization functions

        public WebKitBrowser()
        {
            InitializeComponent();
        
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                // Control Events            
                this.Load += new EventHandler(WebKitBrowser_Load);
                this.Resize += new EventHandler(WebKitBrowser_Resize);

                // If this is the first time the library has been loaded,
                // initialize the activation context required to load the
                // WebKit COM component registration free
                if ((actCtxRefCount++) == 0)
                {
                    activationContext = new ActivationContext("WebKitBrowser.dll.manifest");
                    activationContext.Initialize();

                    // TODO: more error handling here

                    // Enable OLE for drag and drop functionality - WebKit
                    // will throw an OutOfMemory exception if we don't...
                    Application.OleRequired();
                }

                // If this control is brought to focus, focus our webkit child window
                this.GotFocus += (s, e) =>
                {
                    W32API.SetFocus(webViewHWND);
                };            
            }
        }

        private void InitializeWebKit()
        {
            activationContext.Activate();

            webView = new WebViewClass();

            frameLoadDelegate = new WebFrameLoadDelegate();
            Marshal.AddRef(Marshal.GetIUnknownForObject(frameLoadDelegate));

            downloadDelegate = new WebDownloadDelegate();
            Marshal.AddRef(Marshal.GetIUnknownForObject(downloadDelegate));

            policyDelegate = new WebPolicyDelegate();
            Marshal.AddRef(Marshal.GetIUnknownForObject(policyDelegate));

            uiDelegate = new WebUIDelegate();
            Marshal.AddRef(Marshal.GetIUnknownForObject(uiDelegate));

            webView.setPolicyDelegate(policyDelegate);
            webView.setFrameLoadDelegate(frameLoadDelegate);
            webView.setDownloadDelegate(downloadDelegate);

            // haven't implemented this stuff yet
            // webView.setUIDelegate(uiDelegate);

            webView.setHostWindow(this.Handle.ToInt32());

            tagRECT rect = new tagRECT();
            rect.top = rect.left = 0;
            rect.bottom = this.Height - 1;
            rect.right = this.Width - 1;
            webView.initWithFrame(rect, null, null);

            IWebViewPrivate webViewPrivate = (IWebViewPrivate)webView;
            webViewHWND = (IntPtr) webViewPrivate.viewWindow();

            // Subscribe to FrameLoadDelegate events
            frameLoadDelegate.DidRecieveTitle += new DidRecieveTitleEvent(frameLoadDelegate_DidRecieveTitle);
            frameLoadDelegate.DidFinishLoadForFrame += new DidFinishLoadForFrameEvent(frameLoadDelegate_DidFinishLoadForFrame);
            frameLoadDelegate.DidStartProvisionalLoadForFrame += new DidStartProvisionalLoadForFrameEvent(frameLoadDelegate_DidStartProvisionalLoadForFrame);
            frameLoadDelegate.DidCommitLoadForFrame += new DidCommitLoadForFrameEvent(frameLoadDelegate_DidCommitLoadForFrame);
            frameLoadDelegate.DidFailLoadWithError += new DidFailLoadWithErrorEvent(frameLoadDelegate_DidFailLoadWithError);
            frameLoadDelegate.DidFailProvisionalLoadWithError += new DidFailProvisionalLoadWithErrorEvent(frameLoadDelegate_DidFailProvisionalLoadWithError);

            // DownloadDelegate events
            downloadDelegate.DidReceiveResponse += new DidReceiveResponseEvent(downloadDelegate_DidReceiveResponse);
            downloadDelegate.DidReceiveDataOfLength += new DidReceiveDataOfLengthEvent(downloadDelegate_DidReceiveDataOfLength);
            downloadDelegate.DecideDestinationWithSuggestedFilename += new DecideDestinationWithSuggestedFilenameEvent(downloadDelegate_DecideDestinationWithSuggestedFilename);
            downloadDelegate.DidBegin += new DidBeginEvent(downloadDelegate_DidBegin);
            downloadDelegate.DidFinish += new DidFinishEvent(downloadDelegate_DidFinish);
            downloadDelegate.DidFailWithError += new DidFailWithErrorEvent(downloadDelegate_DidFailWithError);

            activationContext.Deactivate();
        }

        #endregion

        #region Control event handers

        private void WebKitBrowser_Resize(object sender, EventArgs e)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                // Resize the WebKit control
                W32API.MoveWindow(webViewHWND, 0, 0, this.Width - 1, this.Height - 1, true);
            }
        }

        private void WebKitBrowser_Load(object sender, EventArgs e)
        {
            // Create the WebKit browser component
            InitializeWebKit();

            if (url != "")
                Navigate(url);
        }

        #endregion

        #region WebFrameLoadDelegate event handlers 

        private void frameLoadDelegate_DidCommitLoadForFrame(WebView WebView, IWebFrame frame)
        {
            if (frame == webView.mainFrame())
            {
                this.UseWaitCursor = true;
                Navigated(this, new WebBrowserNavigatedEventArgs(this.Url));
            }
        }

        private void frameLoadDelegate_DidStartProvisionalLoadForFrame(WebView WebView, IWebFrame frame)
        {
            if (frame == webView.mainFrame())
            {
                this.UseWaitCursor = true;
                Navigating(this, new WebBrowserNavigatingEventArgs(this.Url, frame.name()));
            }
        }

        private void frameLoadDelegate_DidFinishLoadForFrame(WebView WebView, IWebFrame frame)
        {
            if (frame == webView.mainFrame())
            {
                this.UseWaitCursor = false;
                DocumentCompleted(this, new WebBrowserDocumentCompletedEventArgs(this.Url));
            }
        }

        private void frameLoadDelegate_DidRecieveTitle(WebView WebView, string title, IWebFrame frame)
        {
            if (frame == webView.mainFrame())
            {
                DocumentTitle = title;
                DocumentTitleChanged(this, new EventArgs());
            }
        }

        private void frameLoadDelegate_DidFailProvisionalLoadWithError(WebView WebView, IWebError error, IWebFrame frame)
        {
            // ignore an "error" where the page loading is interrupted by a policy change when dowloading a file
            if (!(frame == WebView.mainFrame() && error.Domain() == "WebKitErrorDomain" && error.code() == 102))
            {
                Error(this, new WebKitBrowserErrorEventArgs(error.localizedDescription()));
            }
        }

        private void frameLoadDelegate_DidFailLoadWithError(WebView WebView, IWebError error, IWebFrame frame)
        {
            Error(this, new WebKitBrowserErrorEventArgs(error.localizedDescription())); 
        }

        #endregion

        #region WebDownloadDelegate event handlers

        private void downloadDelegate_DidFailWithError(WebDownload download, WebError error)
        {
            downloads[download].NotifyDidFailWithError(download, error);
        }

        private void downloadDelegate_DidFinish(WebDownload download)
        {
            downloads[download].NotifyDidFinish(download);
            // remove from list
            downloads.Remove(download);
        }

        private void downloadDelegate_DidBegin(WebDownload download)
        {
            // create WebKitDownload object to handle this download and notify listeners
            WebKitDownload d = new WebKitDownload(download);
            downloads.Add(download, d);
            DownloadBegin(this, new FileDownloadBeginEventArgs(d));
        }

        private void downloadDelegate_DecideDestinationWithSuggestedFilename(WebDownload download, string fileName)
        {
            downloads[download].NotifyDecideDestinationWithSuggestedFilename(download, fileName);
        }

        private void downloadDelegate_DidReceiveDataOfLength(WebDownload download, uint length)
        {
            // returns false if we cancelled the download at this point
            if (!downloads[download].NotifyDidReceiveDataOfLength(download, length))
                downloads.Remove(download);
        }

        private void downloadDelegate_DidReceiveResponse(WebDownload download, WebURLResponse response)
        {
            downloads[download].NotifyDidReceiveResponse(download, response);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Navigates to the specified Url.
        /// </summary>
        /// <param name="Url">Url to navigate to.</param>
        public void Navigate(string Url)
        {
            if (webView != null)
            {
                // prepend with "http://" if url not well formed
                if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
                    Url = "http://" + Url;

                activationContext.Activate();

                WebMutableURLRequest request = new WebMutableURLRequestClass();
                request.initWithURL(Url, _WebURLRequestCachePolicy.WebURLRequestUseProtocolCachePolicy, 60);
                request.setHTTPMethod("GET");

                webView.mainFrame().loadRequest(request);

                activationContext.Deactivate();
            }
            else
                url = Url;
        }

        /// <summary>
        /// Navigates to the previous page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        public bool GoBack()
        {
            // TODO: return value
            webView.goBack();
            return true;
        }

        /// <summary>
        /// Navigates to the next page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        public bool GoForward()
        {
            // TODO: return value
            webView.goForward();
            return true;
        }

        /// <summary>
        /// Refreshes the current web page.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            webView.mainFrame().reload();
        }

        /// <summary>
        /// Refreshes the current web page.
        /// </summary>
        /// <param name="Option">Options for refreshing the page.</param>
        public void Refresh(WebBrowserRefreshOption Option)
        {
            // TODO: implement
            Refresh();
        }

        /// <summary>
        /// Stops loading the current web page and any resources associated 
        /// with it.
        /// </summary>
        public void Stop()
        {
            if (webView.isLoading() != 0)
            {
                webView.mainFrame().stopLoading();
            }
        }

        #endregion Public Methods
    }
}
