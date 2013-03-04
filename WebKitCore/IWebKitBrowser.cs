using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace WebKit
{
    public interface IWebKitBrowser
    {
        /// <summary>
        /// Occurs when the DocumentTitle property value changes.
        /// </summary>
        event EventHandler DocumentTitleChanged;

        /// <summary>
        /// Occurs when the WebKitBrowser control finishes loading a document.
        /// </summary>
        event WebBrowserDocumentCompletedEventHandler DocumentCompleted;

        /// <summary>
        /// Occurs when the WebKitBrowser control has navigated to a new document and has begun loading it.
        /// </summary>
        event WebBrowserNavigatedEventHandler Navigated;

        /// <summary>
        /// Occurs before the WebKitBrowser control navigates to a new document.
        /// </summary>
        event WebBrowserNavigatingEventHandler Navigating;

        /// <summary>
        /// Occurs when an error occurs on the current document, or when navigating to a new document.
        /// </summary>
        event WebKitBrowserErrorEventHandler Error;

        /// <summary>
        /// Occurs when the WebKitBrowser control begins a file download, before any data has been transferred.
        /// </summary>
        event FileDownloadBeginEventHandler DownloadBegin;

        /// <summary>
        /// Occurs when the WebKitBrowser control attempts to open a link in a new window.
        /// </summary>
        event NewWindowRequestEventHandler NewWindowRequest;

        /// <summary>
        /// Occurs when the WebKitBrowser control creates a new window.
        /// </summary>
        event NewWindowCreatedEventHandler NewWindowCreated;

        /// <summary>
        /// Occurs when JavaScript requests an alert panel to be displayed via the alert() function.
        /// </summary>
        event ShowJavaScriptAlertPanelEventHandler ShowJavaScriptAlertPanel;

        /// <summary>
        /// Occurs when JavaScript requests a confirm panel to be displayed via the confirm() function.
        /// </summary>
        event ShowJavaScriptConfirmPanelEventHandler ShowJavaScriptConfirmPanel;

        /// <summary>
        /// Occurs when JavaScript requests a prompt panel to be displayed via the prompt() function.
        /// </summary>
        event ShowJavaScriptPromptPanelEventHandler ShowJavaScriptPromptPanel;

        /// <summary>
        /// The current print page settings.
        /// </summary>
        PageSettings PageSettings { get; set; }

        /// <summary>
        /// Gets the title of the current document.
        /// </summary>
        string DocumentTitle { get; }

        /// <summary>
        /// Gets or sets the current Url.
        /// </summary>
        Uri Url { get; set; }

        /// <summary>
        /// Gets a value indicating whether a web page is currently being loaded.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Gets or sets the HTML content of the current document.
        /// </summary>
        string DocumentText { get; set; }

        /// <summary>
        /// Gets the currently selected text.
        /// </summary>
        string SelectedText { get; }

        /// <summary>
        /// Gets or sets the application name for the user agent.
        /// </summary>
        string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the user agent string.
        /// </summary>
        string UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the text size multiplier (1.0 is normal size).
        /// </summary>
        float TextSize { get; set; }

        /// <summary>
        /// Gets or sets whether the control can navigate to another page 
        /// once it's initial page has loaded.
        /// </summary>
        bool AllowNavigation { get; set; }

        /// <summary>
        /// Gets or sets whether to allow file downloads.
        /// </summary>
        bool AllowDownloads { get; set; }

        /// <summary>
        /// Gets or sets whether to allow links to be opened in a new window.
        /// </summary>
        bool AllowNewWindows { get; set; }

        /// <summary>
        /// Gets a value indicating whether a previous page in the navigation history is available.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Gets a value indicating whether a subsequent page in the navigation history is available.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        /// Gets a Document representing the currently displayed page.
        /// </summary>
        DOM.Document Document { get; }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets or sets the scroll offset of the current page, in pixels from the origin.
        /// </summary>
        Point ScrollOffset { get; set; }

        /// <summary>
        /// Gets the visible content rectangle of the current view, in pixels.
        /// </summary>
        Rectangle VisibleContent { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the context menu of the WebKitBrowser is enabled.
        /// </summary>
        bool WebBrowserContextMenuEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether JavaScript is enabled.
        /// </summary>
        bool ScriptingEnabled { get; set; }

        /// <summary>
        /// Gets or sets an object that can be accessed by JavaScript contained within the WebKitBrowser control.
        /// </summary>
        object ObjectForScripting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether LocalStorage is enabled.
        /// </summary>
        bool LocalStorageEnabled { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified path to the directory where 
        /// local storage database files will be stored.
        /// </summary>
        /// <remarks>Value must be a fully qualified directory path.</remarks>
        string LocalStorageDatabaseDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether cross origin requests
        /// from file:// URIs to other file:// URIs are allowed.
        /// </summary>
        bool AllowFileAccessFromFileURLs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        CookieAcceptPolicy CookieAcceptPolicy { get; set; }

        /*bool AcceleratedCompositingEnabled { get; set; }

        bool AllowAnimatedImageLooping { get; set; }

        bool AllowContinuousSpellChecking { get; set; }

        bool AllowAnimatedImages { get; set; }

        bool AllowUniversalFileAccessFromFileURLs { get; set; }

        bool PluginsEnabled { get; set; }

        bool AuthorAndUserStylesEnabled { get; set; }

        bool DetectCacheModelAutomatically { get; set; }

        bool Autosave { get; set; }

        bool AVFoundationEnabled { get; set; }

        object CacheModel { get; set; }

        bool ContinuousSpellCheckingEnabled { get; set; }

        string CursiveFontFamily { get; set; }

        bool CustomDragCursorsEnabled { get; set; }

        bool DatabasesEnabled { get; set; }

        int DefaultFixedFontSize { get; set; }

        int DefaultFontSize { get; set; }

        string DefaultTextEncodingName { get; set; }

        object EditableLinkBehavior { get; set; }

        object EditingBehavior { get; set; }

        bool NotificationsEnabled { get; set; }

        string FantasyFontFamily { get; set; }

        string FixedFontFamily { get; set; }

        object FontSmoothing { get; set; }

        float FontSmoothingContrast { get; set; }

        bool GrammarCheckingEnabled { get; set; }

        bool Hixie76WebSocketProtocolEnabled { get; set; }

        bool HyperlinkAuditingEnabled { get; set; }

        bool IconDatabaseEnabled { get; set; }

        string IconDatabaseDirectory { get; set; }

        bool ApplicationChromeModeEnabled { get; set; }

        bool DNSPrefetchingEnabled { get; set; }

        bool DOMPasteAllowed { get; set; }

        bool FrameFlatteningEnabled { get; set; }

        bool FullScreenEnabled { get; set; }

        bool JavaEnabled { get; set; }

        bool WebSecurityEnabled { get; set; }

        bool XSSAuditorEnabled { get; set; }

        bool JavaScriptCanAccessClipboard { get; set; }

        bool JavaScriptCanOpenWindowsAutomatically { get; set; }

        bool LoadImagesAutomatically { get; set; }

        bool LoadSiteIconsIgnoringImageLoadingPreference { get; set; }

        bool MediaPlaybackAllowsInline { get; set; }

        bool MediaPlaybackRequiresUserGesture { get; set; }

        bool MemoryInfoEnabled { get; set; }

        int MinimumFontSize { get; set; }

        int MinimumLogicalFontSize { get; set; }

        bool OfflineWebApplicationCacheEnabled { get; set; }

        string PictographFontFamily { get; set; }

        bool PrivateBrowsingEnabled { get; set; }

        string SansSerifFontFamily { get; set; }

        string SerifFontFamily { get; set; }

        bool DisplayCaptions { get; set; }

        bool DisplaySubtitles { get; set; }

        bool DisplayTextDescriptions { get; set; }

        bool InvertColors { get; set; }

        bool PaintNativeControls { get; set; }

        bool PrintBackgrounds { get; set; }

        bool HighResolutionTimersEnabled { get; set; }

        bool ShowDebugBorders { get; set; }

        bool ShowRepaintCounter { get; set; }

        bool ShowTooltipOverTruncatedText { get; set; }

        string StandardFontFamily { get; set; }

        bool TabsToLinks { get; set; }

        bool TextAreasAreResizable { get; set; }

        bool UserStyleSheetEnabled { get; set; }

        string UserStyleSheetLocation { get; set; }

        bool PageCacheEnabled { get; set; }

        bool ZoomsTextOnly { get; set; }*/

        /// <summary>
        /// Navigates to the specified Url.
        /// </summary>
        /// <param name="NewUrl">Url to navigate to.</param>
        void Navigate(string NewUrl);

        /// <summary>
        /// Navigates to the previous page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        bool GoBack();

        /// <summary>
        /// Navigates to the next page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        bool GoForward();

        /// <summary>
        /// Reloads the current web page.
        /// </summary>
        void Reload();

        /// <summary>
        /// Reloads the current web page.
        /// </summary>
        /// <param name="Option">Options for reloading the page.</param>
        void Reload(WebBrowserRefreshOption Option);

        /// <summary>
        /// Stops loading the current web page and any resources associated 
        /// with it.
        /// </summary>
        void Stop();

        /// <summary>
        /// Returns the result of running a script.
        /// </summary>
        /// <param name="Script">The script to run.</param>
        /// <returns></returns>
        string StringByEvaluatingJavaScriptFromString(string Script);

        /// <summary>
        /// Gets the underlying WebKit WebView object used by this instance of WebKitBrowser.
        /// </summary>
        /// <returns>The WebView object.</returns>
        object GetWebView();

        /// <summary>
        /// Gets the script context for the WebView.
        /// </summary>
        /// <returns>A JSCore.JSContext object representing the script context.</returns>
        object GetGlobalScriptContext();

        /// <summary>
        /// Prints the document using the current print and page settings.
        /// </summary>
        void Print();

        /// <summary>
        /// Displays a Page Setup dialog box with the current page and print settings.
        /// </summary>
        void ShowPageSetupDialog();

        /// <summary>
        /// Displays a Print dialog box.
        /// </summary>
        void ShowPrintDialog();

        /// <summary>
        /// Displays a Print Preview dialog box.
        /// </summary>
        void ShowPrintPreviewDialog();

        /// <summary>
        /// Gets the host.
        /// </summary>
        /// <value>The host.</value>
        IWebKitBrowserHost Host { get; }
    }
}