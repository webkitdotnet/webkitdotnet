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

using System;
using System.Collections.Generic;
using System.Text;
using WebKit.Interop;

namespace WebKit
{
    public class WebKitDownload
    {
        private WebDownload download;

        public bool DidCancel { get; private set; }
        public string FilePath { get; private set; }

        public event DownloadStartedEvent DownloadStarted;
        public event DownloadReceiveDataEvent DownloadReceiveData;
        public event DownloadFinishedEvent DownloadFinished;

        internal WebKitDownload(WebDownload download)
        {
            this.download = download;
            FilePath = "";
        }
    
        internal void NotifyDidFailWithError(WebDownload download, WebError error)
        {
            // Todo
        }

        internal void NotifyDidFinish(WebDownload download)
        {
            DownloadFinished(this, new EventArgs());
        }

        internal void NotifyDecideDestinationWithSuggestedFilename(WebDownload download, string fileName)
        {
            if (FilePath != "")
                download.setDestination(FilePath, 1);
            else
                download.setDestination(fileName, 1);
        }

        internal bool NotifyDidReceiveDataOfLength(WebDownload download, uint length)
        {
            if (DidCancel)
            {
                download.cancel();
                return false;
            }
            else
            {
                DownloadReceiveData(this, new DownloadReceiveDataEventArgs(length));
                return true;
            }
        }

        internal void NotifyDidReceiveResponse(WebDownload download, WebURLResponse response)
        {
            DownloadStarted(this, new DownloadStartedEventArgs(response.suggestedFilename(), response.expectedContentLength()));
        }

        public void Cancel()
        {
            DidCancel = true;
        }
    }
}
