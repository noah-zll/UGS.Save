//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using UnityEngine;

namespace UGS.Save
{
    /// <summary>
    /// JSON存档序列化器
    /// </summary>
    public class JsonSaveSerializer : ISaveSerializer
    {
        /// <summary>
        /// 序列化对象为JSON字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="data">要序列化的对象</param>
        /// <returns>序列化后的JSON字符串</returns>
        public string Serialize<T>(T data) where T : class
        {
            return JsonUtility.ToJson(data, true);
        }

        /// <summary>
        /// 反序列化JSON字符串为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="serializedData">序列化的JSON字符串</param>
        /// <returns>反序列化后的对象</returns>
        public T Deserialize<T>(string serializedData) where T : class
        {
            return JsonUtility.FromJson<T>(serializedData);
        }
    }
}