//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using UnityEngine;

namespace UGS.Save
{
    /// <summary>
    /// SaveManager配置加载器，用于在游戏启动时自动加载SaveManagerConfig配置
    /// </summary>
    [DefaultExecutionOrder(-100)] // 确保在其他脚本之前执行
    public class SaveManagerConfigLoader : MonoBehaviour
    {
        [Tooltip("是否在Awake时自动加载配置")]
        [SerializeField] private bool loadOnAwake = true;
        
        [Tooltip("配置资源路径，默认为UGS/Save/SaveManagerConfig")]
        [SerializeField] private string configResourcePath = "UGS/Save/SaveManagerConfig";
        
        private static bool _initialized = false;
        
        private void Awake()
        {
            if (loadOnAwake && !_initialized)
            {
                LoadConfig();
            }
        }
        
        /// <summary>
        /// 加载SaveManager配置
        /// </summary>
        /// <returns>是否成功加载配置</returns>
        public bool LoadConfig()
        {
            // 防止重复初始化
            if (_initialized)
            {
                Debug.LogWarning("[SaveManagerConfigLoader] SaveManager配置已经加载过，不会重复加载");
                return false;
            }
            
            // 尝试加载配置资源
            SaveManagerConfig config = Resources.Load<SaveManagerConfig>(configResourcePath);
            
            if (config != null)
            {
                // 应用配置
                config.ApplyConfig();
                _initialized = true;
                Debug.Log($"[SaveManagerConfigLoader] 已成功加载SaveManager配置: {configResourcePath}");
                return true;
            }
            else
            {
                Debug.LogWarning($"[SaveManagerConfigLoader] 未找到SaveManager配置资源: {configResourcePath}，将使用默认配置");
                // 使用默认配置初始化SaveManager
                SaveManager.Initialize();
                _initialized = true;
                return false;
            }
        }
        
        /// <summary>
        /// 重新加载SaveManager配置
        /// </summary>
        /// <returns>是否成功重新加载配置</returns>
        public bool ReloadConfig()
        {
            _initialized = false;
            return LoadConfig();
        }
    }
}