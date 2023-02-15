using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicFacebookFeatures
{
    public partial class FormEventDirections : Form
    {
        private Form m_FormMain;

        public FormEventDirections(Form i_FormMain)
        {
            InitializeComponent();
            m_FormMain = i_FormMain;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            m_FormMain.Show();
        }

        private void webBrowserEventsDirection_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}
