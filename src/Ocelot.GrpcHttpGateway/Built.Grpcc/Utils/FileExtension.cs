using System.Text;

namespace System.IO
{
    public static class FileExtension
    {
        /// <summary>
        /// FileMd5
        /// </summary>
        public static string FileMd5(this string fileName)
        {
            if (!File.Exists(fileName)) return string.Empty;
            byte[] hash;
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                hash = md5.ComputeHash(file);
            }
            if (hash == null) return string.Empty;
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("x2"));
            }
            return result.ToString();
        }
    }
}