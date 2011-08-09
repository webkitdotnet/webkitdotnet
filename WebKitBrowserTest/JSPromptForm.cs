using System;
using System.Windows.Forms;

namespace WebKitBrowserTest
{
    public partial class JSPromptForm : Form
    {
        public string Value
        {
            get
            {
                return textBox1.Text;
            }
        }

        public JSPromptForm(string message, string defaultValue)
        {
            InitializeComponent();

            label1.Text = message;
            textBox1.Text = defaultValue;
            this.Text = "[JavaScript Prompt Form]";
        }

        private void JSPromptForm_Load(object sender, EventArgs e)
        {

        }
    }
}
