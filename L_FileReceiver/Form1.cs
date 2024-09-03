using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using L_FileReceiver.Tcp;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace L_FileReceiver
{
    public partial class Form1 : Form
    {
        string appversion = "1.0";
        
        private Tcp_Listenner _listener = new Tcp_Listenner();

        private Dictionary<string, int> userFileCounts = new Dictionary<string, int>();
        private const string UserDataFile = "received.DB_Libertas";

        bool use_encryption = false;
        private string CertificatKey_; // the password from the key file ( use to decrypt the data that you receive. )
        private bool usecert_ = false;

        public Form1()
        {
            InitializeComponent();
            Text = Text + " v" + appversion;

            button4.Enabled = false;
            LoadUserData();
            PopulateListView();
        }


        // don't allow anything other than numbers in the textbox for ports.
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        #region Buttons
        private void button1_Click(object sender, EventArgs e)
        {
            string p = textBox1.Text;

            if (p != String.Empty)
            {
                listView2.Items.Add(p);
            }
            textBox1.Clear();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            while (listView2.SelectedItems.Count > 0)
            {
                var r = listView2.SelectedItems[0];
                listView2.Items.Remove(r);

                string p_string = r.SubItems[0].Text;
            }
        }
        private async void button3_Click(object sender, EventArgs e)
        {
            bool usekey = use_encryption;

            button4.Enabled = true;
            
            button3.Enabled = false;
            checkBox1.Enabled = false;
            checkBox3.Enabled = false;
            textBox2.Enabled = false;

            List<Task> listenTasks = new List<Task>();
            _listener.FileReceived += UpdateUserData;

            string pwd = String.Empty; // txtbox2

            if (usecert_)
            {
                pwd = CertificatKey_;
            }
            else
            {
                pwd = textBox2.Text;
            }

            foreach (ListViewItem item in listView2.Items)
            {
                string port = item.SubItems[0].Text;
                int portNum = int.Parse(port);

                bool loggggdata = false;
                if (checkBox1.Checked)
                {
                    loggggdata = true;
                }

                //await _listener.Listen(portNum, logggggdata, usekey, pwd); <- is THE basic function to listen
                listenTasks.Add(_listener.Listen(portNum, loggggdata, usekey, pwd));
            }
            await Task.WhenAll(listenTasks);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            
            button3.Enabled = true;
            checkBox1.Enabled = true;
            checkBox3.Enabled = true;
            textBox2.Enabled = true;

            if (checkBox1.Checked)
            {
                _listener.FileReceived -= UpdateUserData;
            }
            _listener.StopListening();
        }
        #endregion

        #region DB management

        // I tried to make it as easier for the user as possible.
        // modify if you want to change how File count is handled.
        public void UpdateUserData(string username)
        {
            if (userFileCounts.ContainsKey(username))
            {
                userFileCounts[username]++;
            }
            else
            {
                userFileCounts[username] = 1;
            }
            if (checkBox1.Checked)
            {
                SaveUserData();
            }
            PopulateListView();
        }

        private void SaveUserData()
        {
            var lines = new List<string>();
            foreach (var entry in userFileCounts)
            {
                lines.Add($"{entry.Key},{entry.Value}");
            }
            File.WriteAllLines(UserDataFile, lines);
        }

        private void LoadUserData()
        {
            if (File.Exists(UserDataFile))
            {
                var lines = File.ReadAllLines(UserDataFile);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2)
                    {
                        var username = parts[0];
                        if (int.TryParse(parts[1], out int fileCount))
                        {
                            userFileCounts[username] = fileCount;
                        }
                    }
                }
            }
        }

        private void PopulateListView()
        {
            listView1.Items.Clear();
            foreach (var entry in userFileCounts)
            {
                var item = new ListViewItem(entry.Key);
                item.SubItems.Add(entry.Value.ToString());
                listView1.Items.Add(item);
            }
        }

        #endregion

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(listView1.SelectedItems.Count > 0 && listView1.SelectedItems.Count < 2) 
            {
                var chosen_item = listView1.SelectedItems[0].Text;
                string folder = Path.Combine("[Received]", chosen_item);

                Process.Start("explorer.exe ", $"\"{folder}\"");
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                textBox2.Enabled = false;
                usecert_ = true;
                button5.Enabled = true;
            }
            else if (!checkBox2.Checked)
            {
                textBox2.Enabled = true;
                usecert_ = false;
                button5.Enabled = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Libertas Key Certificate (*.LKC)|*.LKC";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    CertificatKey_ = File.ReadAllText(filePath);
                }
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                textBox2.Enabled = true;
                checkBox2.Enabled = true;
                use_encryption = true;
            }
            else
            {
                textBox2.Enabled = false;
                checkBox2.Enabled = false;
                use_encryption = false;
            }
        }
    }
}
