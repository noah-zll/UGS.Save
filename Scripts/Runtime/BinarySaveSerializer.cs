//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace UGS.Save
{
    /// <summary>
    /// 二进制存档序列化器
    /// </summary>
    public class BinarySaveSerializer : ISaveSerializer
    {
        /// <summary>
        /// 序列化对象为二进制字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="data">要序列化的对象</param>
        /// <returns>序列化后的Base64编码字符串</returns>
        public string Serialize<T>(T data) where T : class
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, data);
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BinarySaveSerializer] 序列化失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 反序列化二进制字符串为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="serializedData">序列化的Base64编码字符串</param>
        /// <returns>反序列化后的对象</returns>
        public T Deserialize<T>(string serializedData) where T : class
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(serializedData);
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream) as T;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BinarySaveSerializer] 反序列化失败: {ex.Message}");
                throw;
            }
        }
    }
}