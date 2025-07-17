//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace UGS.Save
{
    /// <summary>
    /// 加密工具类
    /// </summary>
    public static class EncryptionUtility
    {
        // 默认盐值
        private static readonly byte[] DefaultSalt = Encoding.ASCII.GetBytes("UGS_SAVE_SYSTEM_SALT");

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="password">密码</param>
        /// <returns>加密后的字符串</returns>
        public static string Encrypt(string plainText, string password)
        {
            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] keyBytes = GenerateKey(password);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.GenerateIV();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        // 写入IV
                        ms.Write(aes.IV, 0, aes.IV.Length);

                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.FlushFinalBlock();
                        }

                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EncryptionUtility] 加密失败: {ex.Message}");
                return plainText; // 加密失败时返回原文
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="encryptedText">密文</param>
        /// <param name="password">密码</param>
        /// <returns>解密后的字符串</returns>
        public static string Decrypt(string encryptedText, string password)
        {
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] keyBytes = GenerateKey(password);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;

                    using (MemoryStream ms = new MemoryStream(encryptedBytes))
                    {
                        // 读取IV
                        byte[] iv = new byte[aes.IV.Length];
                        ms.Read(iv, 0, iv.Length);
                        aes.IV = iv;

                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cs, Encoding.UTF8))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EncryptionUtility] 解密失败: {ex.Message}");
                return encryptedText; // 解密失败时返回原文
            }
        }

        /// <summary>
        /// 生成密钥
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns>生成的密钥</returns>
        private static byte[] GenerateKey(string password)
        {
            using (Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(password, DefaultSalt, 1000))
            {
                return rfc2898.GetBytes(32); // 256位密钥
            }
        }
    }
}