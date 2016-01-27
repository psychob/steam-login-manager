using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace steam_login_manager
{
    public partial class fCreateOpenDatabase : Form
    {
        public enum CreateType
        {
            Create,
            Open,
        }

        public CreateType Type = CreateType.Create;

        public bool IsValidTicket
        {
            get
            {
                switch (Type)
                {
                    case CreateType.Create:
                        return Password.Length >= 3;

                    case CreateType.Open:
                        return File.Exists(Path) && Password.Length >= 3;
                }

                return false;
            }
        }

        public string Path
        {
            get
            {
                return tbPath.Text;
            }

            set
            {
                tbPath.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return tbPass.Text;
            }

            private set
            {
                tbPass.Text = value;
            }
        }

        public fCreateOpenDatabase()
        {
            InitializeComponent();
        }

        private void fCreateOpenDatabase_Load(object sender, EventArgs e)
        {
            switch (Type)
            {
                case CreateType.Create:
                    Text = "Create New Database";
                    break;

                case CreateType.Open:
                    if (!File.Exists(Path))
                    {
                        Type = CreateType.Create;
                        Text = "Create New Database";
                    }
                    else
                    {
                        Text = "Open Database";
                    }
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (IsValidTicket)
                Close();
        }

        private void tbPass_TextChanged(object sender, EventArgs e)
        {
            string pass = tbPass.Text;

            if (pass.Length < 3)
                btnUnlockDatabase.Enabled = false;
            else
                btnUnlockDatabase.Enabled = true;
        }

        private void tbPass_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                btnUnlockDatabase.PerformClick();
            }
        }
    }
}