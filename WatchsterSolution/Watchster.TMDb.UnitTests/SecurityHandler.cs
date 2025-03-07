﻿using System;
using System.IO;
using System.Security.Cryptography;
using Vanguard;

namespace Watchster.TMDb.UnitTests
{
    public static class SecurityHandler
    {
        internal const string EncryptedTMDbApiKey = "Wx25wXfj9W02nE9Ok03/RaAKYnK4du+o7BcBXUdGiwJYSEIpjgWvZ4/68dwskC4K";
        public const string DefaultKey = "ZyT3JP+1oKBVsSYOsfHsrxrbvn8Crzxo1z+bYglmTEU=";
        public const string DefaultIV = "qdYKwzMiHZ4XB6eVTfhoLw==";

        public static string EncryptStringToBytes(string plainText, string KeyBase64Str, string ivBase64Str)
        {
            var key = Convert.FromBase64String(KeyBase64Str);
            var iv = Convert.FromBase64String(ivBase64Str);

            Guard.ArgumentNotNullOrEmpty(plainText, nameof(plainText));
            Guard.ArgumentNotNullOrEmpty(key, nameof(key));
            Guard.ArgumentNotNullOrEmpty(iv, nameof(iv));

            byte[] encrypted;
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = key;
                rijAlg.IV = iv;

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            return Convert.ToBase64String(encrypted);
        }

        public static string DecryptStringFromBytes(string cipherBase64Str, string KeyBase64Str, string IVBase64Str)
        {
            var cipherText = Convert.FromBase64String(cipherBase64Str);
            var key = Convert.FromBase64String(KeyBase64Str);
            var iv = Convert.FromBase64String(IVBase64Str);

            Guard.ArgumentNotNullOrEmpty(cipherText, nameof(cipherText));
            Guard.ArgumentNotNullOrEmpty(key, nameof(key));
            Guard.ArgumentNotNullOrEmpty(iv, nameof(iv));


            string plaintext = null;

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = key;
                rijAlg.IV = iv;

                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }
    }
}
