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
        private WebKitBrowserCore core = new WebKitBrowserCore();

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
                NativeMethods.SendMessage(core.WebViewHWND, (uint)msg.Msg, msg.WParam, msg.LParam);
                return true;
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region WebKitBrowser events



        // public events, roughly the same as in WebBrowser class
        // using the null object pattern to avoid null tests
        
        /// <summary>
        /// Occurs when the DocumentTitle property value changes.
        /// </summary>
        public event EventHandler DocumentTitleChanged
        {
            add { core.DocumentTitleChanged += value; }
            remove { core.DocumentTitleChanged -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control finishes loading a document.
        /// </summary>
        public event WebBrowserDocumentCompletedEventHandler DocumentCompleted
        {
            add { core.DocumentCompleted += value; }
            remove { core.DocumentCompleted -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control has navigated to a new document and has begun loading it.
        /// </summary>
        public event WebBrowserNavigatedEventHandler Navigated
        {
            add { core.Navigated += value; }
            remove { core.Navigated -= value; }
        }

        /// <summary>
        /// Occurs before the WebKitBrowser control navigates to a new document.
        /// </summary>
        public event WebBrowserNavigatingEventHandler Navigating
        {
            add { core.Navigating += value; }
            remove { core.Navigating -= value; }
        }

        /// <summary>
        /// Occurs when an error occurs on the current document, or when navigating to a new document.
        /// </summary>
        public event WebKitBrowserErrorEventHandler Error
        {
            add { core.Error += value; }
            remove { core.Error -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control begins a file download, before any data has been transferred.
        /// </summary>
        public event FileDownloadBeginEventHandler DownloadBegin
        {
            add { core.DownloadBegin += value; }
            remove { core.DownloadBegin -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control attempts to open a link in a new window.
        /// </summary>
        public event NewWindowRequestEventHandler NewWindowRequest
        {
            add { core.NewWindowRequest += value; }
            remove { core.NewWindowRequest -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control creates a new window.
        /// </summary>
        public event NewWindowCreatedEventHandler NewWindowCreated
        {
            add { core.NewWindowCreated += value; }
            remove { core.NewWindowCreated -= value; }
        }

        /// <summary>
        /// Occures when WebKitBrowser control has begun to provide information on the download progress of a document it is navigating to.
        /// </summary>
        public event ProgressStartedEventHandler ProgressStarted
        {
            add { core.ProgressStarted += value; }
            remove { core.ProgressStarted -= value; }
        }

        /// <summary>
        /// Occures when WebKitBrowser control is no longer providing information on the download progress of a document it is navigating to.
        /// </summary>
        public event ProgressFinishedEventHandler ProgressFinished
        {
            add { core.ProgressFinished += value; }
            remove { core.ProgressFinished -= value; }
        }

        /// <summary>
        /// Occurs when the WebKitBrowser control has updated information on the download progress of a document it is navigating to.
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged
        {
            add { core.ProgressChanged += value; }
            remove { core.ProgressChanged -= value; }
        }

        /// <summary>
        /// Occurs when JavaScript requests an alert panel to be displayed via the alert() function.
        /// </summary>
        public event ShowJavaScriptAlertPanelEventHandler ShowJavaScriptAlertPanel
        {
            add { core.ShowJavaScriptAlertPanel += value; }
            remove { core.ShowJavaScriptAlertPanel -= value; }
        }

        /// <summary>
        /// Occurs when JavaScript requests a confirm panel to be displayed via the confirm() function.
        /// </summary>
        public event ShowJavaScriptConfirmPanelEventHandler ShowJavaScriptConfirmPanel
        {
            add { core.ShowJavaScriptConfirmPanel += value; }
            remove { core.ShowJavaScriptConfirmPanel -= value; }
        }

        /// <summary>
        /// Occurs when JavaScript requests a prompt panel to be displayed via the prompt() function.
        /// </summary>
        public event ShowJavaScriptPromptPanelEventHandler ShowJavaScriptPromptPanel
        {
            add { core.ShowJavaScriptPromptPanel += value; }
            remove { core.ShowJavaScriptPromptPanel -= value; }
        }


        #endregion

        #region Public properties

        /// <summary>
        /// The HTTP Basic Authentication UserName
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UserName
        {
            get { return core.UserName; }
            set { core.UserName = value; }
        }

        /// <summary>
        /// The HTTP Basic Authentication Password
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Password
        {
            set { core.Password = value; }
        }

        /// <summary>
        /// The current print page settings.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PageSettings PageSettings
        {
            get { return core.PageSettings; }
            set { core.PageSettings = value; }
        }

        /// <summary>
        /// Gets the title of the current document.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DocumentTitle
        {
            get { return core.DocumentTitle; }
        }

        /// <summary>
        /// Gets or sets the current Url.
        /// </summary>
        [Browsable(true), Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Specifies the Url to navigate to.")]
        public Uri Url
        {
            get { return core.Url; }
            set { core.Url = value; }
        }

        /// <summary>
        /// Gets a value indicating whether a web page is currently being loaded.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBusy
        {
            get { return core.IsBusy; }
        }

        /// <summary>
        /// Gets or sets the HTML content of the current document.
        /// </summary>
        [Browsable(true), DefaultValue(""), Category("Appearance")]
        [Description("The HTML content to be displayed if no Url is specified.")]
        public string DocumentText
        {
            get { return core.DocumentText; }
            set { core.DocumentText = value; }
        }

        /// <summary>
        /// Gets the currently selected text.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get { return core.SelectedText; }
        }

        /// <summary>
        /// Gets or sets the application name for the user agent.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ApplicationName
        {
            get { return core.ApplicationName; }
            set { core.ApplicationName = value; }
        }

        /// <summary>
        /// Gets or sets the user agent string.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UserAgent
        {
            get { return core.UserAgent; }
            set { core.UserAgent = value; }
        }

        /// <summary>
        /// Gets or sets the text size multiplier (1.0 is normal size).
        /// </summary>
        [Browsable(true), DefaultValue(1.0f), Category("Appearance")]
        [Description("Specifies the text size multiplier.")]
        public float TextSize
        {
            get { return core.TextSize; }
            set { core.TextSize = value; }
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
            get { return core.AllowNavigation; }
            set { core.AllowNavigation = value; }
        }

        /// <summary>
        /// Gets or sets whether to allow file downloads.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether to allow file downloads.")]
        public bool AllowDownloads
        {
            get { return core.AllowDownloads; }
            set { core.AllowDownloads = value; }
        }

        /// <summary>
        /// Gets or sets whether to allow links to be opened in a new window.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether to allow links to be" +
            " opened in a new window.")]
        public bool AllowNewWindows
        {
            get { return core.AllowNewWindows; }
            set { core.AllowNewWindows = value; }
        }

        /// <summary>
        /// Gets a value indicating whether a previous page in the navigation history is available.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanGoBack
        {
            get { return core.CanGoBack; }
        }

        /// <summary>
        /// Gets a value indicating whether a subsequent page in the navigation history is available.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanGoForward
        {
            get { return core.CanGoForward; }
        }

        /// <summary>
        /// Gets a Document representing the currently displayed page.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DOM.Document Document
        {
            get { return core.Document; }
        }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Version Version
        {
            get { return core.Version; }
        }

        /// <summary>
        /// Gets or sets the scroll offset of the current page, in pixels from the origin.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point ScrollOffset
        {
            get { return core.ScrollOffset; }
            set { core.ScrollOffset = value; }
        }

        /// <summary>
        /// Gets the visible content rectangle of the current view, in pixels.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle VisibleContent
        {
            get { return core.VisibleContent; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the context menu of the WebKitBrowser is enabled.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether the default browser context menu is enabled")]
        public bool IsWebBrowserContextMenuEnabled
        {
            get { return core.IsWebBrowserContextMenuEnabled; }
            set { core.IsWebBrowserContextMenuEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether JavaScript is enabled.
        /// </summary>
        [Browsable(true), DefaultValue(true), Category("Behavior")]
        [Description("Specifies whether JavaScript is enabled in the WebKitBrowser")]
        public bool IsScriptingEnabled
        {
            get { return core.IsScriptingEnabled; }
            set { core.IsScriptingEnabled = value; }
        }

        /// <summary>
        /// Gets or sets an object that can be accessed by JavaScript contained within the WebKitBrowser control.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object ObjectForScripting
        {
            get { return core.ObjectForScripting; }
            set { core.ObjectForScripting = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LocalStorageDatabasePath
        {
            get { return core.LocalStorageDatabasePath; }
            set { core.LocalStorageDatabasePath = value; }
        }

        #endregion

        #region Constructors / initialization functions

        /// <summary>
        /// Initializes a new instance of the WebKitBrowser control.
        /// </summary>
        public WebKitBrowser()
        {
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

            core.Initialize(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Navigates to the specified Url.
        /// </summary>
        /// <param name="url">Url to navigate to.</param>
        public void Navigate(string url)
        {
            core.Navigate(url);
        }

        /// <summary>
        /// Navigates to the previous page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        public bool GoBack()
        {
            return core.GoBack();
        }

        /// <summary>
        /// Navigates to the next page in the page history, if available.
        /// </summary>
        /// <returns>Success value.</returns>
        public bool GoForward()
        {
            return core.GoForward();
        }

        /// <summary>
        /// Reloads the current web page.
        /// </summary>
        public void Reload()
        {
            core.Reload();
        }

        /// <summary>
        /// Reloads the current web page.
        /// </summary>
        /// <param name="option">Options for reloading the page.</param>
        public void Reload(WebBrowserRefreshOption option)
        {
            core.Reload(option);
        }

        /// <summary>
        /// Stops loading the current web page and any resources associated 
        /// with it.
        /// </summary>
        public void Stop()
        {
            core.Stop();
        }

        /// <summary>
        /// Returns the result of running a script.
        /// </summary>
        /// <param name="Script">The script to run.</param>
        /// <returns></returns>
        public string StringByEvaluatingJavaScriptFromString(string Script)
        {
            return core.StringByEvaluatingJavaScriptFromString(Script);
        }

        /// <summary>
        /// Gets the underlying WebKit WebView object used by this instance of WebKitBrowser.
        /// </summary>
        /// <returns>The WebView object.</returns>
        public object GetWebView()
        {
            return core.GetWebView();
        }

        /// <summary>
        /// Gets the script context for the WebView.
        /// </summary>
        /// <returns>A JSCore.JSContext object representing the script context.</returns>
        public object GetGlobalScriptContext()
        {
            return core.GetGlobalScriptContext();
        }

        // printing methods

        /// <summary>
        /// Prints the document using the current print and page settings.
        /// </summary>
        public void Print()
        {
            core.Print();
        }

        /// <summary>
        /// Displays a Page Setup dialog box with the current page and print settings.
        /// </summary>
        public void ShowPageSetupDialog()
        {
            core.ShowPageSetupDialog();
        }

        /// <summary>
        /// Displays a Print dialog box.
        /// </summary>
        public void ShowPrintDialog()
        {
            core.ShowPrintDialog();
        }

        /// <summary>
        /// Displays a Print Preview dialog box.
        /// </summary>
        public void ShowPrintPreviewDialog()
        {
            core.ShowPrintPreviewDialog();
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
