using System;
using System.Text;

namespace L_SecureFT.KeyGen
{
    public class LibertasKeyGen
    {
        // Just a bloadcode I've made to generate a unique Key File for the app
        // It does matter what is inside the key since the app just reads the inside of the file and use it as a encryption key but this just looks cool so I've done it.
        public static string GenerateKeyFile(int nl)
        {
            StringBuilder sb = new StringBuilder();

            const string alph = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+{}";
            int numberOfLetters = nl;
            int alphabetLength = alph.Length;

            for (int i = 0; i < numberOfLetters; i++)
            {
                sb.Append(alph[i % alphabetLength]);
            }
            string Key = ToBase64(ToBase64(sb.ToString()));
            
            return Key;
        }

        static string ToBase64(string input)
        {
            byte[] byte_A = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(byte_A);
        }
    }
}
