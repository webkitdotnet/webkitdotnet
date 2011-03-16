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

// Various event handlers and argument classes used by WebKitBrowser to
// communicate with the client.

using System;
using System.ComponentModel;
using WebKit.Interop;

namespace WebKit
{
    #region Event handler delegates

    /// <summary>
    /// Represents the method that will handle the WebKitBrowser.Error event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A WebKitBrowserErrorEventArgs that contains the event data.</param>
    public delegate void WebKitBrowserErrorEventHandler (object sender, WebKitBrowserErrorEventArgs e);

    /// <summary>
    /// Represents the method that will handle the WebKitBrowser.FileDownloadBegin event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A FileDownloadBeginEventArgs that contains the event data.</param>
    public delegate void FileDownloadBeginEventHandler (object sender, FileDownloadBeginEventArgs e);

    /// <summary>
    /// Represents the method that will handle the WebKitBrowser.NewWindowRequest event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A NewWindowRequestEventArgs that contains the event data.</param>
    public delegate void NewWindowRequestEventHandler (object sender, NewWindowRequestEventArgs e);

    /// <summary>
    /// Represents the method that will handle the WebKitBrowser.NewWindowCreated event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A NewWindowCreatedEventArgs that contains the event data.</param>
    public delegate void NewWindowCreatedEventHandler (object sender, NewWindowCreatedEventArgs e);

    /// <summary>
    /// Represents the method that will handle the WebKitBrowser.ProgressStartedEventHandler event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An EventArgs that contains the event data.</param>
    public delegate void ProgressStartedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Represents the method that will handle the WebKitBrowser.ProgressFinishedEventHandler event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An EventArgs that contains the event data.</param>
    public delegate void ProgressFinishedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Represents the method that will handle the WebKitBrowser.ProgressChangedEventHandler event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A ProgressChangedEventArgs that contains the event data.</param>
    public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);

    /// <summary>
    /// Represents the method that will handle the WebKitBrowser.ShowJavaScriptAlertPanel event. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A ShowJavaScriptAlertPanelEventArgs that contains the event data.</param>
    public delegate void ShowJavaScriptAlertPanelEventHandler(object sender, ShowJavaScriptAlertPanelEventArgs e);

    /// <summary>
    /// Represents the method that will handle the WebKitBrowser.ShowJavaScriptConfirmPanel event. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A ShowJavaScriptConfirmPanelEventArgs that contains the event data.</param>
    public delegate void ShowJavaScriptConfirmPanelEventHandler(object sender, ShowJavaScriptConfirmPanelEventArgs e);

    /// <summary>
    /// Represents the method that will handle the WebKitBrowser.ShowJavaScriptPromptPanel event. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A ShowJavaScriptPromptPanelEventArgs that contains the event data.</param>
    public delegate void ShowJavaScriptPromptPanelEventHandler(object sender, ShowJavaScriptPromptPanelEventArgs e);

    #endregion

    #region EventArgs classes

    /// <summary>
    /// Provides data for the WebKitBrowser.Error event.
    /// </summary>
    public class WebKitBrowserErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a description of the error that occurred.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Initializes a new instance of the WebKitBrowserErrorEventArgs class.
        /// </summary>
        /// <param name="description">A description of the error that occurred.</param>
        public WebKitBrowserErrorEventArgs(string description)
        {
            this.Description = description;
        }
    }

    /// <summary>
    /// Provides data for the WebKitBrowser.FileDownloadBegin event.
    /// </summary>
    public class FileDownloadBeginEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the WebKitDownload representing the download.
        /// </summary>
        public WebKitDownload Download { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the download should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the FileDownloadBeginEventArgs class.
        /// </summary>
        /// <param name="download">A WebKitDownload representing the download.</param>
        public FileDownloadBeginEventArgs(WebKitDownload download)
        {
            this.Download = Download;
            this.Cancel = false;
        }
    }

    /// <summary>
    /// Provides data for the WebKitBrowser.NewWindowRequest event. 
    /// </summary>
    public class NewWindowRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether opening the new window should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the Url that the new window will attempt to navigate to.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Initializes a new instance of the NewWindowRequestEventArgs class.
        /// </summary>
        /// <param name="url">The Url that the new window will attempt to navigate to.</param>
        public NewWindowRequestEventArgs(string url)
        {
            this.Cancel = false;
            this.Url = url;
        }
    }

    /// <summary>
    /// Provides data for the WebKitBrowser.NewWindowCreated event.
    /// </summary>
    public class NewWindowCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the WebKitBrowser showing the contents of the new window.
        /// </summary>
        public IWebKitBrowser WebKitBrowser { get; private set; }

        /// <summary>
        /// Initializes a new instance of the NewWindowCreatedEventArgs class.
        /// </summary>
        /// <param name="browser">The WebKitBrowser showing the contents of the new window.</param>
        public NewWindowCreatedEventArgs(IWebKitBrowser browser)
        {
            WebKitBrowser = browser;
        }
    }

    /// <summary>
    /// Provides data for the WebKitBrowser.ShowJavaScriptAlertPanel event.
    /// </summary>
    public class ShowJavaScriptAlertPanelEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message to be shown in the alert panel.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ShowJavaScriptAlertPanelEventArgs class.
        /// </summary>
        /// <param name="message">The message to be shown in the alert panel.</param>
        public ShowJavaScriptAlertPanelEventArgs(string message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Provides data for the WebKitBrowser.ShowJavaScriptConfirmPanel event.
    /// </summary>
    public class ShowJavaScriptConfirmPanelEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message to be shown in the confirm panel.
        /// </summary>
        public string Message { get; private set; }
        
        /// <summary>
        /// Gets or sets the return value for the confirm panel.
        /// </summary>
        public bool ReturnValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the ShowJavaScriptConfirmPanelEventArgs class.
        /// </summary>
        /// <param name="message">The message to be shown in the confirm panel.</param>
        public ShowJavaScriptConfirmPanelEventArgs(string message)
        {
            Message = message;
            ReturnValue = false;
        }
    }

    /// <summary>
    /// Provides data for the WebKitBrowser.ShowJavaScriptPromptPanel event.
    /// </summary>
    public class ShowJavaScriptPromptPanelEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message to be shown in the prompt panel.
        /// </summary>
        public string Message { get; private set; }
        
        /// <summary>
        /// Gets the default value of the prompt panel.
        /// </summary>
        public string DefaultValue { get; private set; }

        /// <summary>
        /// Gets or sets the return value for the prompt panel.
        /// </summary>
        public string ReturnValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the ShowJavaScriptConfirmPanelEventArgs class.
        /// </summary>
        /// <param name="message">The message to be shown in the prompt panel.</param>
        /// <param name="defaultValue">The default value to be shown in the prompt panel.</param>
        public ShowJavaScriptPromptPanelEventArgs(string message, string defaultValue)
        {
            Message = message;
            DefaultValue = defaultValue;
            ReturnValue = defaultValue;
        }
    }

    #endregion
}
