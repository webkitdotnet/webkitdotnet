namespace WebKitBrowser.Tests
{
    partial class WebKitBrowserTestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Browser = new WebKit.WebKitBrowser();
            this.SuspendLayout();
            // 
            // Browser
            // 
            this.Browser.BackColor = System.Drawing.Color.White;
            this.Browser.Dock = System.Windows.Forms.DockStyle.Left;
            this.Browser.Location = new System.Drawing.Point(0, 0);
            this.Browser.Name = "Browser";
            this.Browser.Size = new System.Drawing.Size(637, 595);
            this.Browser.TabIndex = 0;
            this.Browser.Url = null;
            // 
            // WebKitBrowserTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1162, 595);
            this.Controls.Add(this.Browser);
            this.Name = "WebKitBrowserTestForm";
            this.Text = "WebKitBrowserTestForm";
            this.ResumeLayout(false);

        }

        #endregion

        public WebKit.WebKitBrowser Browser;
    }
}