//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace UGS.Save.Examples
{
    /// <summary>
    /// SaveManager配置示例
    /// </summary>
    public class SaveManagerConfigExample : MonoBehaviour
    {
        [Header("配置加载器")]
        [SerializeField] private SaveManagerConfigLoader configLoader;
        
        [Header("UI组件")]
        [SerializeField] private Text configInfoText;
        [SerializeField] private Button reloadConfigButton;
        
        private void Start()
        {
            // 初始化UI
            if (reloadConfigButton != null)
            {
                reloadConfigButton.onClick.AddListener(ReloadConfig);
            }
            
            // 显示当前配置
            UpdateConfigInfoText();
        }
        
        /// <summary>
        /// 重新加载配置
        /// </summary>
        public void ReloadConfig()
        {
            if (configLoader != null)
            {
                configLoader.ReloadConfig();
                UpdateConfigInfoText();
            }
        }
        
        /// <summary>
        /// 更新配置信息文本
        /// </summary>
        private void UpdateConfigInfoText()
        {
            if (configInfoText == null)
                return;
            
            // 使用反射获取SaveManager的私有字段
            System.Type type = typeof(SaveManager);
            
            string savePath = "未知";
            string saveMode = "未知";
            string saveFormat = "未知";
            string useEncryption = "未知";
            
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
                saveMode = saveModeField.GetValue(null).ToString();
            }
            
            // 获取存档格式
            var saveFormatField = type.GetField("_currentFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (saveFormatField != null)
            {
                saveFormat = saveFormatField.GetValue(null).ToString();
            }
            
            // 获取加密设置
            var useEncryptionField = type.GetField("_useEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (useEncryptionField != null)
            {
                useEncryption = ((bool)useEncryptionField.GetValue(null)) ? "已启用" : "已禁用";
            }
            
            // 更新文本
            configInfoText.text = $"SaveManager当前配置:\n\n" +
                                 $"存档路径: {savePath}\n" +
                                 $"存档模式: {saveMode}\n" +
                                 $"存档格式: {saveFormat}\n" +
                                 $"加密: {useEncryption}";
        }
    }
}