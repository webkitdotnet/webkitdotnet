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

// Handles frame-level events that occur from loading a web page.  More info at
// http://developer.apple.com/documentation/Cocoa/Reference/WebKit/Protocols/WebFrameLoadDelegate_Protocol

using System;
using WebKit.Interop;

namespace WebKit
{
    // Delegate definitions WebFrameLoadDelegate events
    internal delegate void DidCancelClientRedirectForFrameEvent(WebView WebView, IWebFrame Frame);
    internal delegate void DidChangeLocationWithinPageForFrameEvent(WebView WebView, IWebFrame Frame);
    internal delegate void DidCommitLoadForFrameEvent(WebView WebView, IWebFrame Frame);
    internal delegate void DidFailLoadWithErrorEvent(WebView WebView, IWebError Error, IWebFrame Frame);
    internal delegate void DidFailProvisionalLoadWithErrorEvent(WebView WebView, IWebError Error, IWebFrame Frame);
    internal delegate void DidFinishLoadForFrameEvent(WebView WebView, IWebFrame Frame);
    internal delegate void DidRecieveIconEvent(WebView WebView, int hBitmap, IWebFrame Frame);
    internal delegate void DidRecieveServerRedirectForProvisionalLoadForFrameEvent(WebView WebView, IWebFrame Frame);
    internal delegate void DidRecieveTitleEvent(WebView WebView, string Title, IWebFrame Frame);
    internal delegate void DidStartProvisionalLoadForFrameEvent(WebView WebView, IWebFrame Frame);
    internal delegate void WillCloseFrameEvent(WebView WebView, IWebFrame Frame);
    internal delegate void WillPerformClientRedirectToURLEvent(WebView WebView, string Url, double DelaySeconds, DateTime FireDate, IWebFrame Frame);
    internal delegate void WindowScriptObjectAvailableEvent(WebView WebView, IntPtr Context, IntPtr WindowScriptObject);
    internal delegate void DidClearWindowObjectEvent(WebView WebView, IntPtr Context, IntPtr WindowScriptObject, IWebFrame Frame);

    internal class WebFrameLoadDelegate : IWebFrameLoadDelegate
    {
        public event DidCancelClientRedirectForFrameEvent DidCancelClientRedirectForFrame = delegate { };
        public event DidChangeLocationWithinPageForFrameEvent DidChangeLocationWithinPageForFrame = delegate { };
        public event DidCommitLoadForFrameEvent DidCommitLoadForFrame = delegate { };
        public event DidFailLoadWithErrorEvent DidFailLoadWithError = delegate { };
        public event DidFailProvisionalLoadWithErrorEvent DidFailProvisionalLoadWithError = delegate { };
        public event DidFinishLoadForFrameEvent DidFinishLoadForFrame = delegate { };
        public event DidRecieveIconEvent DidRecieveIcon = delegate { };
        public event DidRecieveServerRedirectForProvisionalLoadForFrameEvent DidRecieveServerRedirectForProvisionalLoadForFrame = delegate { };
        public event DidRecieveTitleEvent DidRecieveTitle = delegate { };
        public event DidStartProvisionalLoadForFrameEvent DidStartProvisionalLoadForFrame = delegate { };
        public event WillCloseFrameEvent WillCloseFrame = delegate { };
        public event WillPerformClientRedirectToURLEvent WillPerformClientRedirectToURL = delegate { };
        public event WindowScriptObjectAvailableEvent WindowScriptObjectAvailable = delegate { };
        public event DidClearWindowObjectEvent DidClearWindowObject = delegate { };

        #region webFrameLoadDelegate Members
        public void didCancelClientRedirectForFrame(WebView WebView, webFrame Frame)
        {
            DidCancelClientRedirectForFrame(WebView, Frame);
        }

        public void didChangeLocationWithinPageForFrame(WebView WebView, webFrame Frame)
        {
            DidChangeLocationWithinPageForFrame(WebView, Frame);
        }

        public void didCommitLoadForFrame(WebView WebView, webFrame Frame)
        {
            DidCommitLoadForFrame(WebView, Frame);
        }

        public void didFailLoadWithError(WebView WebView, WebError Error, webFrame ForFrame)
        {
            DidFailLoadWithError(WebView, Error, ForFrame);
        }

        public void didFailProvisionalLoadWithError(WebView WebView, WebError Error, webFrame Frame)
        {
            DidFailProvisionalLoadWithError(WebView, Error, Frame);
        }

        public void didFinishLoadForFrame(WebView WebView, webFrame Frame)
        {
            DidFinishLoadForFrame(WebView, Frame);
        }

        public void didReceiveIcon(WebView WebView, int hBitmap, webFrame Frame)
        {
            DidRecieveIcon(WebView, hBitmap, Frame);
        }

        public void didReceiveServerRedirectForProvisionalLoadForFrame(WebView WebView, webFrame Frame)
        {
            DidRecieveServerRedirectForProvisionalLoadForFrame(WebView, Frame);
        }

        public void didReceiveTitle(WebView WebView, string Title, webFrame Frame)
        {
            DidRecieveTitle(WebView, Title, Frame);
        }

        public void didStartProvisionalLoadForFrame(WebView WebView, webFrame Frame)
        {
            DidStartProvisionalLoadForFrame(WebView, Frame);
        }

        public void willCloseFrame(WebView WebView, webFrame Frame)
        {
            WillCloseFrame(WebView, Frame);
        }

        public void willPerformClientRedirectToURL(WebView WebView, string Url, double DelaySeconds, DateTime FireDate, webFrame Frame)
        {
            WillPerformClientRedirectToURL(WebView, Url, DelaySeconds, FireDate, Frame);
        }

        public void windowScriptObjectAvailable(WebView WebView, IntPtr Context, IntPtr WindowScriptObject)
        {
            WindowScriptObjectAvailable(WebView, Context, WindowScriptObject);
        }

        public void didClearWindowObject(WebView WebView, IntPtr Context, IntPtr WindowScriptObject, webFrame Frame)
        {
            DidClearWindowObject(WebView, Context, WindowScriptObject, Frame);
        }


        #endregion
    }
}
