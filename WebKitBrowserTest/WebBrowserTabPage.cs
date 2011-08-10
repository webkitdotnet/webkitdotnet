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

using System.Windows.Forms;
using WebKit;
using WebKit.JSCore;
using System;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Reflection;

namespace WebKitBrowserTest
{
    public class TestScriptObject
    {
        public void f()
        {
            MessageBox.Show("Hey!");
        }    
    }

    public class TestClass
    {
        public JSContext ctx { get; set; }

        public void callback(Delegate callback)
        {            
            object[] x = { "first" };
            
            string result = (string) callback.DynamicInvoke(new object[] {x});
            MessageBox.Show(result);
            var worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                Thread.Sleep(1000);
            };
            worker.RunWorkerCompleted += delegate
            {
                callback.DynamicInvoke(new object[] {x});
            };
            worker.RunWorkerAsync();
        }
        public string x { get; set; }
        public string y { get; set; }
        public double i { get; set; }
        public bool b { get; set; }
    }


    public partial class WebBrowserTabPage : TabPage
    {
        public WebKitBrowser browser;
        
        private StatusStrip statusStrip;
        private ToolStripLabel statusLabel;
        private ToolStripLabel iconLabel;
        private ToolStripContainer container;
        
        public WebBrowserTabPage() : this(new WebKitBrowser(), true)
        {
        }
        
        public WebBrowserTabPage(WebKitBrowser browserControl, bool goHome)
        {
            InitializeComponent();

            statusStrip = new StatusStrip();
            statusStrip.Name = "statusStrip";
            statusStrip.Visible = true;
            statusStrip.SizingGrip = false;

            container = new ToolStripContainer();
            container.Name = "container";
            container.Visible = true;
            container.Dock = DockStyle.Fill;

            statusLabel = new ToolStripLabel();
            statusLabel.Name = "statusLabel";
            statusLabel.Text = "Done";
            statusLabel.Visible = true;

            iconLabel = new ToolStripLabel();
            iconLabel.Name = "iconLabel";
            iconLabel.Text = "No Icon";
            iconLabel.Visible = true;

            statusStrip.Items.Add(statusLabel);
            statusStrip.Items.Add(iconLabel);

            container.BottomToolStripPanel.Controls.Add(statusStrip);

            // create webbrowser control
            browser = browserControl;
            browser.Visible = true;
            browser.Dock = DockStyle.Fill;
            browser.Name = "browser";
            //browser.IsWebBrowserContextMenuEnabled = false;
            //browser.IsScriptingEnabled = false;
            container.ContentPanel.Controls.Add(browser);

            browser.ObjectForScripting = new TestClass();

            // context menu

            this.Controls.Add(container);
            this.Text = "<New Tab>";

            // events
            browser.DocumentTitleChanged += (s, e) => this.Text = browser.DocumentTitle;
            browser.Navigating += (s, e) => statusLabel.Text = "Loading...";
            browser.Navigated += (s, e) => { statusLabel.Text = "Downloading..."; };
            browser.DocumentCompleted += (s, e) => { statusLabel.Text = "Done"; };
            if (goHome)
            {
                string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                string htmlPath = Path.Combine(appPath, "index.html");
                browser.Navigate(Uri.EscapeUriString(new Uri(htmlPath).ToString()));
            }
            browser.ShowJavaScriptAlertPanel += (s, e) => MessageBox.Show(e.Message, "[JavaScript Alert]");
            browser.ShowJavaScriptConfirmPanel += (s, e) =>
            {
                e.ReturnValue = MessageBox.Show(e.Message, "[JavaScript Confirm]", MessageBoxButtons.YesNo) == DialogResult.Yes;
            };
            browser.ShowJavaScriptPromptPanel += (s, e) =>
            {
                var frm = new JSPromptForm(e.Message, e.DefaultValue);
                if (frm.ShowDialog() == DialogResult.OK)
                    e.ReturnValue = frm.Value;
            };
        }

        public void Stop()
        {
            browser.Stop();
            statusLabel.Text = "Stopped";
        }
    }
}
