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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace WebKit
{
    /// <summary>
    /// WebKit Browser Control.
    /// </summary>
    public partial class WebKitBrowser : UserControl, IWebKitBrowserHost, IWebKitBrowser
    {
        private readonly WebKitBrowserCore _core;

        /// <summary>
        /// Processes a command key.  Overridden in WebKitBrowser to forward key events to the WebKit window.
        /// </summary>
        /// <param name="Msg">The window message to process.</param>
        /// <param name="KeyData">The key to process.</param>
        /// <returns>Success value.</returns>
        protected override bool ProcessCmdKey(ref Message Msg, Keys KeyData)
        {
            var key = (Keys) Msg.WParam.ToInt32();
            if (key == Keys.Left || key == Keys.Right || key == Keys.Up ||
                key == Keys.Down || key == Keys.Tab)
            {
                NativeMethods.SendMessage(Msg.HWnd, (uint) Msg.Msg, Msg.WParam, Msg.LParam);
                return true;
            }

            return base.ProcessCmdKey(ref Msg, KeyData);
        }

        #region WebKitBrowser events



        // public events, roughly the same as in WebBrowser class
        // using the null object pattern to avoid null tests

        /// <summary>
        /// Occurs when the DocumentTitle property value changes.
        /// </summary>
        public event EventHandler DocumentTitleChanged
        {
            add { _core.DocumentTitleChanged += value; }
            remove { _core.DocumentTitleChanged -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control finishes loading a document.
        /// </summary>
        public event WebBrowserDocumentCompletedEventHandler DocumentCompleted
        {
            add { _core.DocumentCompleted += value; }
            remove { _core.DocumentCompleted -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control has navigated to a new document and has begun loading it.
        /// </summary>
        public event WebBrowserNavigatedEventHandler Navigated
        {
            add { _core.Navigated += value; }
            remove { _core.Navigated -= value; }
        }

        /// <summary>
        /// Occurs before the WebKitBrowser control navigates to a new document.
        /// </summary>
        public event WebBrowserNavigatingEventHandler Navigating
        {
            add { _core.Navigating += value; }
            remove { _core.Navigating -= value; }
        }

        /// <summary>
        /// Occurs when an error occurs on the current document, or when navigating to a new document.
        /// </summary>
        public event WebKitBrowserErrorEventHandler Error
        {
            add { _core.Error += value; }
            remove { _core.Error -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control begins a file download, before any data has been transferred.
        /// </summary>
        public event FileDownloadBeginEventHandler DownloadBegin
        {
            add { _core.DownloadBegin += value; }
            remove { _core.DownloadBegin -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control attempts to open a link in a new window.
        /// </summary>
        public event NewWindowRequestEventHandler NewWindowRequest
        {
            add { _core.NewWindowRequest += value; }
            remove { _core.NewWindowRequest -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control creates a new window.
        /// </summary>
        public event NewWindowCreatedEventHandler NewWindowCreated
        {
            add { _core.NewWindowCreated += value; }
            remove { _core.NewWindowCreated -= value; }
        }

        /// <summary>
        /// Occurs when JavaScript requests an alert panel to be displayed via the alert() function.
        /// </summary>
        public event ShowJavaScriptAlertPanelEventHandler ShowJavaScriptAlertPanel
        {
            add { _core.ShowJavaScriptAlertPanel += value; }
            remove { _core.ShowJavaScriptAlertPanel -= value; }
        }

        /// <summary>
        /// Occurs when JavaScript requests a confirm panel to be displayed via the confirm() function.
        /// </summary>
        public event ShowJavaScriptConfirmPanelEventHandler ShowJavaScriptConfirmPanel
        {
            add { _core.ShowJavaScriptConfirmPanel += value; }
            remove { _core.ShowJavaScriptConfirmPanel -= value; }
        }

        /// <summary>
        /// Occurs when JavaScript requests a prompt panel to be displayed via the prompt() function.
        /// </summary>
        public event ShowJavaScriptPromptPanelEventHandler ShowJavaScriptPromptPanel
        {
            add { _core.ShowJavaScriptPromptPanel += value; }
            remove { _core.ShowJavaScriptPromptPanel -= value; }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The current print page settings.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PageSettings PageSettings
        {
            get { return _core.PageSettings; }
            set { _core.PageSettings = value; }
        }

        /// <summary>
        /// Gets the title of the current document.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DocumentTitle
        {
            get { return _core.DocumentTitle; }
        }

        /// <summary>
        /// Gets or sets the current Url.
        /// </summary>
        [Browsable(true), Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Specifies the Url to navigate to.")]
        public Uri Url
        {
            get { return _core.Url; }
            set { _core.Url = value; }
        }

        /// <summary>
        /// Gets a value indicating whether a web page is currently being loaded.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBusy
        {
            get { return _core.IsBusy; }
        }

        /// <summary>
        /// Gets or sets the HTML content of the current document.
        /// </summary>
        [Browsable(true), DefaultValue(""), Category("Appearance")]
        [Description("The HTML content to be displayed if no Url is specified.")]
        public string DocumentText
        {
            get { return _core.DocumentText; }
            set { _core.DocumentText = value; }
        }

        /// <summary>
        /// Gets the currently selected text.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get { return _core.SelectedText; }
        }

        /// <summary>
        /// Gets or sets the application name for the user agent.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ApplicationName
        {
            get { return _core.ApplicationName; }
            set { _core.ApplicationName = value; }
        }

        /// <summary>
        /// Gets or sets the user agent string.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UserAgent
        {
            get { return _core.UserAgent; }
            set { _core.UserAgent = value; }
        }

        /// <summary>
        /// Gets or sets the text size multiplier (1.0 is normal size).
        /// </summary>
        [Browsable(true), DefaultValue(1.0f), Category("Appearance")]
        [Description("Specifies the text size multiplier.")]
        public float TextSize
        {
            get { return _core.TextSize; }
            set { _core.TextSize = value; }
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
            get { return _core.AllowNavigation; }
            set { _core.AllowNavigation = value; }
        }

        /// <summary>
        /// Gets or sets whether to allow file downloads.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether to allow file downloads.")]
        public bool AllowDownloads
        {
            get { return _core.AllowDownloads; }
            set { _core.AllowDownloads = value; }
        }

        /// <summary>
        /// Gets or sets whether to allow links to be opened in a new window.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether to allow links to be" +
            " opened in a new window.")]
        public bool AllowNewWindows
        {
            get { return _core.AllowNewWindows; }
            set { _core.AllowNewWindows = value; }
        }

        /// <summary>
        /// Gets a value indicating whether a previous page in the navigation history is available.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanGoBack
        {
            get { return _core.CanGoBack; }
        }

        /// <summary>
        /// Gets a value indicating whether a subsequent page in the navigation history is available.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanGoForward
        {
            get { return _core.CanGoForward; }
        }

        /// <summary>
        /// Gets a Document representing the currently displayed page.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DOM.Document Document
        {
            get { return _core.Document; }
        }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Version Version
        {
            get { return _core.Version; }
        }

        /// <summary>
        /// Gets or sets the scroll offset of the current page, in pixels from the origin.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point ScrollOffset
        {
            get { return _core.ScrollOffset; }
            set { _core.ScrollOffset = value; }
        }

        /// <summary>
        /// Gets the visible content rectangle of the current view, in pixels.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle VisibleContent
        {
            get { return _core.VisibleContent; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the context menu of the WebKitBrowser is enabled.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether the default browser context menu is enabled")]
        public bool WebBrowserContextMenuEnabled
        {
            get { return _core.WebBrowserContextMenuEnabled; }
            set { _core.WebBrowserContextMenuEnabled = value; }
        }
        
        [Browsable(false)]
        [Obsolete("Deprecated: use WebBrowserContextMenuEnabled instead")]
        public bool IsWebBrowserContextMenuEnabled
        {
            get { return WebBrowserContextMenuEnabled; }
            set { WebBrowserContextMenuEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether JavaScript is enabled.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether JavaScript is enabled in the WebKitBrowser")]
        public bool ScriptingEnabled
        {
            get { return _core.ScriptingEnabled; }
            set { _core.ScriptingEnabled = value; }
        }

        [Browsable(false)]
        [Obsolete("Deprecated: use ScriptingEnabled instead")]
        public bool IsScriptingEnabled
        {
            get { return ScriptingEnabled; }
            set { ScriptingEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether LocalStorage is enabled.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether LocalStorage is enabled in the WebKitBrowser")]
        public bool LocalStorageEnabled {
          get { return _core.LocalStorageEnabled; }
          set { _core.LocalStorageEnabled = value; }
        }

        [Browsable(false)]
        [Obsolete("Deprecated: use LocalStorageEnabled instead")]
        public bool IsLocalStorageEnabled
        {
            get { return LocalStorageEnabled; }
            set { LocalStorageEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the fully qualified path to the directory where 
        /// local storage database files will be stored.
        /// </summary>
        /// <remarks>Value must be a fully qualified directory path.</remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LocalStorageDatabaseDirectory {
          get { return _core.LocalStorageDatabaseDirectory; }
          set { _core.LocalStorageDatabaseDirectory = value; }
        }

        /// <summary>
        /// Gets or sets an object that can be accessed by JavaScript contained within the WebKitBrowser control.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object ObjectForScripting
        {
            get { return _core.ObjectForScripting; }
            set { _core.ObjectForScripting = value; }
        }

        #endregion

        #region Constructors / initialization functions

        public WebKitBrowser()
            : this(new WebKitBrowserCore())
        {
        }

        /// <summary>
        /// Initializes a new instance of the WebKitBrowser control.
        /// </summary>
        public WebKitBrowser(WebKitBrowserCore BrowserCore)
        {
            _core = BrowserCore;
            NewWindowCreated += delegate { };
            NewWindowRequest += delegate { };
            DownloadBegin += delegate { };
            Error += delegate { };
            Navigating += delegate { };
            Navigated += delegate { };
            DocumentCompleted += delegate { };
            DocumentTitleChanged += delegate { };
            ShowJavaScriptAlertPanel += delegate { };
            ShowJavaScriptConfirmPanel += delegate { };
            ShowJavaScriptPromptPanel += delegate { };
            InitializeComponent();

            _core.Initialize(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Navigates to the specified Url.
        /// </summary>
        /// <param name="Url">Url to navigate to.</param>
        public void Navigate(string Url)
        {
            _core.Navigate(Url);
        }

        /// <summary>
        /// Show the web inspector.
        /// </summary>
        public void ShowInspector()
        {
            _core.ShowInspector();
        }

        /// <summary>
        /// Navigates to the previous page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        public bool GoBack()
        {
            return _core.GoBack();
        }

        /// <summary>
        /// Navigates to the next page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        public bool GoForward()
        {
            return _core.GoForward();
        }

        /// <summary>
        /// Reloads the current web page.
        /// </summary>
        public void Reload()
        {
            _core.Reload();
        }

        /// <summary>
        /// Reloads the current web page.
        /// </summary>
        /// <param name="Option">Options for reloading the page.</param>
        public void Reload(WebBrowserRefreshOption Option)
        {
            _core.Reload(Option);
        }

        /// <summary>
        /// Stops loading the current web page and any resources associated 
        /// with it.
        /// </summary>
        public void Stop()
        {
            _core.Stop();
        }

        /// <summary>
        /// Returns the result of running a script.
        /// </summary>
        /// <param name="Script">The script to run.</param>
        /// <returns></returns>
        public string StringByEvaluatingJavaScriptFromString(string Script)
        {
            return _core.StringByEvaluatingJavaScriptFromString(Script);
        }

        /// <summary>
        /// Gets the underlying WebKit WebView object used by this instance of WebKitBrowser.
        /// </summary>
        /// <returns>The WebView object.</returns>
        public object GetWebView()
        {
            return _core.GetWebView();
        }

        /// <summary>
        /// Gets the script context for the WebView.
        /// </summary>
        /// <returns>A JSCore.JSContext object representing the script context.</returns>
        public object GetGlobalScriptContext()
        {
            return _core.GetGlobalScriptContext();
        }

        // printing methods

        /// <summary>
        /// Prints the document using the current print and page settings.
        /// </summary>
        public void Print()
        {
            _core.Print();
        }

        /// <summary>
        /// Displays a Page Setup dialog box with the current page and print settings.
        /// </summary>
        public void ShowPageSetupDialog()
        {
            _core.ShowPageSetupDialog();
        }

        /// <summary>
        /// Displays a Print dialog box.
        /// </summary>
        public void ShowPrintDialog()
        {
            _core.ShowPrintDialog();
        }

        /// <summary>
        /// Displays a Print Preview dialog box.
        /// </summary>
        public void ShowPrintPreviewDialog()
        {
            _core.ShowPrintPreviewDialog();
        }

        #endregion Public Methods

        IWebKitBrowserHost IWebKitBrowser.Host
        {
            get { return this; }
        }

        bool IWebKitBrowserHost.InDesignMode
        {
            get { return LicenseManager.UsageMode == LicenseUsageMode.Designtime; }
        }
    }
}