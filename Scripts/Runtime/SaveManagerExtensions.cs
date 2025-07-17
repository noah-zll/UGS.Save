//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;
using System.Reflection;

namespace UGS.Save
{
    /// <summary>
    /// SaveManager扩展方法
    /// </summary>
    public static class SaveManagerExtensions
    {
        /// <summary>
        /// 获取当前存档路径
        /// </summary>
        /// <returns>当前存档路径</returns>
        public static string GetSavePath()
        {
            // 使用反射获取私有字段_savePath
            Type type = typeof(SaveManager);
            FieldInfo fieldInfo = type.GetField("_savePath", BindingFlags.NonPublic | BindingFlags.Static);
            
            if (fieldInfo != null)
            {
                return (string)fieldInfo.GetValue(null);
            }
            
            return string.Empty;
        }
    }
}