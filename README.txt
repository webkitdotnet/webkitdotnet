WebKit.NET README
*****************

WebKit.NET is a control library wrapper for WebKit written in C#.

Currently the source code is licensed under a BSD open source license, see
LICENSE.txt for details.

This package currently contains several Visual Studio 2010 projects and a build
of the WebKit and JavaScriptCore libraries (including dependencies). The 
solution is organised into the following folders:

    include\              WebKit and JavaScriptCode header files used in JSCore.

    JSCore\               C++/CLI wrapper for JavaScriptCore.

    JSCore.Tests\         Test suite for JSCore.

    lib\                  Intermediate and miscellaneous libraries.

    tools\                Build tools.
    
    webkit                WebKit, JavaScriptCore libraries and dependencies.

    WebKitBrowser\        C# WinForms control library front-end.

    WebKitBrowser.Tests\  Test suite for WebKitBrowser.
    
    WebKitBrowserTest\    A simple application that uses the WebKit.NET control
                          to display web pages.

    WebKitCore\           C# wrapper for WebKit.

Tutorials & documentation can be found at http://webkitdotnet.sourceforge.net.
Latest source code can be found on GitHub at https://github.com/webkitdotnet.

Please send any questions or feedback to webkitdotnet@peterdn.com.
