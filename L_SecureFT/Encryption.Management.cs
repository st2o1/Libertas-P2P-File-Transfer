using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace L_SecureFT
{
    public class Encryption
    {
        public static string Encrypt(string data, string encryptionkey)
        {
            string encrypted_data = "";


            byte[] key = Encoding.UTF8.GetBytes(encryptionkey);
            Array.Resize(ref key, 32);

            using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
            {
                aesProvider.Key = key;
                aesProvider.GenerateIV();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(aesProvider.IV, 0, aesProvider.IV.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesProvider.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(data);
                        cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                    }

                    encrypted_data = Convert.ToBase64String(memoryStream.ToArray());

                }
            }

            return encrypted_data;
        }
    }
}
