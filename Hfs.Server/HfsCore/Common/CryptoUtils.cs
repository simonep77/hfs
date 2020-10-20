using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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
        /// Cripta array di bytes attraverso una password 
        /// </summary>
        /// <param name="plain"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] AesEncryptBytes(byte[] plain, byte[] password)
        {
            using (var rijndael = Rijndael.Create())
            {
                var salt = GetSalt(rijndael.IV.Length);
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt, 1000);
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
        /// Decripta array di byte
        /// </summary>
        /// <param name="cipher"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] AesDecryptBytes(byte[] cipher, byte[] password)
        {
            using (var rijndael = Rijndael.Create())
            {
                //Legge salt
                var salt = new byte[rijndael.IV.Length];
                Array.Copy(cipher, salt, salt.Length);

                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt, 1000);
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
        /// Cripta array di byte
        /// </summary>
        /// <param name="plain"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] AesEncryptBytes(byte[] plain, string password)
        {
            return AesEncryptBytes(plain, Encoding.UTF8.GetBytes(password));
        }

        /// <summary>
        /// Decripta array di bytes
        /// </summary>
        /// <param name="cipher"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] AesDecryptBytes(byte[] cipher, string password)
        {
            return AesDecryptBytes(cipher, Encoding.UTF8.GetBytes(password));
        }

        /// <summary>
        /// Cripta Stringa AES
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AesEncryptString(string plainText, string password)
        {
            // Check arguments.
            return Convert.ToBase64String(AesEncryptBytes(System.Text.Encoding.UTF8.GetBytes(plainText), password));
        }

        /// <summary>
        /// Decripta stringa AES
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AesDecryptString(string cipherText, string password)
        {
            return System.Text.Encoding.UTF8.GetString(AesDecryptBytes(Convert.FromBase64String(cipherText), password));
        }

        /// <summary>
        /// Cripta buffer codificato AES attraverso chiave RSA (chiave AES random)
        /// </summary>
        /// <param name="plain"></param>
        /// <param name="rsa"></param>
        /// <returns></returns>
        public static byte[] RsaAesEncryptBytes(byte[] plain, RSACryptoServiceProvider rsa)
        {
            //Genera chiave random di encrypt
            var key = GetSalt(16);
            //La cripta con la chiave RSA
            var encKey = rsa.Encrypt(key, false);
            //Scrive buffer con lunghezza chiave codificata
            var encKeyLen = BitConverter.GetBytes(encKey.Length);
            //Esegue encrypt
            var encData = AesEncryptBytes(plain, key);
            //Ritorna buffer concatenati
            return Combine(encKeyLen.Concat(encKey).Concat(encData).ToArray(), encData);
        }


        /// <summary>
        /// Decripta buffer codificato AES attraverso chiave RSA (chiave AES random)
        /// </summary>
        /// <param name="cipher"></param>
        /// <param name="rsa"></param>
        /// <returns></returns>
        public static byte[] RsaAesDecryptBytes(byte[] cipher, RSACryptoServiceProvider rsa)
        {
            var encKeyLen = BitConverter.ToInt32(cipher, 0);
            var encKey = cipher.Skip(sizeof(Int32)).Take(encKeyLen).ToArray();
            var decKey = rsa.Decrypt(encKey, false);
            return AesDecryptBytes(cipher.Skip(sizeof(Int32) + encKeyLen).ToArray(), decKey);
        }

        /// <summary>
        /// Dati 2 buffer ne restituisce 1 comprensivo di entrambi
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
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