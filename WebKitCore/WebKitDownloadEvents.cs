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

// Various event handlers and argument classes used by WebKitDownload to
// communicate with clients.

using System;

namespace WebKit
{
    #region Event delegates

    /// <summary>
    /// Represents the method that will handle the WebKitDownload.DownloadStarted event.
    /// </summary>
    /// <param name="Sender">The source of the event.</param>
    /// <param name="Args">A DownloadStartedEventArgs that contains the event data.</param>
    public delegate void DownloadStartedEventHandler(object Sender, DownloadStartedEventArgs Args);

    /// <summary>
    /// Represents the method that will handle the WebKitDownload.DownloadReceiveData event.
    /// </summary>
    /// <param name="Sender">The source of the event.</param>
    /// <param name="Args">A DownloadReceiveDataEventArgs that contains the event data.</param>
    public delegate void DownloadReceiveDataEventHandler(object Sender, DownloadReceiveDataEventArgs Args);

    /// <summary>
    /// Represents the method that will handle the WebKitDownload.DownloadFinished event.
    /// </summary>
    /// <param name="Sender">The source of the event.</param>
    /// <param name="Args">A DownloadFinishedEventArgs that contains the event data.</param>
    public delegate void DownloadFinishedEventHandler(object Sender, DownloadFinishedEventArgs Args);

    #endregion

    #region EventArg classes

    /// <summary>
    /// Provides data for the WebKitDownload.DownloadStarted event.
    /// </summary>
    public class DownloadStartedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the suggested name for the destination file.
        /// </summary>
        public string SuggestedFileName { get; private set; }

        /// <summary>
        /// Gets the file size in bytes.
        /// </summary>
        public long FileSize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the DownloadStartedEventArgs class.
        /// </summary>
        /// <param name="SuggestedFileName">The suggested name for the destination file.</param>
        /// <param name="FileSize">The file size in bytes.</param>
        public DownloadStartedEventArgs(string SuggestedFileName, long FileSize)
        {
            this.SuggestedFileName = SuggestedFileName;
            this.FileSize = FileSize;
        }
    }

    /// <summary>
    /// Provides data for the WebKitDownload.DownloadReceiveData event.
    /// </summary>
    public class DownloadReceiveDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the length of the data received in bytes.
        /// </summary>
        public uint Length { get; private set; }

        /// <summary>
        /// Initializes a new instance of the DownloadReceiveDataEventArgs class.
        /// </summary>
        /// <param name="Length">The length of the data received in bytes.</param>
        public DownloadReceiveDataEventArgs(uint Length)
        {
            this.Length = Length;
        }
    }

    /// <summary>
    /// Provides data for the WebKitDownload.DownloadFinished event.
    /// </summary>
    public class DownloadFinishedEventArgs : EventArgs
    {
    }

    #endregion
}
