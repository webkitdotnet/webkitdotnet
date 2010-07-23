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
using WebKit.DOM;
using WebKit.Interop;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Drawing.Printing;

namespace WebKit
{
    /// <summary>
    /// WebKit Browser Control.
    /// </summary>
    public partial class WebKitBrowser : UserControl
    {
        // static variables
        private static ActivationContext activationContext;
        private static int actCtxRefCount;

        // private member variables...
        private IWebView webView;
        private IntPtr webViewHWND;
        private Dictionary<WebDownload, WebKitDownload> downloads = new Dictionary<WebDownload, WebKitDownload>();
        private bool disposed = false;

        // initialisation and property stuff
        private string initialText = "";
        private Uri initialUrl = null;
        private bool loaded = false;    // loaded == true -> webView != null
        private bool initialAllowNavigation = true;
        private bool initialAllowDownloads = true;
        private bool initialAllowNewWindows = true;
        private readonly Version version = Assembly.GetExecutingAssembly().GetName().Version;

        // delegates for WebKit events
        private WebFrameLoadDelegate frameLoadDelegate;
        private WebDownloadDelegate downloadDelegate;
        private WebPolicyDelegate policyDelegate;
        private WebUIDelegate uiDelegate;


        #region Overridden methods

        /// <summary>
        /// Processes a command key.  Overridden in WebKitBrowser to forward key events to the WebKit window.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns>Success value.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = (Keys)msg.WParam.ToInt32();
            if (key == Keys.Left || key == Keys.Right || key == Keys.Up || 
                key == Keys.Down || key == Keys.Tab)
            {
                NativeMethods.SendMessage(webViewHWND, (uint)msg.Msg, msg.WParam, msg.LParam);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region WebKitBrowser events

        // public events, roughly the same as in WebBrowser class
        // using the null object pattern to avoid null tests
        
        /// <summary>
        /// Occurs when the DocumentTitle property value changes.
        /// </summary>
        public event EventHandler DocumentTitleChanged = delegate { };

        /// <summary>
        /// Occurs when the WebKitBrowser control finishes loading a document.
        /// </summary>
        public event WebBrowserDocumentCompletedEventHandler DocumentCompleted = delegate { };

        /// <summary>
        /// Occurs when the WebKitBrowser control has navigated to a new document and has begun loading it.
        /// </summary>
        public event WebBrowserNavigatedEventHandler Navigated = delegate { };

        /// <summary>
        /// Occurs before the WebKitBrowser control navigates to a new document.
        /// </summary>
        public event WebBrowserNavigatingEventHandler Navigating = delegate { };

        /// <summary>
        /// Occurs when an error occurs on the current document, or when navigating to a new document.
        /// </summary>
        public event WebKitBrowserErrorEventHandler Error = delegate { };

        /// <summary>
        /// Occurs when the WebKitBrowser control begins a file download, before any data has been transferred.
        /// </summary>
        public event FileDownloadBeginEventHandler DownloadBegin = delegate { };

        /// <summary>
        /// Occurs when the WebKitBrowser control attempts to open a link in a new window.
        /// </summary>
        public event NewWindowRequestEventHandler NewWindowRequest = delegate { };

        /// <summary>
        /// Occurs when the WebKitBrowser control creates a new window.
        /// </summary>
        public event NewWindowCreatedEventHandler NewWindowCreated = delegate { };

        #endregion

        #region Public properties

        /// <summary>
        /// The current print page settings.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PageSettings PageSettings { get; set; }

        /// <summary>
        /// Gets the title of the current document.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DocumentTitle { get; private set; }

        /// <summary>
        /// Gets or sets the current Url.
        /// </summary>
        [Browsable(true), Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Specifies the Url to navigate to.")]
        public Uri Url
        {
            get
            {
                if (loaded)
                {
                    Uri result;
                    return Uri.TryCreate(webView.mainFrame().dataSource().request().url(), 
                        UriKind.Absolute, out result) ? result : null;
                }
                else
                {
                    return initialUrl;
                }
            }
            set
            {
                if (loaded)
                {
                    if (value != null)
                        Navigate(value.AbsoluteUri);
                }
                else
                {
                    initialUrl = value;
                }
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
                if (loaded)
                    return (webView.isLoading() > 0);
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets or sets the HTML content of the current document.
        /// </summary>
        [Browsable(true), DefaultValue(""), Category("Appearance")]
        [Description("The HTML content to be displayed if no Url is specified.")]
        public string DocumentText
        {
            get
            {
                if (loaded)
                {
                    try
                    {
                        return webView.mainFrame().dataSource().representation().documentSource();
                    }
                    catch (COMException)
                    {
                        return "";
                    }
                }
                else
                {
                    return initialText;
                }
            }
            set
            {
                if (loaded)
                    webView.mainFrame().loadHTMLString(value, null);
                else
                    initialText = value;
            }
        }

        /// <summary>
        /// Gets the currently selected text.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get
            {
                if (loaded)
                    return webView.selectedText();
                else
                    return "";
            }
        }

        /// <summary>
        /// Gets or sets the application name for the user agent.
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
        /// Gets or sets the user agent string.
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
        /// Gets or sets the text size multiplier (1.0 is normal size).
        /// </summary>
        [Browsable(true), DefaultValue(1.0f), Category("Appearance")]
        [Description("Specifies the text size multiplier.")]
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

        /// <summary>
        /// Gets or sets whether the control can navigate to another page 
        /// once it's initial page has loaded.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether the control can navigate" +
            " to another page once it's initial page has loaded.")]
        public bool AllowNavigation 
        {
            get
            {
                if (loaded)
                    return policyDelegate.AllowNavigation;
                else
                    return initialAllowNavigation;
            }
            set
            {
                if (loaded)
                    policyDelegate.AllowInitialNavigation = policyDelegate.AllowNavigation = value;
                else
                    initialAllowNavigation = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to allow file downloads.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether to allow file downloads.")]
        public bool AllowDownloads
        {
            get
            {
                if (loaded)
                    return policyDelegate.AllowDownloads;
                else
                    return initialAllowDownloads;
            }
            set
            {
                if (loaded)
                    policyDelegate.AllowDownloads = value;
                else
                    initialAllowDownloads = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to allow links to be opened in a new window.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether to allow links to be" +
            " opened in a new window.")]
        public bool AllowNewWindows
        {
            get
            {
                if (loaded)
                    return policyDelegate.AllowNewWindows;
                else
                    return initialAllowNewWindows;
            }
            set
            {
                if (loaded)
                    policyDelegate.AllowNewWindows = value;
                else
                    initialAllowNewWindows = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a previous page in the navigation history is available.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanGoBack
        {
            get
            {
                return loaded ? webView.backForwardList().backListCount() > 0 : false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a subsequent page in the navigation history is available.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanGoForward
        {
            get
            {
                return loaded ? webView.backForwardList().forwardListCount() > 0 : false;
            }
        }

        /// <summary>
        /// Gets a Document representing the currently displayed page.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DOM.Document Document
        {
            get
            {
                return DOM.Document.Create(webView.mainFrameDocument());
            }
        }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Version Version
        {
            get
            {
                return version;
            }
        }

        /// <summary>
        /// Gets or sets the scroll offset of the current page, in pixels from the origin.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point ScrollOffset
        {
            get
            {
                if (webView != null)
                {
                    IWebViewPrivate v = (IWebViewPrivate)webView;
                    return new Point(v.scrollOffset().x, v.scrollOffset().y);
                }
                else
                {
                    return Point.Empty;
                }
            }
            set
            {
                if (webView != null)
                {
                    IWebViewPrivate v = (IWebViewPrivate)webView;
                    tagPOINT p = new tagPOINT();
                    p.x = value.X - ScrollOffset.X;
                    p.y = value.Y - ScrollOffset.Y;
                    v.scrollBy(ref p);
                }
            }
        }

        /// <summary>
        /// Gets the visible content rectangle of the current view, in pixels.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle VisibleContent
        {
            get
            {
                if (webView != null)
                {
                    IWebViewPrivate v = (IWebViewPrivate)webView;
                    tagRECT r = v.visibleContentRect();
                    return new Rectangle(r.left, r.top, (r.right - r.left), (r.bottom - r.top));
                }
                else
                {
                    return Rectangle.Empty;
                }
            }
        }

        #endregion

        #region Constructors / initialization functions

        /// <summary>
        /// Initializes a new instance of the WebKitBrowser control.
        /// </summary>
        public WebKitBrowser()
        {
            InitializeComponent();

            PageSettings = new PageSettings();
            
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
                    FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    activationContext = new ActivationContext(Path.Combine(fi.DirectoryName, "WebKitBrowser.dll.manifest")); 
                    activationContext.Initialize();

                    // TODO: more error handling here

                    // Enable OLE for drag and drop functionality - WebKit
                    // will throw an OutOfMemory exception if we don't...
                    Application.OleRequired();
                }

                // If this control is brought to focus, focus our webkit child window
                this.GotFocus += (s, e) =>
                {
                    NativeMethods.SetFocus(webViewHWND);
                };            

                activationContext.Activate();
                webView = new WebViewClass();
                activationContext.Deactivate();            
            }
        }

        private void InitializeWebKit()
        {
            activationContext.Activate();

            frameLoadDelegate = new WebFrameLoadDelegate();
            Marshal.AddRef(Marshal.GetIUnknownForObject(frameLoadDelegate));

            downloadDelegate = new WebDownloadDelegate();
            Marshal.AddRef(Marshal.GetIUnknownForObject(downloadDelegate));

            policyDelegate = new WebPolicyDelegate(AllowNavigation, AllowDownloads, AllowNewWindows);
            Marshal.AddRef(Marshal.GetIUnknownForObject(policyDelegate));

            uiDelegate = new WebUIDelegate(this);
            Marshal.AddRef(Marshal.GetIUnknownForObject(uiDelegate));

            webView.setPolicyDelegate(policyDelegate);
            webView.setFrameLoadDelegate(frameLoadDelegate);
            webView.setDownloadDelegate(downloadDelegate);
            webView.setUIDelegate(uiDelegate);

            webView.setHostWindow(this.Handle.ToInt32());

            tagRECT rect = new tagRECT();
            rect.top = rect.left = 0;
            rect.bottom = this.Height - 1;
            rect.right = this.Width - 1;
            webView.initWithFrame(rect, null, null);

            IWebViewPrivate webViewPrivate = (IWebViewPrivate)webView;
            webViewHWND = (IntPtr)webViewPrivate.viewWindow();

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

            // UIDelegate events
            uiDelegate.CreateWebViewWithRequest += new CreateWebViewWithRequestEvent(uiDelegate_CreateWebViewWithRequest);

            activationContext.Deactivate();
        }

        #endregion

        #region Control event handers

        private void WebKitBrowser_Resize(object sender, EventArgs e)
        {
            // Resize the WebKit control
            NativeMethods.MoveWindow(webViewHWND, 0, 0, this.Width - 1, this.Height - 1, true);
        }

        private void WebKitBrowser_Load(object sender, EventArgs e)
        {
            // Create the WebKit browser component
            InitializeWebKit();

            loaded = webView != null;

            if (initialUrl != null)
            {
                Navigate(initialUrl.AbsoluteUri);
            }
            else
            {
                DocumentText = initialText;
                policyDelegate.AllowInitialNavigation = false;
            }
        }

        #endregion

        #region WebFrameLoadDelegate event handlers 

        private void frameLoadDelegate_DidCommitLoadForFrame(WebView WebView, IWebFrame frame)
        {
            if (frame == webView.mainFrame())
            {
                Navigated(this, new WebBrowserNavigatedEventArgs(this.Url));
            }
        }

        private void frameLoadDelegate_DidStartProvisionalLoadForFrame(WebView WebView, IWebFrame frame)
        {
            if (frame == webView.mainFrame())
            {
                string url = frame.provisionalDataSource().request().url();
                Navigating(this, new WebBrowserNavigatingEventArgs(new Uri(url), frame.name()));
            }
        }

        private void frameLoadDelegate_DidFinishLoadForFrame(WebView WebView, IWebFrame frame)
        {
            if (frame == webView.mainFrame())
            {
                policyDelegate.AllowInitialNavigation = policyDelegate.AllowNavigation;
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
            WebKitDownload d = new WebKitDownload();
            downloads.Add(download, d);

            FileDownloadBeginEventArgs args = new FileDownloadBeginEventArgs(d);
            DownloadBegin(this, args);

            if (args.Cancel)
                d.Cancel();
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

        #region WebUIDelegate event handlers

        private void uiDelegate_CreateWebViewWithRequest(IWebURLRequest request, out WebView webView)
        {
            // Todo: find out why url seems to always be empty
            string url = (request == null) ? "" : request.url();
            NewWindowRequestEventArgs args = new NewWindowRequestEventArgs(url);
            NewWindowRequest(this, args);

            if (!args.Cancel)
            {
                WebKitBrowser b = new WebKitBrowser();
                webView = (WebView) b.webView;
                NewWindowCreated(this, new NewWindowCreatedEventArgs(b));
            }
            else
            {
                webView = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Navigates to the specified Url.
        /// </summary>
        /// <param name="url">Url to navigate to.</param>
        public void Navigate(string url)
        {
            if (loaded)
            {
                // prepend with "http://" if url not well formed
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    url = "http://" + url;

                activationContext.Activate();

                WebMutableURLRequest request = new WebMutableURLRequestClass();
                request.initWithURL(url, _WebURLRequestCachePolicy.WebURLRequestUseProtocolCachePolicy, 60);
                request.setHTTPMethod("GET");

                webView.mainFrame().loadRequest(request);

                activationContext.Deactivate();
            }
            else
            {
                initialUrl = url.Length == 0 ? null : new Uri(url);
            }
        }

        /// <summary>
        /// Navigates to the previous page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        public bool GoBack()
        {
            bool retVal = CanGoBack;
            webView.goBack();
            return retVal;
        }

        /// <summary>
        /// Navigates to the next page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        public bool GoForward()
        {
            bool retVal = CanGoForward;
            webView.goForward();
            return retVal;
        }

        /// <summary>
        /// Reloads the current web page.
        /// </summary>
        public void Reload()
        {
            webView.mainFrame().reload();
        }

        /// <summary>
        /// Reloads the current web page.
        /// </summary>
        /// <param name="option">Options for reloading the page.</param>
        public void Reload(WebBrowserRefreshOption option)
        {
            // TODO: implement
            Reload();
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

        /// <summary>
        /// Returns the result of running a script.
        /// </summary>
        /// <param name="Script">The script to run.</param>
        /// <returns></returns>
        public string StringByEvaluatingJavaScriptFromString(string Script)
        {
            return webView.stringByEvaluatingJavaScriptFromString(Script);
        }

        /// <summary>
        /// Gets the underlying WebKit WebView object used by this instance of WebKitBrowser.
        /// </summary>
        /// <returns>The WebView object.</returns>
        public object GetWebView()
        {
            return webView;
        }

        // printing methods

        /// <summary>
        /// Prints the document using the current print and page settings.
        /// </summary>
        /// <remarks>UNIMPLEMENTED</remarks>
        public void Print()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Displays a Page Setup dialog box with the current page and print settings.
        /// </summary>
        public void ShowPageSetupDialog()
        {
            PageSetupDialog pageSetupDlg = new PageSetupDialog();
            pageSetupDlg.EnableMetric = true;
            pageSetupDlg.PageSettings = this.PageSettings;

            if (pageSetupDlg.ShowDialog() == DialogResult.OK)
                this.PageSettings = pageSetupDlg.PageSettings;
        }

        /// <summary>
        /// Displays a Print dialog box.
        /// </summary>
        public void ShowPrintDialog()
        {
            PrintDialog printDlg = new PrintDialog();
            PrintDocument doc = new PrintDocument();
            doc.DocumentName = this.DocumentTitle;
            doc.DefaultPageSettings.Margins = new Margins(500, 100, 100, 100);
            doc.OriginAtMargins = true;
            printDlg.Document = doc;

            if (printDlg.ShowDialog() == DialogResult.OK)
            {
                PrintManager pm = new PrintManager(doc, this);
                pm.Print();
            }
        }

        /// <summary>
        /// Displays a Print Preview dialog box.
        /// </summary>
        /// <remarks>UNIMPLEMENTED</remarks>
        public void ShowPrintPreviewDialog()
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}
