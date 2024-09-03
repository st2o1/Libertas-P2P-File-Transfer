using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using L_SecureFT.Senderlib;
using L_SecureFT.KeyGen;

namespace L_SecureFT
{
    public partial class MainGUI : Form
    {
        string appversion = "1.0";

        private HashSet<string> addedPaths;

        private string CertificatKey_; // the password from the key file ( use to encrypt the data that you send. )

        private bool use_encryption = false;
        private bool usecert_ = false;

        public MainGUI()
        {
            InitializeComponent();
            Text = Text + " v" + appversion;

            textBox2.KeyPress += textBox2_KeyPress;
            addedPaths = new HashSet<string>();

            UpdateTotalFileSize();
        }

        private void button1_Click(object sender, EventArgs e)       // Add file
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        if (addedPaths.Contains(filePath))
                        {
                            continue;
                        }

                        FileInfo fileInfo = new FileInfo(filePath);
                        string fileSize = FormatFileSize(fileInfo.Length);

                        ListViewItem item = new ListViewItem
                        {
                            Text = Path.GetFileName(filePath)
                        };
                        item.SubItems.Add(fileSize);
                        item.SubItems.Add(filePath);

                        listView1.Items.Add(item);
                        addedPaths.Add(filePath);
                    }
                    UpdateTotalFileSize();
                }
            }
        }
        private void UpdateTotalFileSize()
        {
            long totalSize = 0;
            foreach (ListViewItem item in listView1.Items)
            {
                string filePath = item.SubItems[2].Text;
                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    totalSize += fileInfo.Length;
                }
            }

            label4.Text = $"Total Size: {FormatFileSize(totalSize)}";
        }



        // size format
        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            else if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:0.##} KB";
            else if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024):0.##} MB";
            else
                return $"{bytes / (1024.0 * 1024 * 1024):0.##} GB";
        }


        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            while (listView1.SelectedItems.Count > 0)
            {
                string path = listView1.SelectedItems[0].SubItems[2].Text;
                addedPaths.Remove(path);

                listView1.Items.Remove(listView1.SelectedItems[0]);

            }
            UpdateTotalFileSize();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem item = listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0] : null;

            if (item != null)
            {
                string filePath = item.SubItems[1].Text;

                if (File.Exists(filePath))
                {
                    OpenFileLocationAndSelect(filePath);
                }
            }
        }
        private void OpenFileLocationAndSelect(string filePath)
        {
            string argument = $"/select, \"{filePath}\"";
            Process.Start("explorer.exe", argument);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string ipaddr = textBox1.Text;
            int port = int.Parse(textBox2.Text);
            
            int INTEGER_f_number = listView1.Items.Count;
            string f_number = Convert.ToString(INTEGER_f_number);


            string encryptionkey;

            if (usecert_)
            {
                encryptionkey = CertificatKey_;
            }
            else 
            {
                encryptionkey = textBox3.Text;
            }

            foreach (ListViewItem items in listView1.Items)
            {
                // file name
                string f_name = items.SubItems[0].Text;

                // sending size
                string s_size = items.SubItems[1].Text;
                
                // file data
                string f_path = items.SubItems[2].Text;
                byte[] f_bytes = File.ReadAllBytes(f_path);
                string f_b64 = Convert.ToBase64String(f_bytes);


                // encryption method
                if(use_encryption)
                {
                    f_name = Encryption.Encrypt(f_name, encryptionkey);

                    s_size = Encryption.Encrypt(s_size, encryptionkey);
                    
                    f_b64 = Encryption.Encrypt(f_b64, encryptionkey);
                }


#if DEBUG // just some personal debugging stuff (remove if not needed)
                File.WriteAllText(f_name+".b64b.txt", f_b64);
                File.WriteAllText(f_name+".F_name.txt", f_name);
#endif

                try
                {
                    Sender.TCP_Send(f_name, s_size, f_b64, ipaddr, port);
                
                }catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            
            Thread.Sleep(1000); // Sleep for 1 seconds to prevent spam incase of miss-doubleclick
        }

        private void button2_Click(object sender, EventArgs e)
        {
            KeyGenForm k_g = new KeyGenForm();
            k_g.ShowDialog();
        }
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked) 
            {
                textBox3.Enabled = false;
                usecert_ = true;
                button3.Enabled = true;
            }
            else if (!checkBox1.Checked)
            {
                textBox3.Enabled = true;
                usecert_ = false;
                button3.Enabled = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                textBox3.Enabled  = true;
                checkBox1.Enabled = true;
                use_encryption = true;
            }
            else
            {
                textBox3.Enabled = false;
                checkBox1.Enabled = false;
                use_encryption = false;
            }
        }
    }
}
