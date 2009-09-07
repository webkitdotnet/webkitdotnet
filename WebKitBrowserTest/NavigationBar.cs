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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace WebKitBrowserTest
{
    public delegate void Default();

    public partial class NavigationBar : UserControl
    {
        public event Default Back;
        public event Default Forward;
        public new event Default Refresh;
        public event Default Stop;
        public event Default Home;
        public event Default Go;

        public string UrlText
        {
            get
            {
                return comboBoxAddress.Text;
            }
            set
            {
                comboBoxAddress.Text = value;
            }
        }

        public bool CanGoBack
        {
            set
            {
                buttonBack.Enabled = value;
            }
        }

        public bool CanGoForward
        {
            set
            {
                buttonFwd.Enabled = value;
            }
        }

        public NavigationBar()
        {
            InitializeComponent();

            // null event handlers
            Back += () => { };
            Forward += () => { };
            Refresh += () => { };
            Stop += () => { };
            Home += () => { };
            Go += () => { };
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            Back();
        }

        private void buttonFwd_Click(object sender, EventArgs e)
        {
            Forward();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            Refresh();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void buttonHome_Click(object sender, EventArgs e)
        {
            Home();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Go();
        }

        private void comboBoxAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                e.Handled = true;
                Go();
            }
        }
    }
}
