using System;
using System.IO;
using System.Security.Cryptography;

namespace Hfs.Server.CODICE.CLASSI
{
    /// <summary>
    /// Classe per la criptazione di stringhe
    /// </summary>
    public class CryptoUtils
    {

        public static byte[] GetSalt(int sSize)
        {
            using (var random = new RNGCryptoServiceProvider())
            {
                // Empty salt array
                byte[] salt = new byte[sSize];

                // Build the random bytes
                random.GetNonZeroBytes(salt);

                return salt;
            }
        }

        /// <summary>
        /// Cripta array di bytes
        /// </summary>
        /// <param name="plain"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] EncryptBytes(byte[] plain, string password)
        {
            using (var rijndael = Rijndael.Create())
            {
                var salt = GetSalt(rijndael.IV.Length);
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt);
                rijndael.Key = pdb.GetBytes(32);
                rijndael.GenerateIV(); //IV Casuale

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    //Scrive salt
                    memoryStream.Write(salt, 0, salt.Length);
                    //Scrive iv
                    memoryStream.Write(rijndael.IV, 0, rijndael.IV.Length);

                    //Scrive contenuto
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plain, 0, plain.Length);
                    }
                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Decripta array di bytes
        /// </summary>
        /// <param name="cipher"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] DecryptBytes(byte[] cipher, string password)
        {
            using (var rijndael = Rijndael.Create())
            {
                //Legge salt
                var salt = new byte[rijndael.IV.Length];
                Array.Copy(cipher, salt, salt.Length);

                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt);
                rijndael.Key = pdb.GetBytes(32);
                //Legge IV
                var iv = new byte[rijndael.IV.Length];
                Array.Copy(cipher, salt.Length, iv, 0, iv.Length);
                rijndael.IV = iv;
                //Calcola offset
                var offset = salt.Length + rijndael.IV.Length;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(cipher, offset, cipher.Length - offset);
                    }
                    return memoryStream.ToArray();
                }
            }

        }

        /// <summary>
        /// Cripta Stringa AES
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptString(string plainText, string password)
        {
            // Check arguments.
            return Convert.ToBase64String(EncryptBytes(System.Text.Encoding.UTF8.GetBytes(plainText), password));
        }

        /// <summary>
        /// Decripta stringa AES
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptString(string cipherText, string password)
        {
            return System.Text.Encoding.UTF8.GetString(DecryptBytes(Convert.FromBase64String(cipherText), password));
        }


        /// <summary>
        /// Genera una nuova chiave RSA (2048)
        /// </summary>
        /// <returns></returns>
        public static string RsaGenerateToXml()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                //Chiave non salvata in store
                rsa.PersistKeyInCsp = false;
     
                return rsa.ToXmlString(true);
            }
        }


        /// <summary>
        /// Genera una nuova chiave RSA (2048)
        /// </summary>
        /// <returns></returns>
        public RSACryptoServiceProvider RsaLoadFromXml(string xml)
        {
            var rsa = new RSACryptoServiceProvider();
            //Chiave non salvata in store
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(xml);
            return rsa;
        }

    }
}