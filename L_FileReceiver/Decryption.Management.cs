using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace L_FileReceiver
{
    public class Decryption
    {
        public static string Decrypt(string encryptedData, string decryptionKey)
        {
            string decrypted_data = "";

            byte[] key = Encoding.UTF8.GetBytes(decryptionKey);
            Array.Resize(ref key, 32);

            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);

            using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
            {
                aesProvider.Key = key;

                using (MemoryStream memoryStream = new MemoryStream(encryptedBytes))
                {
                    byte[] iv = new byte[aesProvider.BlockSize / 8];
                    memoryStream.Read(iv, 0, iv.Length);
                    aesProvider.IV = iv;

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesProvider.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            decrypted_data = streamReader.ReadToEnd();
                        }
                    }
                }
            }
            return decrypted_data;
        }
    }
}
