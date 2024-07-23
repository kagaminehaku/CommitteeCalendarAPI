using System.Security.Cryptography;
using System.Text;

namespace CommitteeCalendarAPI.IMPLogic
{
    internal class IMPPWDHashing
    {
        public static string EncryptData(string input)
        {
            using (SHA512 sha512 = SHA512.Create())
            {

                string CustomSalt = "";
                byte[] saltBytes = Encoding.UTF8.GetBytes(CustomSalt);
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] saltedInput = new byte[saltBytes.Length + inputBytes.Length];

                Buffer.BlockCopy(saltBytes, 0, saltedInput, 0, saltBytes.Length);
                Buffer.BlockCopy(inputBytes, 0, saltedInput, saltBytes.Length, inputBytes.Length);

                byte[] hashBytes = sha512.ComputeHash(saltedInput);
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
