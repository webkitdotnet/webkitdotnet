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

// Not used yet.  More info at
// http://developer.apple.com/documentation/Cocoa/Reference/WebKit/Protocols/WebResourceLoadDelegate_Protocol

using System;
using System.Collections.Generic;
using System.Text;
using WebKit;
using WebKit.Interop;

namespace WebKit
{
    internal class WebResourceLoadDelegate : IWebResourceLoadDelegate
    {
        #region IWebResourceLoadDelegate Members

        public void didCancelAuthenticationChallenge(WebView WebView, uint identifier, IWebURLAuthenticationChallenge challenge, IWebDataSource dataSource)
        {
            throw new NotImplementedException();
        }

        public void didFailLoadingWithError(WebView WebView, uint identifier, WebError error, IWebDataSource dataSource)
        {
            throw new NotImplementedException();
        }

        public void didFinishLoadingFromDataSource(WebView WebView, uint identifier, IWebDataSource dataSource)
        {
            throw new NotImplementedException();
        }

        public void didReceiveAuthenticationChallenge(WebView WebView, uint identifier, IWebURLAuthenticationChallenge challenge, IWebDataSource dataSource)
        {
            throw new NotImplementedException();
        }

        public void didReceiveContentLength(WebView WebView, uint identifier, uint length, IWebDataSource dataSource)
        {
            throw new NotImplementedException();
        }

        public void didReceiveResponse(WebView WebView, uint identifier, WebURLResponse response, IWebDataSource dataSource)
        {
            throw new NotImplementedException();
        }

        public void identifierForInitialRequest(WebView WebView, WebURLRequest request, IWebDataSource dataSource, uint identifier)
        {
            throw new NotImplementedException();
        }

        public void plugInFailedWithError(WebView WebView, WebError error, IWebDataSource dataSource)
        {
            throw new NotImplementedException();
        }

        public WebURLRequest willSendRequest(WebView WebView, uint identifier, WebURLRequest request, WebURLResponse redirectResponse, IWebDataSource dataSource)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}
