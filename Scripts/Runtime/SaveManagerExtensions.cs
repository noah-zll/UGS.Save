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
        
        /// <summary>
        /// 获取存档文件夹路径
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <returns>存档文件夹路径</returns>
        public static string GetSaveFolderPath(string saveId)
        {
            return System.IO.Path.Combine(GetSavePath(), saveId);
        }
        
        /// <summary>
        /// 获取存档文件路径
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="dataKey">数据键（仅在文件夹模式下使用）</param>
        /// <returns>存档文件路径</returns>
        public static string GetSaveFilePath(string saveId, string dataKey = null)
        {
            // 使用反射获取私有字段_saveMode和_serializer
            Type type = typeof(SaveManager);
            FieldInfo saveModeField = type.GetField("_saveMode", BindingFlags.NonPublic | BindingFlags.Static);
            FieldInfo serializerField = type.GetField("_serializer", BindingFlags.NonPublic | BindingFlags.Static);
            
            object saveMode = saveModeField?.GetValue(null);
            object serializer = serializerField?.GetValue(null);
            
            bool isFolderBased = saveMode != null && saveMode.ToString() == "FolderBased";
            bool isJsonSerializer = serializer != null && serializer.GetType().Name.Contains("JsonSaveSerializer");
            
            string extension = isJsonSerializer ? ".json" : ".sav";
            
            if (isFolderBased && !string.IsNullOrEmpty(dataKey))
            {
                string saveFolderPath = GetSaveFolderPath(saveId);
                return System.IO.Path.Combine(saveFolderPath, dataKey + extension);
            }
            else
            {
                return System.IO.Path.Combine(GetSavePath(), saveId + extension);
            }
        }
    }
}