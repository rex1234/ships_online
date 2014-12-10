using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ships
{
    public partial class NickDialog : Form
    {
        public string PlayerName { get; set; }

        public NickDialog(string defaultName)
        {
            InitializeComponent();
            nameTextBox.Text = defaultName;
        }

        private void Confirm()
        {
            if (nameTextBox.Text.Length > 0)
            {
                PlayerName = nameTextBox.Text;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            Confirm();
        }

        private void nameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Confirm();
            }
        }

    }
}
