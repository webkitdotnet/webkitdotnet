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

// TODO: currently, calling WebDownload.cancel() in the 'decideDestinationWithSuggestedFilename'
// method causes a crash - I've seen examples of this method being used here with no problems.
// Find out whether this is a bug in webkit or what else is going on. Various useful information at 
// http://developer.apple.com/documentation/Cocoa/Conceptual/URLLoadingSystem/Tasks/UsingNSURLDownload.html
// Also note that currently (as of revision ~45680), download stuff is not implemented in the 
// cairo build of WebKit yet.


using WebKit.Interop;

namespace WebKit
{
    internal delegate void DecideDestinationWithSuggestedFilenameEvent(WebDownload Download, string FileName);
    internal delegate void DidBeginEvent(WebDownload Download);
    internal delegate void DidCancelAuthenticationChallengeEvent(WebDownload Download, IWebURLAuthenticationChallenge Challenge);
    internal delegate void DidCreateDestinationEvent(WebDownload Download, string Destination);
    internal delegate void DidFailWithErrorEvent(WebDownload Download, WebError Error);
    internal delegate void DidFinishEvent(WebDownload Download);
    internal delegate void DidReceiveAuthenticationChallengeEvent(WebDownload Download, IWebURLAuthenticationChallenge Challenge);
    internal delegate void DidReceiveDataOfLengthEvent(WebDownload Download, uint Length);
    internal delegate void DidReceiveResponseEvent(WebDownload Download, WebURLResponse Response);
    internal delegate int ShouldDecodeSourceDataOfMIMETypeEvent(WebDownload Download, string EncodingType);
    internal delegate void WillResumeWithResponseEvent(WebDownload Download, WebURLResponse Response, long FromByte);
    internal delegate void WillSendRequestEvent(WebDownload Download, WebMutableURLRequest Request, WebURLResponse RedirectResponse, out WebMutableURLRequest FinalRequest);

    internal class WebDownloadDelegate : IWebDownloadDelegate
    {
        public event DecideDestinationWithSuggestedFilenameEvent DecideDestinationWithSuggestedFilename = delegate { };
        public event DidBeginEvent DidBegin = delegate { };
        public event DidCancelAuthenticationChallengeEvent DidCancelAuthenticationChallenge = delegate { };
        public event DidCreateDestinationEvent DidCreateDestination = delegate { };
        public event DidFailWithErrorEvent DidFailWithError = delegate { };
        public event DidFinishEvent DidFinish = delegate { };
        public event DidReceiveAuthenticationChallengeEvent DidReceiveAuthenticationChallenge = delegate { };
        public event DidReceiveDataOfLengthEvent DidReceiveDataOfLength = delegate { };
        public event DidReceiveResponseEvent DidReceiveResponse = delegate { };
        public event WillResumeWithResponseEvent WillResumeWithResponse = delegate { };

        #region IWebDownloadDelegate Members

        public void decideDestinationWithSuggestedFilename(WebDownload Download, string FileName)
        {
            DecideDestinationWithSuggestedFilename(Download, FileName);
        }

        public void didBegin(WebDownload Download)
        {
            DidBegin(Download);
        }

        public void didCancelAuthenticationChallenge(WebDownload Download, IWebURLAuthenticationChallenge Challenge)
        {
            DidCancelAuthenticationChallenge(Download, Challenge);
        }

        public void didCreateDestination(WebDownload Download, string Destination)
        {
            DidCreateDestination(Download, Destination);
        }

        public void didFailWithError(WebDownload Download, WebError Error)
        {
            DidFailWithError(Download, Error);
        }

        public void didFinish(WebDownload Download)
        {
            DidFinish(Download);
        }

        public void didReceiveAuthenticationChallenge(WebDownload Download, IWebURLAuthenticationChallenge Challenge)
        {
            DidReceiveAuthenticationChallenge(Download, Challenge);
        }

        public void didReceiveDataOfLength(WebDownload Download, uint Length)
        {
            DidReceiveDataOfLength(Download, Length);
        }

        public void didReceiveResponse(WebDownload Download, WebURLResponse Response)
        {
            DidReceiveResponse(Download, Response);
        }

        public int shouldDecodeSourceDataOfMIMEType(WebDownload Download, string EncodingType)
        {
            // TODO
            return 0;
        }

        public void willResumeWithResponse(WebDownload Download, WebURLResponse Response, long FromByte)
        {
            WillResumeWithResponse(Download, Response, FromByte);
        }

        public void willSendRequest(WebDownload Download, WebMutableURLRequest Request, WebURLResponse RedirectResponse, out WebMutableURLRequest FinalRequest)
        {
            FinalRequest = Request;
        }

        #endregion
    }
}
