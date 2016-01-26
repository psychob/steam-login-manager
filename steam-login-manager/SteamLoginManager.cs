using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace steam_login_manager
{
    public partial class fSteamLoginManager : Form
    {
        private List<SteamLogin> logins = new List<SteamLogin>();
        private string dbPassword = "";

        private string SteamPath
        {
            get
            {
                return tbSteamPath.Text;
            }

            set
            {
                tbSteamPath.Text = value;
            }
        }

        private string DatabasePath
        {
            get
            {
                return textBox2.Text;
            }

            set
            {
                textBox2.Text = value;
            }
        }

        private bool HideInTray
        {
            get
            {
                return checkBox1.Checked;
            }

            set
            {
                checkBox1.Checked = value;
            }
        }

        public fSteamLoginManager()
        {
            InitializeComponent();
        }

        private void fSteamLoginManager_Load(object sender, EventArgs e)
        {
            string lastPath = "";
            bool run = true;

            if (!SteamLoginLoader.GetLastPath(out lastPath))
            {
                if (SteamLoginLoader.HaveDefaultDatabase())
                    lastPath = SteamLoginLoader.GetDefaultDatabase();
                else
                    lastPath = "";
            }

            fCreateOpenDatabase db = new fCreateOpenDatabase();

            if (lastPath == "")
            {
                db.Type = fCreateOpenDatabase.CreateType.Create;
                db.Path = SteamLoginLoader.GetDefaultDatabase();
            }
            else
            {
                db.Type = fCreateOpenDatabase.CreateType.Open;
                db.Path = lastPath;
            }

            do
            {
                db.ShowDialog(this);

                if (!db.IsValidTicket)
                {
                    Close();
                    return;
                }

                switch (db.Type)
                {
                    case fCreateOpenDatabase.CreateType.Create:
                        DatabasePath = db.Path;
                        dbPassword = db.Password;
                        HideInTray = true;

                        run = false;
                        break;

                    case fCreateOpenDatabase.CreateType.Open:
                        {
                            string steamPath = "";
                            bool hide = true;
                            if (SteamLoginLoader.IsValidDatabase(db.Path, db.Password,
                                out logins, out steamPath, out hide))
                            {
                                DatabasePath = db.Path;
                                dbPassword = db.Password;
                                SteamPath = steamPath;
                                HideInTray = hide;

                                run = false;
                            }
                            else
                                MessageBox.Show(String.Format("Can't open file: {0}!", db.Path));
                        }
                        break;
                }
            } while (run || !db.IsValidTicket);

            SteamLoginLoader.SaveLastLocation(DatabasePath);

            PopulateList();
        }

        private void PopulateList()
        {
            foreach (var it in logins)
            {
                AddNewEntry(it.Name, it.Login, it.Password, it.LastAccess, false);
            }
        }

        private void fSteamLoginManager_FormClosed(object sender, FormClosedEventArgs e)
        {
            SteamLoginLoader.SaveDatabase(DatabasePath, dbPassword, SteamPath,
                logins, HideInTray);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "Steam Exec (steam.exe)|steam.exe";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SteamPath = ofd.FileName;
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAddEditEntry add = new fAddEditEntry();

            add.Type = fAddEditEntry.EntryType.Add;

            add.ShowDialog(this);

            if (!add.IsValidEntry)
                return;

            AddNewEntry(add.EntryName, add.EntryLogin, add.EntryPassword,
                DateTime.Now);
        }

        private void AddNewEntry(string name, string login, string password,
            DateTime now, bool AddToLogins = true)
        {
            listView1.Items.Add(new ListViewItem(new string[]
            {
                    login,
                    "*".Repeat(password.Length),
                    now.ToString(),
                    name
            }));

            if (AddToLogins)
                logins.Add(new SteamLogin(login, password, name, now, 1));
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SteamLoginLoader.SaveDatabase(DatabasePath, dbPassword, SteamPath, logins,
                HideInTray);
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                var first = listView1.SelectedItems[0];

                RunSteam(first.SubItems[0].Text);
            }
        }

        private void RunSteam(string login)
        {
            if (SteamPath.IsEmpty())
            {
                MessageBox.Show("Enter steam path!");
                return;
            }

            int entry_id = logins.FindIndex(p => p.Login == login);
            var entry = logins[entry_id];

            entry.LastAccess = DateTime.Now;
            entry.Count++;

            logins[entry_id] = entry;

            StartSteam(SteamPath, entry.Login, entry.Password);
        }

        private void StartSteam(string steamPath, string login, string password)
        {
            if (SteamIsRunning())
            {
                if (MessageBox.Show(this, "Steam is currently running, do you want to close it?",
                    "Steam is running!", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;

                CloseSteam();

                WaitForSteamClose();
            }

            Process prc = new Process();

            prc.StartInfo.FileName = steamPath;
            prc.StartInfo.Arguments = String.Format("-login \"{0}\" \"{1}\"",
                login.Escape(), password.Escape());

            prc.Start();
        }

        private void edutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
            {
                return;
            }

            var first = listView1.SelectedItems[0];
            var entryId = logins.FindIndex(p => p.Login == first.SubItems[0].Text);
            var entry = logins[entryId];

            fAddEditEntry edit = new fAddEditEntry();

            edit.Type = fAddEditEntry.EntryType.Edit;

            edit.EntryName = entry.Name;
            edit.EntryLogin = entry.Login;
            edit.EntryPassword = entry.Password;

            edit.ShowDialog(this);

            if (!edit.IsValidEntry)
                return;

            entry.Name = edit.EntryName;
            entry.Login = edit.EntryLogin;
            entry.Password = edit.EntryPassword;

            logins.RemoveAt(entryId);
            logins.Add(entry);

            listView1.Items.Clear();

            PopulateList();
        }

        private void CloseSteam()
        {
            Process prc = new Process();

            prc.StartInfo.FileName = SteamPath;
            prc.StartInfo.Arguments = "-exitsteam";

            prc.Start();
        }

        private bool SteamIsRunning()
        {
            var proc = Process.GetProcesses();

            foreach (var it in proc)
            {
                try
                {
                    if (it.MainModule.FileName.Equals(SteamPath, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
                catch (Exception)
                {
                    // ignorujemy błędy pobrania głównego modułu, bo to pewnie
                    // nie steam
                }
            }

            return false;
        }

        private void WaitForSteamClose()
        {
            var proc = Process.GetProcesses();

            foreach (var it in proc)
            {
                try
                {
                    if (it.MainModule.FileName.Equals(SteamPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        it.WaitForExit();
                        WaitForSteamClose();
                        return;
                    }
                }
                catch (Exception)
                {
                    // ignorujemy błędy pobrania głównego modułu, bo to pewnie
                    // nie steam
                }
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
            {
                return;
            }

            var first = listView1.SelectedItems[0];
            logins.RemoveAll(p => p.Login == first.SubItems[0].Text);
            listView1.Items.Clear();

            PopulateList();
        }

        private void fSteamLoginManager_Resize(object sender, EventArgs e)
        {
            if (HideInTray)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    niIcon.Visible = true;
                    niIcon.ShowBalloonTip(500);
                    Hide();
                }
            }
        }

        private void niIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (HideInTray)
            {
                Show();
                WindowState = FormWindowState.Normal;
                niIcon.Visible = false;
            }
        }
    }
}
