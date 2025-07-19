//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;

namespace UGS.Save
{
    /// <summary>
    /// 存档模式
    /// </summary>
    [Serializable]
    public enum SaveMode
    {
        /// <summary>
        /// 单文件模式 - 每个存档保存为单个文件
        /// </summary>
        SingleFile,
        
        /// <summary>
        /// 文件夹模式 - 每个存档保存在单独的文件夹中，可包含多个文件
        /// </summary>
        FolderBased
    }
}