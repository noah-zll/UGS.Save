//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;
using System.Text;
using MemoryPack;

namespace UGS.Save
{
    /// <summary>
    /// MemoryPack存档序列化器
    /// </summary>
    public class MemoryPackSaveSerializer : ISaveSerializer
    {
        /// <summary>
        /// 序列化对象为Base64字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="data">要序列化的对象</param>
        /// <returns>序列化后的Base64字符串</returns>
        public string Serialize<T>(T data) where T : class
        {
            byte[] bytes = MemoryPackSerializer.Serialize(data);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 反序列化Base64字符串为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="serializedData">序列化的Base64字符串</param>
        /// <returns>反序列化后的对象</returns>
        public T Deserialize<T>(string serializedData) where T : class
        {
            byte[] bytes = Convert.FromBase64String(serializedData);
            return MemoryPackSerializer.Deserialize<T>(bytes);
        }
    }
}