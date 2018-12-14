using System.Security.Cryptography;
using System.Text;

namespace servicioCliente.AppUtils
{
    public class Hasher
    {
        /// <summary>
        /// Genera Hash con algoritmo SHA512
        /// </summary>
        /// <param name="key">Texto a cifrar con hash</param>
        /// <returns>Hash del texto</returns>
        public static string GenerateSHA512Hash(string key){
            SHA512 sha512 = SHA512.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(key);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }
        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }
    }
}