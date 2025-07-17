//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

namespace UGS.Save
{
    /// <summary>
    /// 存档序列化器接口
    /// </summary>
    public interface ISaveSerializer
    {
        /// <summary>
        /// 序列化对象为字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="data">要序列化的对象</param>
        /// <returns>序列化后的字符串</returns>
        string Serialize<T>(T data) where T : class;

        /// <summary>
        /// 反序列化字符串为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="serializedData">序列化的字符串</param>
        /// <returns>反序列化后的对象</returns>
        T Deserialize<T>(string serializedData) where T : class;
    }
}