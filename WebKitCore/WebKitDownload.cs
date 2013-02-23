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

// TODO: make more robust, support for more options etc...
// Design feels a little messy at the moment.

using WebKit.Interop;

namespace WebKit
{
    /// <summary>
    /// Represents a file download.
    /// </summary>
    public class WebKitDownload
    {
        #region FileDownload events

        /// <summary>
        /// Gets a value indicating whether the download was cancelled.
        /// </summary>
        public bool DidCancel { get; private set; }

        /// <summary>
        /// Gets the destination file path.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Occurs when file data transfer begins.
        /// </summary>
        public event DownloadStartedEventHandler DownloadStarted;
        
        /// <summary>
        /// Occurs when file data arrives.
        /// </summary>
        public event DownloadReceiveDataEventHandler DownloadReceiveData;

        /// <summary>
        /// Occurs when the file is downloaded successfully.
        /// </summary>
        public event DownloadFinishedEventHandler DownloadFinished;

        #endregion

        internal WebKitDownload()
        {
            FilePath = "";
        }
    
        internal void NotifyDidFailWithError(WebDownload Download, WebError Error)
        {
            // Todo
        }

        internal void NotifyDidFinish(WebDownload Download)
        {
            DownloadFinished(this, new DownloadFinishedEventArgs());
        }

        internal void NotifyDecideDestinationWithSuggestedFilename(WebDownload Download, string FileName)
        {
            Download.setDestination(FilePath.Length != 0 ? FilePath : FileName, 1);
        }

        internal bool NotifyDidReceiveDataOfLength(WebDownload Download, uint Length)
        {
            if (DidCancel)
            {
                Download.cancel();
                return false;
            }
            DownloadReceiveData(this, new DownloadReceiveDataEventArgs(Length));
            return true;
        }

        internal void NotifyDidReceiveResponse(WebDownload Download, WebURLResponse Response)
        {
            DownloadStarted(this, new DownloadStartedEventArgs(Response.suggestedFilename(), Response.expectedContentLength()));
        }

        /// <summary>
        /// Cancel the file download.
        /// </summary>
        public void Cancel()
        {
            DidCancel = true;
        }
    }
}
