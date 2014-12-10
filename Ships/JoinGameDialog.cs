using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ships
{
    public partial class JoinGameDialog : Form
    {
        public IPAddress Ip 
        { 
            get { return ip; } 
            private set { Ip = value; } 
        }
        private IPAddress ip;

        public JoinGameDialog()
        {
            InitializeComponent();            
        }

        private void Join()
        {
            if (IPAddress.TryParse(ipTextBox.Text, out ip))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }   
    
        private void joinButton_Click(object sender, EventArgs e)
        {
            Join();
        }


        private void ipTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Join();
            }
        }

    }
}
