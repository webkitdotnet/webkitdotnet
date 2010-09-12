using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WebKit.Interop;
using WebKit.JSCore;

namespace WebKit
{
    public class WebKitBrowserCore : IWebKitBrowser
    {
        // static variables
        private static ActivationContext activationContext;
        private static int actCtxRefCount;

        // private member variables...
        private IWebView webView;
        private IntPtr webViewHWND;
        private IWebKitBrowserHost host;

        // Note: we do not provide overridden Equals or GetHashCode methods for the
        // WebDownload interface used as a key here - the default implementations should suffice
        private Dictionary<WebDownload, WebKitDownload> downloads = new Dictionary<WebDownload, WebKitDownload>();
        private bool disposed = false;

        // initialisation and property stuff
        private string initialText = "";
        private Uri initialUrl = null;
        private bool loaded = false;    // loaded == true -> webView != null
        private bool initialAllowNavigation = true;
        private bool initialAllowDownloads = true;
        private bool initialAllowNewWindows = true;
        private bool initialJavaScriptEnabled = true;
        private bool _contextMenuEnabled = true;
        private readonly Version version = Assembly.GetExecutingAssembly().GetName().Version;
        private object _scriptObject = null;

        // delegates for WebKit events
        private WebFrameLoadDelegate frameLoadDelegate;
        private WebDownloadDelegate downloadDelegate;
        private WebPolicyDelegate policyDelegate;
        private WebUIDelegate uiDelegate;

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

        /// <summary>
        /// Occurs when JavaScript requests an alert panel to be displayed via the alert() function.
        /// </summary>
        public event ShowJavaScriptAlertPanelEventHandler ShowJavaScriptAlertPanel = delegate { };

        /// <summary>
        /// Occurs when JavaScript requests a confirm panel to be displayed via the confirm() function.
        /// </summary>
        public event ShowJavaScriptConfirmPanelEventHandler ShowJavaScriptConfirmPanel = delegate { };

        /// <summary>
        /// Occurs when JavaScript requests a prompt panel to be displayed via the prompt() function.
        /// </summary>
        public event ShowJavaScriptPromptPanelEventHandler ShowJavaScriptPromptPanel = delegate { };

        #endregion

        #region Public properties

        /// <summary>
        /// The current print page settings.
        /// </summary>
        public PageSettings PageSettings { get; set; }

        /// <summary>
        /// Gets the title of the current document.
        /// </summary>
        public string DocumentTitle { get; private set; }

        /// <summary>
        /// Gets or sets the current Url.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the context menu of the WebKitBrowser is enabled.
        /// </summary>
        public bool IsWebBrowserContextMenuEnabled
        {
            get { return _contextMenuEnabled; }
            set { _contextMenuEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether JavaScript is enabled.
        /// </summary>
        public bool IsScriptingEnabled
        {
            get
            {
                if (loaded)
                    return webView.preferences().isJavaScriptEnabled() != 0;
                else
                    return initialJavaScriptEnabled;
            }
            set
            {
                if (loaded)
                {
                    var prefs = webView.preferences();
                    prefs.setJavaScriptEnabled(value ? 1 : 0);
                    webView.setPreferences(prefs);
                }
                else
                {
                    initialJavaScriptEnabled = value;
                }
            }
        }

        /// <summary>
        /// Gets the host.
        /// </summary>
        /// <value>The host.</value>
        public IWebKitBrowserHost Host
        {
            get { return host; }
        }

        /// <summary>
        /// Gets the web view HWND.
        /// </summary>
        /// <value>The web view HWND.</value>
        public IntPtr WebViewHWND
        {
            get { return webViewHWND; }
        }

        /// <summary>
        /// Gets or sets an object that can be accessed by JavaScript contained within the WebKitBrowser control.
        /// </summary>
        /// <value>The object to be exposed to JavaScript.</value>
        public object ObjectForScripting
        {
            get { return _scriptObject; }
            set 
            { 
                _scriptObject = value; 
                CreateWindowScriptObject((JSContext)GetGlobalScriptContext()); 
            }
        }

        #endregion

        #region Constructors / initialization functions

        /// <summary>
        /// Initializes a new instance of the WebKitBrowser control.
        /// </summary>
        public WebKitBrowserCore()
        {
            PageSettings = new PageSettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebKitBrowserCore"/> class.
        /// </summary>
        /// <param name="webKitBrowserHost">The web kit browser host.</param>
        private WebKitBrowserCore(IWebKitBrowserHost webKitBrowserHost)
        {
            PageSettings = new PageSettings();
            Initialize(webKitBrowserHost);
        }

        /// <summary>
        /// Initializes the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        public void Initialize(IWebKitBrowserHost host)
        {
            if(host == null)
                throw new ArgumentNullException("host");

            this.host = host;

            if(!host.InDesignMode)
            {
                // Control Events            
                this.host.Load += new EventHandler(WebKitBrowser_Load);
                this.host.Resize += new EventHandler(WebKitBrowser_Resize);

                // If this is the first time the library has been loaded,
                // initialize the activation context required to load the
                // WebKit COM component registration free
                if((actCtxRefCount++) == 0)
                {
                    FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
                    activationContext = new ActivationContext(Path.Combine(fi.DirectoryName, "WebKitBrowser.dll.manifest"));
                    activationContext.Initialize();

                    // TODO: more error handling here

                    // Enable OLE for drag and drop functionality - WebKit
                    // will throw an OutOfMemory exception if we don't...
                    Application.OleRequired();
                }

                // If this control is brought to focus, focus our webkit child window
                this.host.GotFocus += (s, e) =>
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

            webView.setHostWindow(this.host.Handle.ToInt32());

            tagRECT rect = new tagRECT();
            rect.top = rect.left = 0;
            rect.bottom = this.host.Height - 1;
            rect.right = this.host.Width - 1;
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
            frameLoadDelegate.DidClearWindowObject += new DidClearWindowObjectEvent(frameLoadDelegate_DidClearWindowObject);

            // DownloadDelegate events
            downloadDelegate.DidReceiveResponse += new DidReceiveResponseEvent(downloadDelegate_DidReceiveResponse);
            downloadDelegate.DidReceiveDataOfLength += new DidReceiveDataOfLengthEvent(downloadDelegate_DidReceiveDataOfLength);
            downloadDelegate.DecideDestinationWithSuggestedFilename += new DecideDestinationWithSuggestedFilenameEvent(downloadDelegate_DecideDestinationWithSuggestedFilename);
            downloadDelegate.DidBegin += new DidBeginEvent(downloadDelegate_DidBegin);
            downloadDelegate.DidFinish += new DidFinishEvent(downloadDelegate_DidFinish);
            downloadDelegate.DidFailWithError += new DidFailWithErrorEvent(downloadDelegate_DidFailWithError);

            // UIDelegate events
            uiDelegate.CreateWebViewWithRequest += new CreateWebViewWithRequestEvent(uiDelegate_CreateWebViewWithRequest);
            uiDelegate.RunJavaScriptAlertPanelWithMessage += new RunJavaScriptAlertPanelWithMessageEvent(uiDelegate_RunJavaScriptAlertPanelWithMessage);
            uiDelegate.RunJavaScriptConfirmPanelWithMessage += new RunJavaScriptConfirmPanelWithMessageEvent(uiDelegate_RunJavaScriptConfirmPanelWithMessage);
            uiDelegate.RunJavaScriptTextInputPanelWithPrompt += new RunJavaScriptTextInputPanelWithPromptEvent(uiDelegate_RunJavaScriptTextInputPanelWithPrompt);

            activationContext.Deactivate();
        }

        #endregion

        #region Control event handers

        private void WebKitBrowser_Resize(object sender, EventArgs e)
        {
            // Resize the WebKit control
            NativeMethods.MoveWindow(webViewHWND, 0, 0, this.host.Width - 1, this.host.Height - 1, true);
        }

        private void WebKitBrowser_Load(object sender, EventArgs e)
        {
            // Create the WebKit browser component
            InitializeWebKit();

            loaded = webView != null;

            // intialize properties that depend on load
            if (initialUrl != null)
            {
                Navigate(initialUrl.AbsoluteUri);
            }
            else
            {
                DocumentText = initialText;
                policyDelegate.AllowInitialNavigation = false;
            }

            IsScriptingEnabled = initialJavaScriptEnabled;
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

        private void frameLoadDelegate_DidClearWindowObject(WebView WebView, IntPtr context, IntPtr windowScriptObject, webFrame frame)
        {
            CreateWindowScriptObject(new JSContext(context));
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
            // TODO: find out why url seems to always be empty:
            // https://bugs.webkit.org/show_bug.cgi?id=41441 explains all
            string url = (request == null) ? "" : request.url();
            NewWindowRequestEventArgs args = new NewWindowRequestEventArgs(url);
            NewWindowRequest(this, args);

            if (!args.Cancel)
            {
                WebKitBrowserCore b = new WebKitBrowserCore(host);
                webView = (WebView) b.webView;
                NewWindowCreated(this, new NewWindowCreatedEventArgs(b));
            }
            else
            {
                webView = null;
            }
        }

        private void uiDelegate_RunJavaScriptAlertPanelWithMessage(WebView sender, string message)
        {
            ShowJavaScriptAlertPanel(this, new ShowJavaScriptAlertPanelEventArgs(message));
        }

        private int uiDelegate_RunJavaScriptConfirmPanelWithMessage(WebView sender, string message)
        {
            var args = new ShowJavaScriptConfirmPanelEventArgs(message);
            ShowJavaScriptConfirmPanel(this, args);
            return args.ReturnValue ? 1 : 0;
        }

        private string uiDelegate_RunJavaScriptTextInputPanelWithPrompt(WebView sender, string message, string defaultText)
        {
            var args = new ShowJavaScriptPromptPanelEventArgs(message, defaultText);
            ShowJavaScriptPromptPanel(this, args);
            return args.ReturnValue;
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
            /* return webView.stringByEvaluatingJavaScriptFromString(Script); */

            // Instead of relying on the barely-implemented method in webkit above,
            // we can talk directly to JavaScriptCore via JSCore wrapper:

            JSValue val = ((JSContext)GetGlobalScriptContext()).EvaluateScript(Script);
            return val != null ? val.ToString() : "";
        }

        /// <summary>
        /// Gets the underlying WebKit WebView object used by this instance of WebKitBrowser.
        /// </summary>
        /// <returns>The WebView object.</returns>
        public object GetWebView()
        {
            return webView;
        }

        /// <summary>
        /// Gets the script context for the WebView.
        /// </summary>
        /// <returns>A JSCore.JSContext object representing the script context.</returns>
        public object GetGlobalScriptContext()
        {
            if (loaded)
                return new JSContext(webView.mainFrame());
            else
                return null;
        }

        // printing methods

        /// <summary>
        /// Prints the document using the current print and page settings.
        /// </summary>
        public void Print()
        {
            PrintDocument doc = new PrintDocument();
            doc.DocumentName = this.DocumentTitle;
            doc.DefaultPageSettings = PageSettings;
            doc.OriginAtMargins = true;
            PrintManager pm = new PrintManager(doc, this, false);
            pm.Print();
        }

        /// <summary>
        /// Displays a Page Setup dialog box with the current page and print settings.
        /// </summary>
        public void ShowPageSetupDialog()
        {
            PageSetupDialog pageSetupDlg = new PageSetupDialog();
            pageSetupDlg.EnableMetric = true;
            pageSetupDlg.PageSettings = this.PageSettings;
            pageSetupDlg.AllowPrinter = true;

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
            doc.DefaultPageSettings = PageSettings;
            doc.OriginAtMargins = true;
            printDlg.Document = doc;

            if (printDlg.ShowDialog() == DialogResult.OK)
            {
                PrintManager pm = new PrintManager(doc, this, false);
                pm.Print();
            }
        }

        /// <summary>
        /// Displays a Print Preview dialog box.
        /// </summary>
        public void ShowPrintPreviewDialog()
        {
            // TODO: find out why it apparently only shows the first page on the preview...
            PrintPreviewDialog printDlg = new PrintPreviewDialog();
            PrintDocument doc = new PrintDocument();
            doc.DocumentName = this.DocumentTitle;
            doc.DefaultPageSettings = PageSettings;
            doc.OriginAtMargins = true;
            printDlg.Document = doc;
            PrintManager pm = new PrintManager(doc, this, true);
            pm.Print();
            printDlg.ShowDialog();
        }

        #endregion Public Methods

        public void Dispose(bool disposing)
        {
            if(disposed)
                return;
            
            if((--actCtxRefCount) == 0 && activationContext != null)
            {
                activationContext.Dispose();
            }

            disposed = true;
        }

        private void CreateWindowScriptObject(JSContext context)
        {
            if (ObjectForScripting != null && context != null)
            {
                MessageBox.Show("Need to create object again!");
            }
        }
    }
}