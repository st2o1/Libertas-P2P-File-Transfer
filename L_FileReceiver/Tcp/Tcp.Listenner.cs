using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;

namespace L_FileReceiver.Tcp
{
    public class Tcp_Listenner
    {
        public event Action<string> FileReceived;

        private TcpListener _listener;
        private bool _isListening;

        private List<TcpListener> _listeners = new List<TcpListener>(); // All listeners

        public async Task Listen(int p, bool chkbox, bool usekey, string pwd) // Start listening func
        {
            _listener = new TcpListener(IPAddress.Any, p);

            _listeners.Add(_listener);

            _listener.Start();
            _isListening = true;
            await DataStream(_listener, chkbox, usekey, pwd);
        }

        public void StopListening() // Stop listening func
        {
            List<TcpListener> listenersToStop; // Locking all listenners so it does not produce errors
            lock (_listeners)
            {
                listenersToStop = new List<TcpListener>(_listeners);
                _listeners.Clear();
            }

            foreach (var listener in listenersToStop)
            {
                listener.Stop();
            }


            _isListening = false;
        }


        // Network data => DATA receiver
        // This is a basic receiver i've made, I do not know if there are potential vulnerabilities over this,
        // so correct it if needed. Thank you
        private async Task DataStream(TcpListener listener, bool loggggg, bool usekey, string d_pwd)
        {
            string pcName;
            string f_size;

            // Create Log directory.
            string logDir = Path.Combine(Directory.GetCurrentDirectory(), "[Logs]");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }



            // it is basically a foreach for this class
            // ( it will read data foreach files that TCP receives.))
            while (_isListening)
            {
                try
                {
                    using (TcpClient client = await listener.AcceptTcpClientAsync())
                    using (NetworkStream networkStream = client.GetStream())
                    {
                        // TCP sender info
                        byte[] pcNameLengthBytes = new byte[4];
                        await networkStream.ReadAsync(pcNameLengthBytes, 0, pcNameLengthBytes.Length);
                        int pcNameLength = BitConverter.ToInt32(pcNameLengthBytes, 0);

                        byte[] pcNameBytes = new byte[pcNameLength];
                        await networkStream.ReadAsync(pcNameBytes, 0, pcNameBytes.Length);
                        pcName = Encoding.UTF8.GetString(pcNameBytes);

                        // Create receiving Directory
                        string receivedDir = Path.Combine(Directory.GetCurrentDirectory(), "[Received]", pcName);
                        if (!Directory.Exists(receivedDir))
                        {
                            Directory.CreateDirectory(receivedDir);
                        }

                        // TCP file size 
                        byte[] f_sizeLengthBytes = new byte[4];
                        await networkStream.ReadAsync(f_sizeLengthBytes, 0, f_sizeLengthBytes.Length);
                        int f_sizeLength = BitConverter.ToInt32(f_sizeLengthBytes, 0);

                        byte[] f_sizebytes = new byte[f_sizeLength];
                        await networkStream.ReadAsync(f_sizebytes, 0, f_sizebytes.Length);
                        f_size = Encoding.UTF8.GetString(f_sizebytes);
                        if (usekey) { f_size = Decryption.Decrypt(f_size, d_pwd); }

                        // TCP file name,
                        byte[] fileNameLengthBytes = new byte[4];
                        await networkStream.ReadAsync(fileNameLengthBytes, 0, fileNameLengthBytes.Length);
                        int fileNameLength = BitConverter.ToInt32(fileNameLengthBytes, 0);

                        byte[] fileNameBytes = new byte[fileNameLength];
                        await networkStream.ReadAsync(fileNameBytes, 0, fileNameBytes.Length);
                        string fileName = Encoding.UTF8.GetString(fileNameBytes);
                        if (usekey) { fileName = Decryption.Decrypt(fileName, d_pwd); }

                        // Receive b64 data from stream.
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            await networkStream.CopyToAsync(memoryStream);
                            byte[] base64Bytes = memoryStream.ToArray();
                            string base64String = Encoding.UTF8.GetString(base64Bytes);
                            if (usekey) { base64String = Decryption.Decrypt(base64String, d_pwd); }
                            byte[] fileBytes = Convert.FromBase64String(base64String);
                            
                            // Save the file
                            string filePath = Path.Combine(receivedDir, fileName);
                            File.WriteAllBytes(filePath, fileBytes);

                            FileReceived?.Invoke(pcName);
                        }
                    }
                    if (loggggg)
                    {
                        using (StreamWriter sw = File.AppendText(Path.Combine(logDir, $"Logs From {pcName}.txt")))
                        {
                            sw.WriteLine("1 file received.  Total size of : " + f_size + $" @ {DateTime.Now.ToString("h-mm-ss tt")}");
                        }
                    }
                }
                catch (ObjectDisposedException) { } // Leave if stopped
                catch (Exception e)
                {
                    if (loggggg)
                    {
                        using (StreamWriter sw = File.AppendText(Path.Combine(logDir, $"ERROR_LOGS.txt")))
                        {
                            sw.WriteLine($"Err : {e.Message}" + $" @ {DateTime.Now.ToString("h-mm-ss tt")}\n");
                        } // write errors into an error file list
                    }
                }
                // write info about the received files into logs
            }
        }
    }
}
