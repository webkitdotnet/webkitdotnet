using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WebKit;

namespace WebKitBrowserTest
{
    public partial class DownloadForm : Form
    {
        private WebKitDownload Download;
        private long size, recv;

        public DownloadForm(WebKitDownload Download)
        {
            this.Download = Download;
            InitializeComponent();

            this.Visible = false;

            Download.DownloadStarted += new DownloadStartedEventHandler(Download_DownloadStarted);
            Download.DownloadReceiveData += new DownloadReceiveDataEventHandler(Download_DownloadReceiveData);
            Download.DownloadFinished += new DownloadFinishedEventHandler(Download_DownloadFinished);
        }

        void Download_DownloadFinished(object sender, EventArgs args)
        {
            progressBar1.Value = progressBar1.Maximum;
            label2.Text = "Finished!";
        }

        void Download_DownloadReceiveData(object sender, DownloadReceiveDataEventArgs args)
        {
            recv += args.Length;
            label2.Text = recv.ToString() + " / " + size.ToString() + " bytes downloaded";
            progressBar1.Value = (int)((((float)recv) / ((float)size)) * progressBar1.Maximum);
        }

        void Download_DownloadStarted(object sender, DownloadStartedEventArgs args)
        {
            if (MessageBox.Show("Download file " + args.SuggestedFileName + "?", "Download", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                size = args.FileSize;
                label1.Text = args.SuggestedFileName;
                this.Text = "Download " + args.SuggestedFileName;
                label2.Text = "0";
                this.Show();
            }
            else
            {
                Download.Cancel();
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Download.Cancel();
            this.Close();
        }
    }
}
