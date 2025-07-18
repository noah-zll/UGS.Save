//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System.IO;
using UnityEngine;

namespace UGS.Save
{
    /// <summary>
    /// SaveManager配置，用于保存和加载SaveManager的配置
    /// </summary>
    [CreateAssetMenu(fileName = "SaveManagerConfig", menuName = "UGS/Save/SaveManagerConfig")]
    public class SaveManagerConfig : ScriptableObject
    {
        [Header("基本设置")]
        [Tooltip("存档路径，为空则使用默认路径")]
        [SerializeField] private string savePath = "";
        
        [Tooltip("存档模式")]
        [SerializeField] private SaveMode saveMode = SaveMode.SingleFile;
        
        [Tooltip("存档格式")]
        [SerializeField] private SaveFormat saveFormat = SaveFormat.Json;
        
        [Header("加密设置")]
        [Tooltip("是否启用加密")]
        [SerializeField] private bool useEncryption = false;
        
        [Tooltip("加密密钥，为空则使用默认密钥")]
        [SerializeField] private string encryptionKey = "";
        
        /// <summary>
        /// 应用配置到SaveManager
        /// </summary>
        public void ApplyConfig()
        {
            // 初始化SaveManager
            if (!string.IsNullOrEmpty(savePath))
            {
                SaveManager.Initialize(savePath);
            }
            else
            {
                SaveManager.Initialize();
            }
            
            // 设置存档模式
            SaveManager.SetSaveMode(saveMode);
            
            // 设置存档格式
            SaveManager.SetSaveFormat(saveFormat);
            
            // 设置加密
            SaveManager.EnableEncryption(useEncryption, encryptionKey);
            
            Debug.Log("[SaveManagerConfig] 配置已应用到SaveManager");
        }
        
        /// <summary>
        /// 从SaveManager获取当前配置
        /// </summary>
        public void GetConfigFromSaveManager()
        {
            // 使用反射获取SaveManager的私有字段
            System.Type type = typeof(SaveManager);
            
            // 获取存档路径
            var savePathField = type.GetField("_savePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (savePathField != null)
            {
                savePath = (string)savePathField.GetValue(null);
            }
            
            // 获取存档模式
            var saveModeField = type.GetField("_saveMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (saveModeField != null)
            {
                saveMode = (SaveMode)saveModeField.GetValue(null);
            }
            
            // 获取存档格式
            var saveFormatField = type.GetField("_currentFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (saveFormatField != null)
            {
                saveFormat = (SaveFormat)saveFormatField.GetValue(null);
            }
            
            // 获取加密设置
            var useEncryptionField = type.GetField("_useEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (useEncryptionField != null)
            {
                useEncryption = (bool)useEncryptionField.GetValue(null);
            }
            
            // 获取加密密钥
            var encryptionKeyField = type.GetField("_encryptionKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (encryptionKeyField != null)
            {
                encryptionKey = (string)encryptionKeyField.GetValue(null);
            }
            
            Debug.Log("[SaveManagerConfig] 已从SaveManager获取当前配置");
        }
    }
}