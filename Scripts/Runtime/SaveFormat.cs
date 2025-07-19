//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;

namespace UGS.Save
{
    /// <summary>
    /// 存档格式枚举
    /// </summary>
    [Serializable]
    public enum SaveFormat
    {
        /// <summary>
        /// JSON格式
        /// </summary>
        Json,

        /// <summary>
        /// 二进制格式
        /// </summary>
        Binary,
        
        /// <summary>
        /// Protobuf格式
        /// </summary>
        Protobuf
    }
}