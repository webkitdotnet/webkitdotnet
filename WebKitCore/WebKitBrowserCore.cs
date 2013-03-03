using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static ActivationContext _activationContext;
        private static int _actCtxRefCount;

        // private member variables...
        private IWebView _webView;
        private IntPtr _webViewHwnd;
        private IWebKitBrowserHost _webKitBrowserHost;
        private WebNotificationObserver _webNotificationObserver;
        private WebNotificationCenter _webNotificationCenter;

        // Note: we do not provide overridden Equals or GetHashCode methods for the
        // WebDownload interface used as a key here - the default implementations should suffice
        private readonly Dictionary<WebDownload, WebKitDownload> _downloads = new Dictionary<WebDownload, WebKitDownload>();
        private bool _disposed;

        // initialisation and property stuff
        private bool _loaded;    // loaded == true -> webView != null
        private string _initialText = "";
        private Uri _initialUrl;
        private bool _initialAllowNavigation = true;
        private bool _initialAllowDownloads = true;
        private bool _initialAllowNewWindows = true;
        private bool _initialJavaScriptEnabled = true;
        private bool _initialLocalStorageEnabled = true;
        private string _initialLocalStorageDatabaseDirectory = "";
        private bool _contextMenuEnabled = true;
        private readonly Version _version = Assembly.GetExecutingAssembly().GetName().Version;
        private object _scriptObject;

        // delegates for WebKit events
        private WebFrameLoadDelegate _frameLoadDelegate;
        private WebDownloadDelegate _downloadDelegate;
        private WebPolicyDelegate _policyDelegate;
        private WebUIDelegate _uiDelegate;

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
        /// Occures when WebKitBrowser control has begun to provide information on the download progress of a document it is navigating to.
        /// </summary>
        public event ProgressStartedEventHandler ProgressStarted = delegate { };

        /// <summary>
        /// Occures when WebKitBrowser control is no longer providing information on the download progress of a document it is navigating to.
        /// </summary>
        public event ProgressFinishedEventHandler ProgressFinished = delegate { };

        /// <summary>
        /// Occurs when the WebKitBrowser control has updated information on the download progress of a document it is navigating to.
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged = delegate { };

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

        private void SetIfLoaded<T>(T Value, ref T InitialValue, Action<T> Setter)
        {
            if (_loaded)
                Setter(Value);
            else
                InitialValue = Value;
        }

        private T GetIfLoaded<T>(T InitialValue, Func<T> Getter)
        {
            if (_loaded)
                return Getter();
            return InitialValue;
        }

        #region Public properties

        /// <summary>
        /// The HTTP Basic Authentication UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The HTTP Basic Authentication Password
        /// </summary>
        public string Password { private get; set; }

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
                return GetIfLoaded(_initialUrl, () => {
                    Uri result;
                    return Uri.TryCreate(_webView.mainFrame().dataSource().request().url(),
                                         UriKind.Absolute, out result) ? result : null;
                });
            }
            set
            {
                SetIfLoaded(value, ref _initialUrl, Uri => {
                    if (Uri != null)
                        Navigate(Uri.AbsoluteUri);
                });
            }
        }

        /// <summary>
        /// Gets a value indicating whether a web page is currently being loaded.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return GetIfLoaded(false, () => _webView.isLoading() > 0);
            }
        }

        /// <summary>
        /// Gets or sets the HTML content of the current document.
        /// </summary>
        public string DocumentText
        {
            get
            {
                return GetIfLoaded(_initialText, () => {
                    try
                    {
                        return _webView.mainFrame().dataSource().representation().documentSource();
                    }
                    catch (COMException)
                    {
                        return "";
                    }
                });
            }
            set
            {
                SetIfLoaded(value, ref _initialText,
                                Text => _webView.mainFrame().loadHTMLString(Text, null));
            }
        }

        /// <summary>
        /// Gets the currently selected text.
        /// </summary>
        public string SelectedText
        {
            get
            {
                return GetIfLoaded("", () => _webView.selectedText());
            }
        }

        /// <summary>
        /// Gets or sets the application name for the user agent.
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return _webView != null ? _webView.applicationNameForUserAgent() : "";
            }
            set
            {
                if (_webView != null)
                    _webView.setApplicationNameForUserAgent(value);
            }
        }

        /// <summary>
        /// Gets or sets the user agent string.
        /// </summary>
        public string UserAgent
        {
            get
            {
                return _webView != null ? _webView.userAgentForURL("") : "";
            }
            set
            {
                if (_webView != null)
                    _webView.setCustomUserAgent(value);
            }
        }

        /// <summary>
        /// Gets or sets the text size multiplier (1.0 is normal size).
        /// </summary>
        public float TextSize
        {
            get
            {
                return _webView != null ? _webView.textSizeMultiplier() : 1.0f;
            }
            set
            {
                if (_webView != null)
                    _webView.setTextSizeMultiplier(value);
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
                return GetIfLoaded(_initialAllowNavigation, () => _policyDelegate.AllowNavigation);
            }
            set
            {
                SetIfLoaded(value, ref _initialAllowNavigation,
                                B => _policyDelegate.AllowInitialNavigation = _policyDelegate.AllowNavigation = B);
            }
        }

        /// <summary>
        /// Gets or sets whether to allow file downloads.
        /// </summary>
        public bool AllowDownloads
        {
            get
            {
                return GetIfLoaded(_initialAllowDownloads, () => _policyDelegate.AllowDownloads);
            }
            set
            {
                SetIfLoaded(value, ref _initialAllowDownloads,
                                B => _policyDelegate.AllowDownloads = _policyDelegate.AllowDownloads = B);
            }
        }

        /// <summary>
        /// Gets or sets whether to allow links to be opened in a new window.
        /// </summary>
        public bool AllowNewWindows
        {
            get
            {
                return GetIfLoaded(_initialAllowNewWindows, () => _policyDelegate.AllowNewWindows);
            }
            set
            {
                SetIfLoaded(value, ref _initialAllowNewWindows,
                                B => _policyDelegate.AllowNewWindows = _policyDelegate.AllowNewWindows = B);
            }
        }

        /// <summary>
        /// Gets a value indicating whether a previous page in the navigation history is available.
        /// </summary>
        public bool CanGoBack
        {
            get
            {
                return GetIfLoaded(false, () => _webView.backForwardList().backListCount() > 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether a subsequent page in the navigation history is available.
        /// </summary>
        public bool CanGoForward
        {
            get
            {
                return GetIfLoaded(false, () => _webView.backForwardList().forwardListCount() > 0);
            }
        }

        /// <summary>
        /// Gets a Document representing the currently displayed page.
        /// </summary>
        public DOM.Document Document
        {
            get
            {
                return DOM.Document.Create(_webView.mainFrameDocument());
            }
        }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        public Version Version
        {
            get
            {
                return _version;
            }
        }

        /// <summary>
        /// Gets or sets the scroll offset of the current page, in pixels from the origin.
        /// </summary>
        public Point ScrollOffset
        {
            get
            {
                if (_webView == null)
                    return Point.Empty;
                var v = (IWebViewPrivate) _webView;
                return new Point(v.scrollOffset().x, v.scrollOffset().y);
            }
            set
            {
                if (_webView == null)
                    return;
                var v = (IWebViewPrivate) _webView;
                var p = new tagPOINT();
                p.x = value.X - ScrollOffset.X;
                p.y = value.Y - ScrollOffset.Y;
                v.scrollBy(ref p);
            }
        }

        /// <summary>
        /// Gets the visible content rectangle of the current view, in pixels.
        /// </summary>
        public Rectangle VisibleContent
        {
            get
            {
                if (_webView == null)
                    return Rectangle.Empty;
                var v = (IWebViewPrivate)_webView;
                var r = v.visibleContentRect();
                return new Rectangle(r.left, r.top, (r.right - r.left), (r.bottom - r.top));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the context menu of the WebKitBrowser is enabled.
        /// </summary>
        public bool WebBrowserContextMenuEnabled
        {
            get { return _contextMenuEnabled; }
            set { _contextMenuEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether JavaScript is enabled.
        /// </summary>
        public bool ScriptingEnabled {
          get
          {
              return GetIfLoaded(_initialJavaScriptEnabled, () => _webView.preferences().isJavaScriptEnabled() != 0);
          }
          set 
          {
              SetIfLoaded(value, ref _initialJavaScriptEnabled, B => {
                  var prefs = _webView.preferences();
                  prefs.setJavaScriptEnabled(B ? 1 : 0);
                  _webView.setPreferences(prefs);
              });
          }
        }

        /// <summary>
        /// Gets or sets a value indicating whether LocalStorage is enabled.
        /// </summary>
        public bool LocalStorageEnabled
        {
            get
            {
                return GetIfLoaded(_initialLocalStorageEnabled,
                                       () => ((IWebPreferencesPrivate) _webView.preferences()).localStorageEnabled() != 0);
            }
            set
            {
                SetIfLoaded(value, ref _initialLocalStorageEnabled,
                                B => ((IWebPreferencesPrivate) _webView.preferences()).setLocalStorageEnabled(B ? 1 : 0));
            }
        }

        /// <summary>
        /// Gets or sets the fully qualified path to the directory where 
        /// local storage database files will be stored.
        /// </summary>
        /// <remarks>Value must be a fully qualified directory path.</remarks>
        public string LocalStorageDatabaseDirectory
        {
            get
            {
                return GetIfLoaded(_initialLocalStorageDatabaseDirectory,
                                () => ((IWebPreferencesPrivate) _webView.preferences()).localStorageDatabasePath());
            }
            set
            {
                SetIfLoaded(value, ref _initialLocalStorageDatabaseDirectory,
                                B => {
                                    if (!string.IsNullOrEmpty(B))
                                        ((IWebPreferencesPrivate) _webView.preferences()).setLocalStorageDatabasePath(B);
                                });
            }
        }

        /// <summary>
        /// Gets the host.
        /// </summary>
        /// <value>The host.</value>
        public IWebKitBrowserHost Host
        {
            get { return _webKitBrowserHost; }
        }

        /// <summary>
        /// Gets the web view HWND.
        /// </summary>
        /// <value>The web view HWND.</value>
        public IntPtr WebViewHWND
        {
            get { return _webViewHwnd; }
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
        /// <param name="WebKitBrowserHost">The web kit browser host.</param>
        private WebKitBrowserCore(IWebKitBrowserHost WebKitBrowserHost)
        {
            PageSettings = new PageSettings();
            Initialize(WebKitBrowserHost);
        }

        /// <summary>
        /// Initializes the specified host.
        /// </summary>
        /// <param name="WebKitBrowserHost">The host.</param>
        public void Initialize(IWebKitBrowserHost WebKitBrowserHost)
        {
            if (WebKitBrowserHost == null)
                throw new ArgumentNullException("WebKitBrowserHost");

            this._webKitBrowserHost = WebKitBrowserHost;

            if(!WebKitBrowserHost.InDesignMode)
            {
                // Control Events            
                this._webKitBrowserHost.Load += WebKitBrowser_Load;
                this._webKitBrowserHost.Resize += WebKitBrowser_Resize;

                // If this is the first time the library has been loaded,
                // initialize the activation context required to load the
                // WebKit COM component registration free
                if((_actCtxRefCount++) == 0)
                {
                    var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
                    _activationContext = new ActivationContext(Path.Combine(fi.DirectoryName, "WebKitBrowser.dll.manifest"));
                    _activationContext.Initialize();

                    // TODO: more error handling here

                    // Enable OLE for drag and drop functionality - WebKit
                    // will throw an OutOfMemory exception if we don't...
                    Application.OleRequired();
                }

                // If this control is brought to focus, focus our webkit child window
                this._webKitBrowserHost.GotFocus += (_, __) => NativeMethods.SetFocus(_webViewHwnd);

                _activationContext.Activate();
                _webView = new WebViewClass();
                _activationContext.Deactivate();
            }
        }

        private void InitializeWebKit()
        {
            _activationContext.Activate();

            _frameLoadDelegate = new WebFrameLoadDelegate();
            Marshal.AddRef(Marshal.GetIUnknownForObject(_frameLoadDelegate));

            _downloadDelegate = new WebDownloadDelegate();
            Marshal.AddRef(Marshal.GetIUnknownForObject(_downloadDelegate));

            _policyDelegate = new WebPolicyDelegate(AllowNavigation, AllowDownloads, AllowNewWindows);
            Marshal.AddRef(Marshal.GetIUnknownForObject(_policyDelegate));

            _uiDelegate = new WebUIDelegate(this);
            Marshal.AddRef(Marshal.GetIUnknownForObject(_uiDelegate));

            _webNotificationCenter = new WebNotificationCenter();
            Marshal.AddRef(Marshal.GetIUnknownForObject(_webNotificationCenter)); // TODO: find out if this is really needed
            _webNotificationObserver = new WebNotificationObserver();
            _webNotificationCenter.defaultCenter().addObserver(_webNotificationObserver, "WebProgressEstimateChangedNotification", _webView);
            _webNotificationCenter.defaultCenter().addObserver(_webNotificationObserver, "WebProgressStartedNotification", _webView);
            _webNotificationCenter.defaultCenter().addObserver(_webNotificationObserver, "WebProgressFinishedNotification", _webView);

            _webView.setPolicyDelegate(_policyDelegate);
            _webView.setFrameLoadDelegate(_frameLoadDelegate);
            _webView.setDownloadDelegate(_downloadDelegate);
            _webView.setUIDelegate(_uiDelegate);

            _webView.setHostWindow(this._webKitBrowserHost.Handle.ToInt32());

            var rect = new tagRECT();
            rect.top = rect.left = 0;
            rect.bottom = this._webKitBrowserHost.Height - 1;
            rect.right = this._webKitBrowserHost.Width - 1;
            _webView.initWithFrame(rect, null, null);

            var webViewPrivate = (IWebViewPrivate)_webView;
            _webViewHwnd = (IntPtr)webViewPrivate.viewWindow();

            // Subscribe to FrameLoadDelegate events
            _frameLoadDelegate.DidRecieveTitle += FrameLoadDelegate_DidRecieveTitle;
            _frameLoadDelegate.DidFinishLoadForFrame += FrameLoadDelegate_DidFinishLoadForFrame;
            _frameLoadDelegate.DidStartProvisionalLoadForFrame += FrameLoadDelegate_DidStartProvisionalLoadForFrame;
            _frameLoadDelegate.DidCommitLoadForFrame += FrameLoadDelegate_DidCommitLoadForFrame;
            _frameLoadDelegate.DidFailLoadWithError += FrameLoadDelegate_DidFailLoadWithError;
            _frameLoadDelegate.DidFailProvisionalLoadWithError += FrameLoadDelegate_DidFailProvisionalLoadWithError;
            _frameLoadDelegate.DidClearWindowObject += FrameLoadDelegate_DidClearWindowObject;

            // DownloadDelegate events
            _downloadDelegate.DidReceiveResponse += DownloadDelegate_DidReceiveResponse;
            _downloadDelegate.DidReceiveDataOfLength += DownloadDelegate_DidReceiveDataOfLength;
            _downloadDelegate.DecideDestinationWithSuggestedFilename += DownloadDelegate_DecideDestinationWithSuggestedFilename;
            _downloadDelegate.DidBegin += DownloadDelegate_DidBegin;
            _downloadDelegate.DidFinish += DownloadDelegate_DidFinish;
            _downloadDelegate.DidFailWithError += DownloadDelegate_DidFailWithError;

            // UIDelegate events
            _uiDelegate.CreateWebViewWithRequest += UIDelegate_CreateWebViewWithRequest;
            _uiDelegate.RunJavaScriptAlertPanelWithMessage += UIDelegate_RunJavaScriptAlertPanelWithMessage;
            _uiDelegate.RunJavaScriptConfirmPanelWithMessage += UIDelegate_RunJavaScriptConfirmPanelWithMessage;
            _uiDelegate.RunJavaScriptTextInputPanelWithPrompt += UIDelegate_RunJavaScriptTextInputPanelWithPrompt;

            // Notification events
            _webNotificationObserver.OnNotify += webNotificationObserver_OnNotify;

            _activationContext.Deactivate();
        }

        #endregion

        #region Control event handers

        private void WebKitBrowser_Resize(object Sender, EventArgs Args)
        {
            // Resize the WebKit control
            NativeMethods.MoveWindow(_webViewHwnd, 0, 0, this._webKitBrowserHost.Width - 1, this._webKitBrowserHost.Height - 1, true);
        }

        private void WebKitBrowser_Load(object Sender, EventArgs Args)
        {
            // Create the WebKit browser component
            InitializeWebKit();

            _loaded = _webView != null;

            // intialize properties that depend on load
            if (_initialUrl != null)
            {
                Navigate(_initialUrl.AbsoluteUri);
            }
            else
            {
                DocumentText = _initialText;
                _policyDelegate.AllowInitialNavigation = false;
            }

            ScriptingEnabled = _initialJavaScriptEnabled;
            LocalStorageEnabled = _initialLocalStorageEnabled;
            LocalStorageDatabaseDirectory = _initialLocalStorageDatabaseDirectory;
        }

        // TODO: unused?
        /*private void WebKitBrowser_HandleDestroyed(object sender, EventArgs e)
        {
            _webNotificationCenter.defaultCenter().removeObserver(_webNotificationObserver, "WebProgressEstimateChangedNotification", _webView);
            _webNotificationCenter.defaultCenter().removeObserver(_webNotificationObserver, "WebProgressStartedNotification", _webView);
            _webNotificationCenter.defaultCenter().removeObserver(_webNotificationObserver, "WebProgressFinishedNotification", _webView);
        }*/

        #endregion

        #region WebFrameLoadDelegate event handlers 

        private void FrameLoadDelegate_DidCommitLoadForFrame(WebView WebView, IWebFrame Frame)
        {
            if (Frame == _webView.mainFrame())
            {
                Navigated(this, new WebBrowserNavigatedEventArgs(Url));
            }
        }

        private void FrameLoadDelegate_DidStartProvisionalLoadForFrame(WebView WebView, IWebFrame Frame)
        {
            if (Frame == _webView.mainFrame())
            {
                var url = Frame.provisionalDataSource().request().url();
                Navigating(this, new WebBrowserNavigatingEventArgs(new Uri(url), Frame.name()));
            }
        }

        private void FrameLoadDelegate_DidFinishLoadForFrame(WebView WebView, IWebFrame Frame)
        {
            if (Frame == _webView.mainFrame())
            {
                _policyDelegate.AllowInitialNavigation = _policyDelegate.AllowNavigation;
                DocumentCompleted(this, new WebBrowserDocumentCompletedEventArgs(Url));
            }
        }

        private void FrameLoadDelegate_DidRecieveTitle(WebView WebView, string Title, IWebFrame Frame)
        {
            if (Frame == _webView.mainFrame())
            {
                DocumentTitle = Title;
                DocumentTitleChanged(this, new EventArgs());
            }
        }

        private void FrameLoadDelegate_DidFailProvisionalLoadWithError(WebView WebView, IWebError WebError, IWebFrame Frame)
        {
            // ignore an "error" where the page loading is interrupted by a policy change when dowloading a file
            if (!(Frame == WebView.mainFrame() && WebError.Domain() == "WebKitErrorDomain" && WebError.code() == 102))
            {
                Error(this, new WebKitBrowserErrorEventArgs(WebError.localizedDescription()));
            }
        }

        private void FrameLoadDelegate_DidFailLoadWithError(WebView WebView, IWebError WebError, IWebFrame Frame)
        {
            Error(this, new WebKitBrowserErrorEventArgs(WebError.localizedDescription())); 
        }

        private void FrameLoadDelegate_DidClearWindowObject(WebView WebView, IntPtr Context, IntPtr WindowScriptObject, IWebFrame Frame)
        {
            CreateWindowScriptObject(new JSContext(Context));
        }

        #endregion

        #region WebDownloadDelegate event handlers

        private void DownloadDelegate_DidFailWithError(WebDownload Download, WebError WebError)
        {
            _downloads[Download].NotifyDidFailWithError(Download, WebError);
        }

        private void DownloadDelegate_DidFinish(WebDownload Download)
        {
            _downloads[Download].NotifyDidFinish(Download);
            _downloads.Remove(Download);
        }

        private void DownloadDelegate_DidBegin(WebDownload Download)
        {
            // create WebKitDownload object to handle this download and notify listeners
            var d = new WebKitDownload();
            _downloads.Add(Download, d);

            var args = new FileDownloadBeginEventArgs(d);
            DownloadBegin(this, args);

            if (args.Cancel)
                d.Cancel();
        }

        private void DownloadDelegate_DecideDestinationWithSuggestedFilename(WebDownload Download, string FileName)
        {
            _downloads[Download].NotifyDecideDestinationWithSuggestedFilename(Download, FileName);
        }

        private void DownloadDelegate_DidReceiveDataOfLength(WebDownload Download, uint Length)
        {
            // returns false if we cancelled the download at this point
            if (!_downloads[Download].NotifyDidReceiveDataOfLength(Download, Length))
                _downloads.Remove(Download);
        }

        private void DownloadDelegate_DidReceiveResponse(WebDownload Download, WebURLResponse Response)
        {
            _downloads[Download].NotifyDidReceiveResponse(Download, Response);
        }

        #endregion

        #region WebUIDelegate event handlers

        private void UIDelegate_CreateWebViewWithRequest(IWebURLRequest Request, out WebView WebView)
        {
            // TODO: find out why url seems to always be empty:
            // https://bugs.webkit.org/show_bug.cgi?id=41441 explains all
            string url = (Request == null) ? "" : Request.url();
            var args = new NewWindowRequestEventArgs(url);
            NewWindowRequest(this, args);

            if (!args.Cancel)
            {
                var b = new WebKitBrowserCore(_webKitBrowserHost);
                WebView = (WebView) b._webView;
                NewWindowCreated(this, new NewWindowCreatedEventArgs(b));
            }
            else
            {
                WebView = null;
            }
        }

        private void UIDelegate_RunJavaScriptAlertPanelWithMessage(WebView Sender, string Message)
        {
            ShowJavaScriptAlertPanel(this, new ShowJavaScriptAlertPanelEventArgs(Message));
        }

        private int UIDelegate_RunJavaScriptConfirmPanelWithMessage(WebView Sender, string Message)
        {
            var args = new ShowJavaScriptConfirmPanelEventArgs(Message);
            ShowJavaScriptConfirmPanel(this, args);
            return args.ReturnValue ? 1 : 0;
        }

        private string UIDelegate_RunJavaScriptTextInputPanelWithPrompt(WebView Sender, string Message, string DefaultText)
        {
            var args = new ShowJavaScriptPromptPanelEventArgs(Message, DefaultText);
            ShowJavaScriptPromptPanel(this, args);
            return args.ReturnValue;
        }

        #endregion

        #region WebNotificationObserver event handlers

        private void webNotificationObserver_OnNotify(IWebNotification Notification)
        {
            switch (Notification.name())
            {
                case "WebProgressStartedNotification":
                    var startedArgs = new EventArgs();
                    ProgressStarted(this, startedArgs);
                    break;
                case "WebProgressFinishedNotification":
                    var finishedArgs = new EventArgs();
                    ProgressFinished(this, finishedArgs);
                    break;
                case "WebProgressEstimateChangedNotification":
                    var changedArgs = new ProgressChangedEventArgs((int)(_webView.estimatedProgress() * 100), null);
                    ProgressChanged(this, changedArgs);
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Navigates to the specified Url.
        /// </summary>
        /// <param name="NewUrl">Url to navigate to.</param>
        public void Navigate(string NewUrl)
        {
            if (_loaded)
            {
                // prepend with "http://" if url not well formed
                if (!Uri.IsWellFormedUriString(NewUrl, UriKind.Absolute))
                    NewUrl = "http://" + NewUrl;

                _activationContext.Activate();

                WebMutableURLRequest request = new WebMutableURLRequestClass();
                request.initWithURL(NewUrl, _WebURLRequestCachePolicy.WebURLRequestUseProtocolCachePolicy, 60);
                request.setHTTPMethod("GET");

                //use basic authentication if username and password are supplied.
                if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
                    request.setValue("Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", UserName, Password))), "Authorization");

                _webView.mainFrame().loadRequest((WebURLRequest)request);

                _activationContext.Deactivate();
            }
            else
            {
                _initialUrl = NewUrl.Length == 0 ? null : new Uri(NewUrl);
            }
        }

        /// <summary>
        /// Navigates to the previous page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        public bool GoBack()
        {
            bool retVal = CanGoBack;
            _webView.goBack();
            return retVal;
        }

        /// <summary>
        /// Navigates to the next page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        public bool GoForward()
        {
            bool retVal = CanGoForward;
            _webView.goForward();
            return retVal;
        }

        /// <summary>
        /// Reloads the current web page.
        /// </summary>
        public void Reload()
        {
            _webView.mainFrame().reload();
        }

        /// <summary>
        /// Reloads the current web page.
        /// </summary>
        /// <param name="Option">Options for reloading the page.</param>
        public void Reload(WebBrowserRefreshOption Option)
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
            if (_webView.isLoading() != 0)
            {
                _webView.mainFrame().stopLoading();
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
            return _webView;
        }

        /// <summary>
        /// Gets the script context for the WebView.
        /// </summary>
        /// <returns>A JSCore.JSContext object representing the script context.</returns>
        public object GetGlobalScriptContext()
        {
            if (_loaded)
                return new JSContext(_webView.mainFrame());
            return null;
        }

        public void ShowInspector()
        {
            if (_webView == null)
                return;
            var v = (IWebViewPrivate) _webView;
            v.inspector().attach();
            v.inspector().show();
        }

        #endregion Public Methods

        #region Printing Methods

        /// <summary>
        /// Prints the document using the current print and page settings.
        /// </summary>
        public void Print()
        {
            var doc = this.GetCommonPrintDocument();
            var pm = new PrintManager(doc, this._webKitBrowserHost, this, false);
            pm.Print();
        }

        /// <summary>
        /// Displays a Page Setup dialog box with the current page and print settings.
        /// </summary>
        public void ShowPageSetupDialog()
        {
            var pageSetupDlg = new PageSetupDialog();
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
            var printDlg = new PrintDialog();
            var doc = this.GetCommonPrintDocument();
            printDlg.Document = doc;

            if (printDlg.ShowDialog() == DialogResult.OK)
            {
                var pm = new PrintManager(doc, this._webKitBrowserHost, this, false);
                pm.Print();
            }
        }

        /// <summary>
        /// Displays a Print Preview dialog box.
        /// </summary>
        public void ShowPrintPreviewDialog()
        {
            var printDlg = new PrintPreviewDialog();
            var doc = this.GetCommonPrintDocument();
            printDlg.Document = doc;
            var pm = new PrintManager(doc, this._webKitBrowserHost, this, true);
            pm.Print();
            printDlg.ShowDialog();
        }

        // Gets a PrintDocument with the current default settings.
        private PrintDocument GetCommonPrintDocument()
        {
            var doc = new PrintDocument();
            doc.DocumentName = this.DocumentTitle;
            doc.DefaultPageSettings = PageSettings;
            doc.OriginAtMargins = true;
            doc.PrinterSettings = PageSettings.PrinterSettings;
            return doc;
        }

        #endregion

        public void Dispose(bool Disposing)
        {
            if(_disposed)
                return;
            
            if((--_actCtxRefCount) == 0 && _activationContext != null)
            {
                _activationContext.Dispose();
            }

            _disposed = true;
        }

        private void CreateWindowScriptObject(JSContext Context)
        {
            if (ObjectForScripting != null && Context != null)
            {
                var global = Context.GetGlobalObject();
                var window = global.GetProperty("window");
                if (window == null || !window.IsObject)
                    return;
                var windowObj = window.ToObject();
                if (windowObj == null)
                    return;
                windowObj.SetProperty("external", ObjectForScripting);
            }
        }
    }
}