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
using WebKit.Interop;

namespace WebKit
{
    internal delegate void CreateWebViewWithRequestEvent(IWebURLRequest Request, out WebView WebView);
    internal delegate string FTPDirectoryTemplatePath(WebView WebView);
    internal delegate void RunJavaScriptAlertPanelWithMessageEvent(WebView Sender, string Message);
    internal delegate int RunJavaScriptConfirmPanelWithMessageEvent(WebView Sender, string Message);
    internal delegate string RunJavaScriptTextInputPanelWithPromptEvent(WebView Sender, string Message, string DefaultText);

    internal class WebUIDelegate : IWebUIDelegate
    {
        public event CreateWebViewWithRequestEvent CreateWebViewWithRequest;
        public event FTPDirectoryTemplatePath FTPDirectoryTemplatePath;
        public event RunJavaScriptAlertPanelWithMessageEvent RunJavaScriptAlertPanelWithMessage;
        public event RunJavaScriptConfirmPanelWithMessageEvent RunJavaScriptConfirmPanelWithMessage;
        public event RunJavaScriptTextInputPanelWithPromptEvent RunJavaScriptTextInputPanelWithPrompt;

        private readonly WebKitBrowserCore _owner;

        public WebUIDelegate(WebKitBrowserCore Browser)
        {
            this._owner = Browser;
        }

        #region IWebUIDelegate Members

        public void addCustomMenuDrawingData(WebView Sender, int hMenu)
        {
        }

        public int canRedo()
        {
            return 1;
        }

        public int canRunModal(WebView WebView)
        {
            throw new NotImplementedException();
        }

        public void canTakeFocus(WebView Sender, int Forward, out int Result)
        {
            Result = 0;            
        }

        public int canUndo()
        {
            return 1;
        }

        public void cleanUpCustomMenuDrawingData(WebView Sender, int hMenu)
        {
        }

        public void contextMenuItemSelected(WebView Sender, IntPtr Item, CFDictionaryPropertyBag Element)
        {
        }

        public int contextMenuItemsForElement(WebView Sender, CFDictionaryPropertyBag Element, int DefaultItemsHMenu)
        {
            return _owner.WebBrowserContextMenuEnabled ? DefaultItemsHMenu : 0;
        }

        public WebView createModalDialog(WebView Sender, WebURLRequest Request)
        {
            throw new NotImplementedException();
        }

        public WebView createWebViewWithRequest(WebView Sender, WebURLRequest Request)
        {
            // this should be caught in the WebPolicyDelegate, but isn't in the Cairo build
            if (_owner.AllowNewWindows)
            {
                WebView view;
                CreateWebViewWithRequest(Request, out view);
                return view;
            }
            return null;
        }

        public WebDragDestinationAction dragDestinationActionMaskForDraggingInfo(WebView WebView, IDataObject DraggingInfo)
        {
            return WebDragDestinationAction.WebDragDestinationActionNone;            
        }

        public WebDragSourceAction dragSourceActionMaskForPoint(WebView WebView, ref tagPOINT Point)
        {
            return WebDragSourceAction.WebDragSourceActionNone;
        }

        public void drawCustomMenuItem(WebView Sender, IntPtr DrawItem)
        {
        }

        public void drawFooterInRect(WebView WebView, ref tagRECT Rect, int DrawingContext, uint PageIndex, uint PageCount)
        {
        }

        public void drawHeaderInRect(WebView WebView, ref tagRECT Rect, int DrawingContext)
        {
        }

        // TODO: Um, what does this do? I can't figure it out from the source code and can't find it
        // in the docs.
        // From: https://trac.webkit.org/timeline?from=2007-07-15&daysback=4
        // --> Set the path to the FTP listing document template
        public string ftpDirectoryTemplatePath(WebView WebView)
        {
            //return FTPDirectoryTemplatePath(WebView);
            // You'd think the above would work but it's returning NullReferenceException
            return "";
        }

        public int hasCustomMenuImplementation()
        {
            return 0;
        }

        public int isMenuBarVisible(WebView WebView)
        {
            throw new NotImplementedException();
        }

        public void makeFirstResponder(WebView Sender, int ResponderHWnd)
        {
        }

        public void measureCustomMenuItem(WebView Sender, IntPtr MeasureItem)
        {
        }

        public void mouseDidMoveOverElement(WebView Sender, CFDictionaryPropertyBag ElementInformation, uint ModifierFlags)
        {
        }

        public void paintCustomScrollCorner(WebView WebView, int hDC, tagRECT Rect)
        {
        }

        public void paintCustomScrollbar(WebView WebView, int hDC, tagRECT Rect, WebScrollBarControlSize Size, uint State, WebScrollbarControlPart PressedPart, int Vertical, float Value, float Proportion, uint Parts)
        {
        }

        public void printFrame(WebView WebView, webFrame Frame)
        {
        }

        public void redo()
        {
        }

        public void registerUndoWithTarget(IWebUndoTarget Target, string ActionName, object ActionArg)
        {
        }

        public void removeAllActionsWithTarget(IWebUndoTarget Target)
        {
        }

        public int runBeforeUnloadConfirmPanelWithMessage(WebView Sender, string Message, webFrame InitiatedByFrame)
        {
            throw new NotImplementedException();
        }

        public int runDatabaseSizeLimitPrompt(WebView WebView, string DisplayName, webFrame InitiatedByFrame)
        {
            throw new NotImplementedException();
        }

        public void runJavaScriptAlertPanelWithMessage(WebView Sender, string Message)
        {
            RunJavaScriptAlertPanelWithMessage(Sender, Message);
        }

        public int runJavaScriptConfirmPanelWithMessage(WebView Sender, string Message)
        {
            return RunJavaScriptConfirmPanelWithMessage(Sender, Message);
        }

        public string runJavaScriptTextInputPanelWithPrompt(WebView Sender, string Message, string DefaultText)
        {
            return RunJavaScriptTextInputPanelWithPrompt(Sender, Message, DefaultText);
        }

        public void runModal(WebView WebView)
        {
        }

        public void runOpenPanelForFileButtonWithResultListener(WebView Sender, IWebOpenPanelResultListener ResultListener)
        {
        }

        public void setActionTitle(string ActionTitle)
        {
        }

        public void setContentRect(WebView Sender, ref tagRECT ContentRect)
        {
        }

        public void setFrame(WebView Sender, ref tagRECT Frame)
        {
        }

        public void setMenuBarVisible(WebView WebView, int Visible)
        {
        }

        public void setResizable(WebView Sender, int Resizable)
        {
        }

        public void setStatusBarVisible(WebView Sender, int Visible)
        {
        }

        public void setStatusText(WebView Sender, string Text)
        {
        }

        public void setToolbarsVisible(WebView Sender, int Visible)
        {
        }

        public void shouldPerformAction(WebView WebView, uint ItemCommandID, uint Sender)
        {
        }

        public void takeFocus(WebView Sender, int Forward)
        {
        }

        public void trackCustomPopupMenu(WebView Sender, int hMenu, ref tagPOINT Point)
        {
        }

        public void undo()
        {
        }

        public int validateUserInterfaceItem(WebView WebView, uint ItemCommandID, int DefaultValidation)
        {
            throw new NotImplementedException();
        }

        public int webViewAreToolbarsVisible(WebView Sender)
        {
            throw new NotImplementedException();
        }

        public void webViewClose(WebView Sender)
        {
        }

        public tagRECT webViewContentRect(WebView Sender)
        {
            throw new NotImplementedException();
        }

        public int webViewFirstResponder(WebView Sender)
        {
            throw new NotImplementedException();
        }

        public void webViewFocus(WebView Sender)
        {
        }

        public float webViewFooterHeight(WebView WebView)
        {
            return 0;
        }

        public tagRECT webViewFrame(WebView Sender)
        {
            return ((IWebViewPrivate)Sender).visibleContentRect();
        }

        public float webViewHeaderHeight(WebView WebView)
        {
            return 0;            
        }

        public int webViewIsResizable(WebView Sender)
        {
            return 1;            
        }

        public int webViewIsStatusBarVisible(WebView Sender)
        {
            return 0;
        }

        public tagRECT webViewPrintingMarginRect(WebView WebView)
        {
            var settings = _owner.PageSettings;

            // WebKit specifies margins in 1000ths of an inch.
            // PrinterResolution.Y returns 0 for some reason,
            // on Adobe distiller anyway, so we'll use X for the moment.
            int dpi = settings.PrinterResolution.X;
            int marginLeft = settings.Margins.Left;
            int marginRight = settings.Margins.Right;
            int marginTop = settings.Margins.Top;
            int marginBottom = settings.Margins.Bottom;
            
            var rect = new tagRECT();
            rect.left = marginLeft * 10;
            rect.top = marginTop * 10;
            rect.right = marginRight * 10;
            rect.bottom = marginBottom * 10;
            return rect;
        }

        public void webViewShow(WebView Sender)
        {
        }

        public string webViewStatusText(WebView Sender)
        {
            throw new NotImplementedException();
        }

        public void webViewUnfocus(WebView Sender)
        {
        }

        public void willPerformDragDestinationAction(WebView WebView, WebDragDestinationAction Action, IDataObject DraggingInfo)
        {
        }

        public IDataObject willPerformDragSourceAction(WebView WebView, WebDragSourceAction Action, ref tagPOINT Point, IDataObject Pasteboard)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
