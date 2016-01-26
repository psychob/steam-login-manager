using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace steam_login_manager
{
    public partial class fAddEditEntry : Form
    {
        public enum EntryType
        {
            Add,
            Edit,
        }

        public EntryType Type = EntryType.Add;

        private bool Canceling = false;

        public bool IsValidEntry
        {
            get
            {
                return EntryName.Length > 0 && EntryLogin.Length > 0 &&
                    EntryPassword.Length > 0 && !Canceling;
            }
        }

        public string EntryName
        {
            get
            {
                return tbName.Text;
            }

            set
            {
                tbName.Text = value;
            }
        }

        public string EntryLogin
        {
            get
            {
                return tbLogin.Text;
            }

            set
            {
                tbLogin.Text = value;
            }
        }

        public string EntryPassword
        {

            get
            {
                return tbPassword.Text;
            }

            set
            {
                tbPassword.Text = value;
            }
        }

        public fAddEditEntry()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Canceling = true;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Canceling = false;

            if (IsValidEntry)
                Close();
        }
    }
}
