using System;
using System.IO;
using System.Windows.Forms;

namespace L_SecureFT.KeyGen
{
    public partial class KeyGenForm : Form
    {
        public KeyGenForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int alph_num;

            if (radioButton1.Checked)
            {
                alph_num = 128;
            }
            else if (radioButton2.Checked)
            {
                alph_num = 256;
            }
            else
            {
                alph_num = 512;
            }

            string Keydata = LibertasKeyGen.GenerateKeyFile(alph_num);

            string keyfile_n = GetUniqueFileName(AppDomain.CurrentDomain.BaseDirectory, "Cert.LKC"); // save the key as a Libertas Key Certificate file.

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + keyfile_n, Keydata);
            Close();
        }


        public static string GetUniqueFileName(string directory, string fileName) // File name helper
        {
            string filePath = Path.Combine(directory, fileName);
            if (!File.Exists(filePath))
            {
                return fileName;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            int counter = 1;

            while (File.Exists(filePath))
            {
                string newFileName = $"{fileNameWithoutExtension} ({counter}){extension}";
                filePath = Path.Combine(directory, newFileName);
                counter++;
            }

            return Path.GetFileName(filePath);
        }
    }
}
