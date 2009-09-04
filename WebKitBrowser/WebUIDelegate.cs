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

// Not implemented yet.  More info at 
// http://developer.apple.com/documentation/Cocoa/Reference/WebKit/Protocols/WebUIDelegate_Protocol

using System;
using System.Collections.Generic;
using System.Text;
using WebKit.Interop;

namespace WebKit
{
    delegate void CreateWebViewWithRequestEvent(IWebURLRequest request, out WebView webView);

    internal class WebUIDelegate : IWebUIDelegate
    {
        public event CreateWebViewWithRequestEvent CreateWebViewWithRequest;

        #region IWebUIDelegate Members

        public void addCustomMenuDrawingData(WebView sender, int hMenu)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void contextMenuItemSelected(WebView sender, IntPtr item, CFDictionaryPropertyBag element)
        {
            throw new NotImplementedException();
        }

        public int contextMenuItemsForElement(WebView sender, CFDictionaryPropertyBag element, int defaultItemsHMenu)
        {
            throw new NotImplementedException();
        }

        public WebView createModalDialog(WebView sender, IWebURLRequest request)
        {
            throw new NotImplementedException();
        }

        public WebView createWebViewWithRequest(WebView sender, IWebURLRequest request)
        {
            WebView view;
            CreateWebViewWithRequest(request, out view);
            return view;
        }

        /*public IWebDesktopNotificationsDelegate desktopNotificationsDelegate()
        {
            throw new NotImplementedException();
        }*/

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
            throw new NotImplementedException();
        }

        public void drawFooterInRect(WebView WebView, ref tagRECT rect, int drawingContext, uint pageIndex, uint pageCount)
        {
            throw new NotImplementedException();
        }

        public void drawHeaderInRect(WebView WebView, ref tagRECT rect, int drawingContext)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void measureCustomMenuItem(WebView sender, IntPtr measureItem)
        {
            throw new NotImplementedException();
        }

        public void mouseDidMoveOverElement(WebView sender, CFDictionaryPropertyBag elementInformation, uint modifierFlags)
        {
        }

        public void paintCustomScrollCorner(WebView WebView, ref _RemotableHandle hDC, tagRECT rect)
        {
            throw new NotImplementedException();
        }

        public void paintCustomScrollbar(WebView WebView, ref _RemotableHandle hDC, tagRECT rect, WebScrollBarControlSize size, uint state, WebScrollbarControlPart pressedPart, int vertical, float value, float proportion, uint parts)
        {
            throw new NotImplementedException();
        }

        public void printFrame(WebView WebView, IWebFrame frame)
        {
            throw new NotImplementedException();
        }

        public void redo()
        {
            throw new NotImplementedException();
        }

        public void registerUndoWithTarget(IWebUndoTarget target, string actionName, object actionArg)
        {
            throw new NotImplementedException();
        }

        public void removeAllActionsWithTarget(IWebUndoTarget target)
        {
            throw new NotImplementedException();
        }

        public int runBeforeUnloadConfirmPanelWithMessage(WebView sender, string message, IWebFrame initiatedByFrame)
        {
            throw new NotImplementedException();
        }

        public int runDatabaseSizeLimitPrompt(WebView WebView, string displayName, IWebFrame initiatedByFrame)
        {
            throw new NotImplementedException();
        }

        public void runJavaScriptAlertPanelWithMessage(WebView sender, string message)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void runOpenPanelForFileButtonWithResultListener(WebView sender, IWebOpenPanelResultListener resultListener)
        {
            throw new NotImplementedException();
        }

        public void setActionTitle(string actionTitle)
        {
            throw new NotImplementedException();
        }

        public void setContentRect(WebView sender, ref tagRECT contentRect)
        {
            throw new NotImplementedException();
        }

        public void setFrame(WebView sender, ref tagRECT frame)
        {
            throw new NotImplementedException();
        }

        public void setMenuBarVisible(WebView WebView, int visible)
        {
            throw new NotImplementedException();
        }

        public void setResizable(WebView sender, int resizable)
        {
            throw new NotImplementedException();
        }

        public void setStatusBarVisible(WebView sender, int visible)
        {
            throw new NotImplementedException();
        }

        public void setStatusText(WebView sender, string text)
        {
        }

        public void setToolbarsVisible(WebView sender, int visible)
        {
            throw new NotImplementedException();
        }

        public void shouldPerformAction(WebView WebView, uint itemCommandID, uint sender)
        {
            throw new NotImplementedException();
        }

        public void takeFocus(WebView sender, int forward)
        {
            throw new NotImplementedException();
        }

        public void trackCustomPopupMenu(WebView sender, int hMenu, ref tagPOINT point)
        {
            throw new NotImplementedException();
        }

        public void undo()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void webViewShow(WebView sender)
        {
            throw new NotImplementedException();
        }

        public string webViewStatusText(WebView sender)
        {
            throw new NotImplementedException();
        }

        public void webViewUnfocus(WebView sender)
        {
            throw new NotImplementedException();
        }

        public void willPerformDragDestinationAction(WebView WebView, WebDragDestinationAction action, IDataObject draggingInfo)
        {
            throw new NotImplementedException();
        }

        public IDataObject willPerformDragSourceAction(WebView WebView, WebDragSourceAction action, ref tagPOINT point, IDataObject pasteboard)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
