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

// Handles events relating to UI changes.  More info at 
// http://developer.apple.com/documentation/Cocoa/Reference/WebKit/Protocols/WebUIDelegate_Protocol

// TODO: most of these events aren't used at all (yet). Find out what they
// do and whether they are actually working in WebKit.

using System;
using System.Drawing.Printing;
using WebKit.Interop;

namespace WebKit
{
    internal delegate void CreateWebViewWithRequestEvent(IWebURLRequest request, out WebView webView);

    internal class WebUIDelegate : IWebUIDelegate
    {
        public event CreateWebViewWithRequestEvent CreateWebViewWithRequest;

        private WebKitBrowserCore owner;

        public WebUIDelegate(WebKitBrowserCore browser)
        {
            this.owner = browser;
        }

        #region IWebUIDelegate Members

        public void addCustomMenuDrawingData(WebView sender, int hMenu)
        {
        }

        public int canRedo()
        {
            throw new NotImplementedException();
        }

        public int canRunModal(WebView WebView)
        {
            throw new NotImplementedException();
        }

        public void canTakeFocus(WebView sender, int forward, out int result)
        {
            throw new NotImplementedException();
        }

        public int canUndo()
        {
            throw new NotImplementedException();
        }

        public void cleanUpCustomMenuDrawingData(WebView sender, int hMenu)
        {
        }

        public void contextMenuItemSelected(WebView sender, IntPtr item, CFDictionaryPropertyBag element)
        {
        }

        public int contextMenuItemsForElement(WebView sender, CFDictionaryPropertyBag element, int defaultItemsHMenu)
        {
            return owner.IsWebBrowserContextMenuEnabled ? defaultItemsHMenu : 0;
        }

        public WebView createModalDialog(WebView sender, IWebURLRequest request)
        {
            throw new NotImplementedException();
        }

        public WebView createWebViewWithRequest(WebView sender, IWebURLRequest request)
        {
            // this should be caught in the WebPolicyDelegate, but isn't in the Cairo build
            if (owner.AllowNewWindows)
            {
                WebView view;
                CreateWebViewWithRequest(request, out view);
                return view;
            }
            else
            {
                return null;
            }
        }

        public WebDragDestinationAction dragDestinationActionMaskForDraggingInfo(WebView WebView, IDataObject draggingInfo)
        {
            throw new NotImplementedException();
        }

        public WebDragSourceAction dragSourceActionMaskForPoint(WebView WebView, ref tagPOINT point)
        {
            throw new NotImplementedException();
        }

        public void drawCustomMenuItem(WebView sender, IntPtr drawItem)
        {
        }

        public void drawFooterInRect(WebView WebView, ref tagRECT rect, int drawingContext, uint pageIndex, uint pageCount)
        {
        }

        public void drawHeaderInRect(WebView WebView, ref tagRECT rect, int drawingContext)
        {
        }

        public string ftpDirectoryTemplatePath(WebView WebView)
        {
            throw new NotImplementedException();
        }

        public int hasCustomMenuImplementation()
        {
            throw new NotImplementedException();
        }

        public int isMenuBarVisible(WebView WebView)
        {
            throw new NotImplementedException();
        }

        public void makeFirstResponder(WebView sender, int responderHWnd)
        {
        }

        public void measureCustomMenuItem(WebView sender, IntPtr measureItem)
        {
        }

        public void mouseDidMoveOverElement(WebView sender, CFDictionaryPropertyBag elementInformation, uint modifierFlags)
        {
        }

        public void paintCustomScrollCorner(WebView WebView, int hDC, tagRECT rect)
        {
        }

        public void paintCustomScrollbar(WebView WebView, int hDC, tagRECT rect, WebScrollBarControlSize size, uint state, WebScrollbarControlPart pressedPart, int vertical, float value, float proportion, uint parts)
        {
        }

        public void printFrame(WebView WebView, webFrame frame)
        {
        }

        public void redo()
        {
        }

        public void registerUndoWithTarget(IWebUndoTarget target, string actionName, object actionArg)
        {
        }

        public void removeAllActionsWithTarget(IWebUndoTarget target)
        {
        }

        public int runBeforeUnloadConfirmPanelWithMessage(WebView sender, string message, webFrame initiatedByFrame)
        {
            throw new NotImplementedException();
        }

        public int runDatabaseSizeLimitPrompt(WebView WebView, string displayName, webFrame initiatedByFrame)
        {
            throw new NotImplementedException();
        }

        public void runJavaScriptAlertPanelWithMessage(WebView sender, string message)
        {
        }

        public int runJavaScriptConfirmPanelWithMessage(WebView sender, string message)
        {
            throw new NotImplementedException();
        }

        public string runJavaScriptTextInputPanelWithPrompt(WebView sender, string message, string defaultText)
        {
            throw new NotImplementedException();
        }

        public void runModal(WebView WebView)
        {
        }

        public void runOpenPanelForFileButtonWithResultListener(WebView sender, IWebOpenPanelResultListener resultListener)
        {
        }

        public void setActionTitle(string actionTitle)
        {
        }

        public void setContentRect(WebView sender, ref tagRECT contentRect)
        {
        }

        public void setFrame(WebView sender, ref tagRECT frame)
        {
        }

        public void setMenuBarVisible(WebView WebView, int visible)
        {
        }

        public void setResizable(WebView sender, int resizable)
        {
        }

        public void setStatusBarVisible(WebView sender, int visible)
        {
        }

        public void setStatusText(WebView sender, string text)
        {
        }

        public void setToolbarsVisible(WebView sender, int visible)
        {
        }

        public void shouldPerformAction(WebView WebView, uint itemCommandID, uint sender)
        {
        }

        public void takeFocus(WebView sender, int forward)
        {
        }

        public void trackCustomPopupMenu(WebView sender, int hMenu, ref tagPOINT point)
        {
        }

        public void undo()
        {
        }

        public int validateUserInterfaceItem(WebView WebView, uint itemCommandID, int defaultValidation)
        {
            throw new NotImplementedException();
        }

        public int webViewAreToolbarsVisible(WebView sender)
        {
            throw new NotImplementedException();
        }

        public void webViewClose(WebView sender)
        {
        }

        public tagRECT webViewContentRect(WebView sender)
        {
            throw new NotImplementedException();
        }

        public int webViewFirstResponder(WebView sender)
        {
            throw new NotImplementedException();
        }

        public void webViewFocus(WebView sender)
        {
        }

        public float webViewFooterHeight(WebView WebView)
        {
            throw new NotImplementedException();
        }

        public tagRECT webViewFrame(WebView sender)
        {
            throw new NotImplementedException();
        }

        public float webViewHeaderHeight(WebView WebView)
        {
            throw new NotImplementedException();
        }

        public int webViewIsResizable(WebView sender)
        {
            throw new NotImplementedException();
        }

        public int webViewIsStatusBarVisible(WebView sender)
        {
            return 0;
        }

        public tagRECT webViewPrintingMarginRect(WebView WebView)
        {
            PageSettings settings = owner.PageSettings;

            // WebKit specifies margins in 1000ths of an inch.
            // PrinterResolution.Y returns 0 for some reason,
            // on Adobe distiller anyway, so we'll use X for the moment.
            int dpi = settings.PrinterResolution.X;
            int marginLeft = settings.Margins.Left;
            int marginRight = settings.Margins.Right;
            int marginTop = settings.Margins.Top;
            int marginBottom = settings.Margins.Bottom;
            
            tagRECT rect = new tagRECT();
            rect.left = marginLeft * 10;
            rect.top = marginTop * 10;
            rect.right = marginRight * 10;
            rect.bottom = marginBottom * 10;
            return rect;
        }

        public void webViewShow(WebView sender)
        {
        }

        public string webViewStatusText(WebView sender)
        {
            throw new NotImplementedException();
        }

        public void webViewUnfocus(WebView sender)
        {
        }

        public void willPerformDragDestinationAction(WebView WebView, WebDragDestinationAction action, IDataObject draggingInfo)
        {
        }

        public IDataObject willPerformDragSourceAction(WebView WebView, WebDragSourceAction action, ref tagPOINT point, IDataObject pasteboard)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
