using System;
using System.Net.Sockets;
using System.Text;

namespace L_SecureFT.Senderlib
{
    public class Sender
    {
        public static void TCP_Send(string n, string f_size, string base64string, string ip, int p)
        {
            string Sender_info = "[" + Environment.MachineName + "]=[" + Environment.UserName + "]";

            using (TcpClient client = new TcpClient(ip, p))
            using (NetworkStream networkStream = client.GetStream())
            {
                byte[] pcNameBytes = Encoding.UTF8.GetBytes(Sender_info);
                byte[] pcNameLengthBytes = BitConverter.GetBytes(pcNameBytes.Length);
                networkStream.Write(pcNameLengthBytes, 0, pcNameLengthBytes.Length);
                networkStream.Write(pcNameBytes, 0, pcNameBytes.Length);

                byte[] f_sizebytes = Encoding.UTF8.GetBytes(f_size);
                byte[] f_sizeLengthBytes = BitConverter.GetBytes(f_sizebytes.Length);
                networkStream.Write(f_sizeLengthBytes, 0, f_sizeLengthBytes.Length);
                networkStream.Write(f_sizebytes, 0, f_sizebytes.Length);

                byte[] fileNameBytes = Encoding.UTF8.GetBytes(n);
                byte[] fileNameLength = BitConverter.GetBytes(fileNameBytes.Length);
                networkStream.Write(fileNameLength, 0, fileNameLength.Length);
                networkStream.Write(fileNameBytes, 0, fileNameBytes.Length);

                byte[] base64Bytes = Encoding.UTF8.GetBytes(base64string);
                networkStream.Write(base64Bytes, 0, base64Bytes.Length);
            }
        }

    }
}
