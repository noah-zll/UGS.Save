//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UGS.Save;

namespace UGS.Save.Editor
{
    /// <summary>
    /// 存档管理器窗口
    /// </summary>
    public class SaveManagerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _customSavePath = "";
        private SaveFormat _selectedFormat = SaveFormat.Json;
        private bool _useEncryption = false;
        private string _encryptionKey = "";
        private string _selectedSaveId = "";
        private string _selectedDataKey;
        private List<string> _saveList = new List<string>();
        private List<string> _dataKeysList = new List<string>();
        private SaveMode _saveMode = SaveMode.SingleFile;
        private bool _showSaveMode = false;
        
        // 当前使用的配置对象
        private SaveManagerConfig _currentConfig;

        [MenuItem("Window/UGS/Save Manager")]
        public static void ShowWindow()
        {
            SaveManagerWindow window = GetWindow<SaveManagerWindow>("存档管理器");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            // 尝试加载配置文件
            _currentConfig = Resources.Load<SaveManagerConfig>("UGS/Save/SaveManagerConfig");
            
            // 如果配置文件不存在，则创建默认配置
            if (_currentConfig == null)
            {
                // 创建配置资源
                _currentConfig = ScriptableObject.CreateInstance<SaveManagerConfig>();
                
                // 确保Resources目录存在
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                
                if (!AssetDatabase.IsValidFolder("Assets/Resources/UGS"))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "UGS");
                }
                
                if (!AssetDatabase.IsValidFolder("Assets/Resources/UGS/Save"))
                {
                    AssetDatabase.CreateFolder("Assets/Resources/UGS", "Save");
                }
                
                // 保存资源
                AssetDatabase.CreateAsset(_currentConfig, "Assets/Resources/UGS/Save/SaveManagerConfig.asset");
                AssetDatabase.SaveAssets();
                
                Debug.Log("[SaveManagerWindow] 已创建默认SaveManager配置文件");
            }
            
            // 应用配置
            _currentConfig.ApplyConfig();
            
            // 从配置对象更新窗口设置
            UpdateWindowSettingsFromConfig();
            
            // 刷新存档列表
            RefreshSaveList();
        }

        private void OnGUI()
        {
            GUILayout.Label("UGS 存档管理器", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawSettingsSection();
            EditorGUILayout.Space();

            // 存档模式设置
            _showSaveMode = EditorGUILayout.Foldout(_showSaveMode, "存档模式设置", true, EditorStyles.foldoutHeader);
            if (_showSaveMode)
            {
                EditorGUI.indentLevel++;
                
                SaveMode newSaveMode = (SaveMode)EditorGUILayout.EnumPopup("存档模式", _saveMode);
                if (newSaveMode != _saveMode)
                {
                    _saveMode = newSaveMode;
                    
                    // 更新SaveManager
                    SaveManager.SetSaveMode(_saveMode);
                    
                    // 直接更新配置对象
                    if (_currentConfig != null)
                    {
                        // 直接设置SaveManagerConfig的saveMode字段
                        System.Type type = typeof(SaveManagerConfig);
                        var saveModeField = type.GetField("saveMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (saveModeField != null)
                        {
                            saveModeField.SetValue(_currentConfig, _saveMode);
                            EditorUtility.SetDirty(_currentConfig);
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        // 如果配置对象不存在，则创建并同步到SaveManagerConfig
                        SyncToSaveManagerConfig();
                    }
                    
                    Debug.Log($"[SaveManager] 已设置存档模式为: {_saveMode}，已同步到配置文件");
                    RefreshSaveList();
                }
                
                EditorGUILayout.HelpBox(_saveMode == SaveMode.SingleFile ? 
                    "单文件模式: 每个存档保存为一个文件" : 
                    "文件夹模式: 每个存档保存在单独的文件夹中，可以包含多个数据文件", 
                    MessageType.Info);
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            DrawSaveListSection();
            EditorGUILayout.Space();

            DrawSelectedSaveSection();
        }

        /// <summary>
        /// 绘制设置部分
        /// </summary>
        private void DrawSettingsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("存档系统设置", EditorStyles.boldLabel);

            // 存档路径设置
            EditorGUILayout.BeginHorizontal();
            string newSavePath = EditorGUILayout.TextField("自定义存档路径", _customSavePath);
            if (newSavePath != _customSavePath)
            {
                _customSavePath = newSavePath;
                // 直接更新SaveManager和配置对象
                if (!string.IsNullOrEmpty(_customSavePath))
                {
                    SaveManager.SetSavePath(_customSavePath);
                    
                    // 直接更新配置对象
                    if (_currentConfig != null)
                    {
                        System.Type type = typeof(SaveManagerConfig);
                        var savePathField = type.GetField("savePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (savePathField != null)
                        {
                            savePathField.SetValue(_currentConfig, _customSavePath);
                            EditorUtility.SetDirty(_currentConfig);
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        // 如果配置对象不存在，则创建并同步到SaveManagerConfig
                        SyncToSaveManagerConfig();
                    }
                    RefreshSaveList();
                }
            }
            
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择存档路径", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path) && path != _customSavePath)
                {
                    _customSavePath = path;
                    // 直接更新SaveManager和配置对象
                    SaveManager.SetSavePath(_customSavePath);
                    
                    // 直接更新配置对象
                    if (_currentConfig != null)
                    {
                        System.Type type = typeof(SaveManagerConfig);
                        var savePathField = type.GetField("savePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (savePathField != null)
                        {
                            savePathField.SetValue(_currentConfig, _customSavePath);
                            EditorUtility.SetDirty(_currentConfig);
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        // 如果配置对象不存在，则创建并同步到SaveManagerConfig
                        SyncToSaveManagerConfig();
                    }
                    RefreshSaveList();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("应用路径设置"))
            {
                if (!string.IsNullOrEmpty(_customSavePath))
                {
                    // 更新SaveManager
                    SaveManager.SetSavePath(_customSavePath);
                    RefreshSaveList();
                    
                    // 直接更新配置对象
                    if (_currentConfig != null)
                    {
                        // 使用反射设置SaveManagerConfig的savePath字段
                        System.Type type = typeof(SaveManagerConfig);
                        var savePathField = type.GetField("savePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (savePathField != null)
                        {
                            savePathField.SetValue(_currentConfig, _customSavePath);
                            EditorUtility.SetDirty(_currentConfig);
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        // 如果配置对象不存在，则创建并同步到SaveManagerConfig
                        SyncToSaveManagerConfig();
                    }
                    
                    EditorUtility.DisplayDialog("成功", $"存档路径已设置为: {_customSavePath}\n已同步到配置文件", "确定");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请输入有效的存档路径", "确定");
                }
            }

            EditorGUILayout.Space();

            // 存档格式设置
            SaveFormat newFormat = (SaveFormat)EditorGUILayout.EnumPopup("存档格式", _selectedFormat);
            if (newFormat != _selectedFormat)
            {
                _selectedFormat = newFormat;
                
                // 更新SaveManager
                SaveManager.SetSaveFormat(_selectedFormat);
                
                // 直接更新配置对象
                if (_currentConfig != null)
                {
                    // 使用反射设置SaveManagerConfig的saveFormat字段
                    System.Type type = typeof(SaveManagerConfig);
                    var saveFormatField = type.GetField("saveFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (saveFormatField != null)
                    {
                        saveFormatField.SetValue(_currentConfig, _selectedFormat);
                        EditorUtility.SetDirty(_currentConfig);
                        AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    // 如果配置对象不存在，则创建并同步到SaveManagerConfig
                    SyncToSaveManagerConfig();
                }
                
                Debug.Log($"[SaveManager] 已设置存档格式为: {_selectedFormat}，已同步到配置文件");
            }
            
            if (GUILayout.Button("应用格式设置"))
            {
                // 更新SaveManager
                SaveManager.SetSaveFormat(_selectedFormat);
                
                // 直接更新配置对象
                if (_currentConfig != null)
                {
                    // 使用反射设置SaveManagerConfig的saveFormat字段
                    System.Type type = typeof(SaveManagerConfig);
                    var saveFormatField = type.GetField("saveFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (saveFormatField != null)
                    {
                        saveFormatField.SetValue(_currentConfig, _selectedFormat);
                        EditorUtility.SetDirty(_currentConfig);
                        AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    // 如果配置对象不存在，则创建并同步到SaveManagerConfig
                    SyncToSaveManagerConfig();
                }
                
                EditorUtility.DisplayDialog("成功", $"存档格式已设置为: {_selectedFormat}\n已同步到配置文件", "确定");
            }

            EditorGUILayout.Space();

            // 加密设置
            bool newUseEncryption = EditorGUILayout.Toggle("启用加密", _useEncryption);
            if (newUseEncryption != _useEncryption)
            {
                _useEncryption = newUseEncryption;
                
                // 更新SaveManager
                SaveManager.EnableEncryption(_useEncryption);
                
                // 直接更新配置对象
                if (_currentConfig != null)
                {
                    // 使用反射设置SaveManagerConfig的useEncryption字段
                    System.Type type = typeof(SaveManagerConfig);
                    var useEncryptionField = type.GetField("useEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (useEncryptionField != null)
                    {
                        useEncryptionField.SetValue(_currentConfig, _useEncryption);
                        EditorUtility.SetDirty(_currentConfig);
                        AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    // 如果配置对象不存在，则创建并同步到SaveManagerConfig
                    SyncToSaveManagerConfig();
                }
                
                Debug.Log($"[SaveManager] 已{(_useEncryption ? "启用" : "禁用")}加密，已同步到配置文件");
            }
            
            if (_useEncryption)
            {
                string newEncryptionKey = EditorGUILayout.PasswordField("加密密钥", _encryptionKey);
                if (newEncryptionKey != _encryptionKey)
                {
                    _encryptionKey = newEncryptionKey;
                    
                    // 更新SaveManager
                    SaveManager.EnableEncryption(_useEncryption, _encryptionKey);
                    
                    // 直接更新配置对象
                    if (_currentConfig != null)
                    {
                        // 使用反射设置SaveManagerConfig的encryptionKey字段
                        System.Type type = typeof(SaveManagerConfig);
                        var encryptionKeyField = type.GetField("encryptionKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (encryptionKeyField != null)
                        {
                            encryptionKeyField.SetValue(_currentConfig, _encryptionKey);
                            EditorUtility.SetDirty(_currentConfig);
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        // 如果配置对象不存在，则创建并同步到SaveManagerConfig
                        SyncToSaveManagerConfig();
                    }
                }
            }

            if (GUILayout.Button("应用加密设置"))
            {
                // 更新SaveManager
                SaveManager.EnableEncryption(_useEncryption, _encryptionKey);
                
                // 直接更新配置对象
                if (_currentConfig != null)
                {
                    // 使用反射设置SaveManagerConfig的加密相关字段
                    System.Type type = typeof(SaveManagerConfig);
                    
                    // 设置useEncryption字段
                    var useEncryptionField = type.GetField("useEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (useEncryptionField != null)
                    {
                        useEncryptionField.SetValue(_currentConfig, _useEncryption);
                    }
                    
                    // 设置encryptionKey字段
                    var encryptionKeyField = type.GetField("encryptionKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (encryptionKeyField != null)
                    {
                        encryptionKeyField.SetValue(_currentConfig, _encryptionKey);
                    }
                    
                    EditorUtility.SetDirty(_currentConfig);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    // 如果配置对象不存在，则创建并同步到SaveManagerConfig
                    SyncToSaveManagerConfig();
                }
                
                EditorUtility.DisplayDialog("成功", $"存档加密已{(_useEncryption ? "启用" : "禁用")}\n已同步到配置文件", "确定");
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("配置文件", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox("配置文件保存在 Resources/UGS/Save/SaveManagerConfig.asset\n可以在游戏启动时通过 SaveManagerConfigLoader 组件自动加载", MessageType.Info);

            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 同步当前配置到SaveManagerConfig
        /// </summary>
        private void SyncToSaveManagerConfig()
        {
            // 如果当前配置对象为空，尝试加载配置资源
            if (_currentConfig == null)
            {
                _currentConfig = Resources.Load<SaveManagerConfig>("UGS/Save/SaveManagerConfig");
                
                // 如果配置不存在，则创建一个
                if (_currentConfig == null)
                {
                    // 创建配置资源
                    _currentConfig = ScriptableObject.CreateInstance<SaveManagerConfig>();
                    
                    // 确保Resources目录存在
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    }
                    
                    if (!AssetDatabase.IsValidFolder("Assets/Resources/UGS"))
                    {
                        AssetDatabase.CreateFolder("Assets/Resources", "UGS");
                    }
                    
                    if (!AssetDatabase.IsValidFolder("Assets/Resources/UGS/Save"))
                    {
                        AssetDatabase.CreateFolder("Assets/Resources/UGS", "Save");
                    }
                    
                    // 保存资源
                    AssetDatabase.CreateAsset(_currentConfig, "Assets/Resources/UGS/Save/SaveManagerConfig.asset");
                }
            }
            
            // 直接设置配置对象的属性
            System.Type type = typeof(SaveManagerConfig);
            
            // 设置savePath字段
            var savePathField = type.GetField("savePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (savePathField != null)
            {
                savePathField.SetValue(_currentConfig, _customSavePath);
            }
            
            // 设置saveMode字段
            var saveModeField = type.GetField("saveMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (saveModeField != null)
            {
                saveModeField.SetValue(_currentConfig, _saveMode);
            }
            
            // 设置saveFormat字段
            var saveFormatField = type.GetField("saveFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (saveFormatField != null)
            {
                saveFormatField.SetValue(_currentConfig, _selectedFormat);
            }
            
            // 设置useEncryption字段
            var useEncryptionField = type.GetField("useEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (useEncryptionField != null)
            {
                useEncryptionField.SetValue(_currentConfig, _useEncryption);
            }
            
            // 设置encryptionKey字段
            var encryptionKeyField = type.GetField("encryptionKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (encryptionKeyField != null)
            {
                encryptionKeyField.SetValue(_currentConfig, _encryptionKey);
            }
            
            // 标记为已修改，以便保存
            EditorUtility.SetDirty(_currentConfig);
            AssetDatabase.SaveAssets();
        }
        
        /// <summary>
        /// 保存当前配置到文件
        /// </summary>
        private void SaveCurrentConfigToFile()
        {
            // 如果当前配置对象为空，尝试加载配置资源
            if (_currentConfig == null)
            {
                _currentConfig = Resources.Load<SaveManagerConfig>("UGS/Save/SaveManagerConfig");
            }
            
            bool isNewConfig = false;
            
            // 如果配置不存在，则创建一个
            if (_currentConfig == null)
            {
                if (EditorUtility.DisplayDialog("配置不存在", "SaveManager配置文件不存在，是否创建一个？", "创建", "取消"))
                {
                    // 创建配置资源
                    _currentConfig = ScriptableObject.CreateInstance<SaveManagerConfig>();
                    isNewConfig = true;
                }
                else
                {
                    return;
                }
            }
            
            // 直接使用_currentConfig对象设置属性
            System.Type type = typeof(SaveManagerConfig);
            
            // 设置savePath字段
            var savePathField = type.GetField("savePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (savePathField != null)
            {
                savePathField.SetValue(_currentConfig, _customSavePath);
            }
            
            // 设置saveMode字段
            var saveModeField = type.GetField("saveMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (saveModeField != null)
            {
                saveModeField.SetValue(_currentConfig, _saveMode);
            }
            
            // 设置saveFormat字段
            var saveFormatField = type.GetField("saveFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (saveFormatField != null)
            {
                saveFormatField.SetValue(_currentConfig, _selectedFormat);
            }
            
            // 设置useEncryption字段
            var useEncryptionField = type.GetField("useEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (useEncryptionField != null)
            {
                useEncryptionField.SetValue(_currentConfig, _useEncryption);
            }
            
            // 设置encryptionKey字段
            var encryptionKeyField = type.GetField("encryptionKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (encryptionKeyField != null)
            {
                encryptionKeyField.SetValue(_currentConfig, _encryptionKey);
            }
            
            // 如果是新创建的配置，需要保存资源
            if (isNewConfig)
            {
                // 确保Resources目录存在
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                
                if (!AssetDatabase.IsValidFolder("Assets/Resources/UGS"))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "UGS");
                }
                
                if (!AssetDatabase.IsValidFolder("Assets/Resources/UGS/Save"))
                {
                    AssetDatabase.CreateFolder("Assets/Resources/UGS", "Save");
                }
                
                // 保存资源
                AssetDatabase.CreateAsset(_currentConfig, "Assets/Resources/UGS/Save/SaveManagerConfig.asset");
            }
            
            // 标记为已修改，以便保存
            EditorUtility.SetDirty(_currentConfig);
            AssetDatabase.SaveAssets();
            
            // 选中并显示资源
            Selection.activeObject = _currentConfig;
            EditorUtility.FocusProjectWindow();
            
            EditorUtility.DisplayDialog("成功", "当前配置已保存到文件\nAssets/Resources/UGS/Save/SaveManagerConfig.asset", "确定");
        }
        
        /// <summary>
        /// 从配置文件加载
        /// </summary>
        private void LoadConfigFromFile()
        {
            // 如果当前配置对象为空，尝试加载配置资源
            if (_currentConfig == null)
            {
                _currentConfig = Resources.Load<SaveManagerConfig>("UGS/Save/SaveManagerConfig");
            }
            
            if (_currentConfig != null)
            {
                // 应用配置
                _currentConfig.ApplyConfig();
                
                // 更新窗口中的设置
                UpdateWindowSettingsFromConfig();
                
                EditorUtility.DisplayDialog("成功", "已从配置文件加载设置", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "未找到配置文件\nAssets/Resources/UGS/Save/SaveManagerConfig.asset", "确定");
            }
        }
        
        /// <summary>
        /// 从配置对象更新窗口中的设置
        /// </summary>
        private void UpdateWindowSettingsFromConfig()
        {
            if (_currentConfig != null)
            {
                // 使用反射获取SaveManagerConfig的私有字段
                System.Type type = typeof(SaveManagerConfig);
                
                // 获取存档路径
                var savePathField = type.GetField("savePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (savePathField != null)
                {
                    _customSavePath = (string)savePathField.GetValue(_currentConfig);
                }
                
                // 获取存档模式
                var saveModeField = type.GetField("saveMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (saveModeField != null)
                {
                    _saveMode = (SaveMode)saveModeField.GetValue(_currentConfig);
                }
                
                // 获取存档格式
                var saveFormatField = type.GetField("saveFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (saveFormatField != null)
                {
                    _selectedFormat = (SaveFormat)saveFormatField.GetValue(_currentConfig);
                }
                
                // 获取加密设置
                var useEncryptionField = type.GetField("useEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (useEncryptionField != null)
                {
                    _useEncryption = (bool)useEncryptionField.GetValue(_currentConfig);
                }
                
                // 获取加密密钥
                var encryptionKeyField = type.GetField("encryptionKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (encryptionKeyField != null)
                {
                    _encryptionKey = (string)encryptionKeyField.GetValue(_currentConfig);
                }
            }
            else
            {
                // 如果配置对象不存在，则使用反射获取SaveManager的私有字段
                System.Type type = typeof(SaveManager);
                
                // 获取存档路径
                var savePathField = type.GetField("_savePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (savePathField != null)
                {
                    _customSavePath = (string)savePathField.GetValue(null);
                }
                
                // 获取存档模式
                var saveModeField = type.GetField("_saveMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (saveModeField != null)
                {
                    _saveMode = (SaveMode)saveModeField.GetValue(null);
                }
                
                // 获取存档格式
                var saveFormatField = type.GetField("_currentFormat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (saveFormatField != null)
                {
                    _selectedFormat = (SaveFormat)saveFormatField.GetValue(null);
                }
                
                // 获取加密设置
                var useEncryptionField = type.GetField("_useEncryption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (useEncryptionField != null)
                {
                    _useEncryption = (bool)useEncryptionField.GetValue(null);
                }
                
                // 获取加密密钥
                var encryptionKeyField = type.GetField("_encryptionKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (encryptionKeyField != null)
                {
                    _encryptionKey = (string)encryptionKeyField.GetValue(null);
                }
            }
            
            // 刷新存档列表
            RefreshSaveList();
        }
        
        /// <summary>
        /// 更新窗口中的设置（兼容旧版本）
        /// </summary>
        private void UpdateWindowSettings()
        {
            UpdateWindowSettingsFromConfig();
        }

        /// <summary>
        /// 绘制存档列表部分
        /// </summary>
        private void DrawSaveListSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("存档列表", EditorStyles.boldLabel);
            if (GUILayout.Button("刷新", GUILayout.Width(60)))
            {
                RefreshSaveList();
            }
            EditorGUILayout.EndHorizontal();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));

            if (_saveList.Count > 0)
            {
                foreach (string saveId in _saveList)
                {
                    EditorGUILayout.BeginHorizontal();

                    SaveFileInfo saveInfo = SaveManager.GetSaveInfo(saveId);
                    string saveInfoText = saveInfo != null
                        ? $"{saveId} - {saveInfo.FormattedLastWriteTime} ({saveInfo.FormattedSize})"
                        : saveId;

                    bool isSelected = _selectedSaveId == saveId;
                    bool newIsSelected = GUILayout.Toggle(isSelected, saveInfoText, "Button");

                    if (newIsSelected && !isSelected)
                    {
                        _selectedSaveId = saveId;
                        
                        if (_saveMode == SaveMode.FolderBased)
                         {
                             // 在文件夹模式下，获取数据键列表
                             RefreshDataKeysList(saveId);
                             _selectedDataKey = null;
                         }
                    }
                    else if (!newIsSelected && isSelected)
                    {
                        _selectedSaveId = "";
                        _dataKeysList.Clear();
                        _selectedDataKey = null;
                    }

                    if (GUILayout.Button("删除", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("确认删除", $"确定要删除存档 '{saveId}' 吗？", "删除", "取消"))
                        {
                            SaveManager.DeleteSave(saveId);
                            RefreshSaveList();
                            if (_selectedSaveId == saveId)
                            {
                                _selectedSaveId = "";
                                _dataKeysList.Clear();
                                _selectedDataKey = null;
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("没有找到存档文件", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();

            // 显示文件夹模式下的数据键列表
            if (_saveMode == SaveMode.FolderBased && !string.IsNullOrEmpty(_selectedSaveId) && _dataKeysList.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"存档 {_selectedSaveId} 的数据键列表", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical("box");
                foreach (string dataKey in _dataKeysList)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    bool isSelected = dataKey == _selectedDataKey;
                    bool newIsSelected = GUILayout.Toggle(isSelected, dataKey, "Button");
                    
                    if (newIsSelected && !isSelected)
                    {
                        _selectedDataKey = dataKey;
                    }
                    else if (!newIsSelected && isSelected)
                    {
                        _selectedDataKey = null;
                    }
                    
                    if (GUILayout.Button("删除", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("确认删除", $"确定要删除数据 {_selectedSaveId}/{dataKey} 吗？", "删除", "取消"))
                        {
                            SaveManager.DeleteSave(_selectedSaveId, dataKey);
                             RefreshDataKeysList(_selectedSaveId);
                             if (_selectedDataKey == dataKey)
                             {
                                 _selectedDataKey = null;
                             }
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("删除所有存档"))
            {
                if (EditorUtility.DisplayDialog("确认删除", "确定要删除所有存档吗？此操作不可撤销！", "删除", "取消"))
                {
                    SaveManager.DeleteAllSaves();
                    RefreshSaveList();
                    _selectedSaveId = "";
                    _dataKeysList.Clear();
                    _selectedDataKey = null;
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制选中存档部分
        /// </summary>
        private void DrawSelectedSaveSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("选中的存档", EditorStyles.boldLabel);

            if (!string.IsNullOrEmpty(_selectedSaveId))
            {
                SaveFileInfo saveInfo = SaveManager.GetSaveInfo(_selectedSaveId);
                if (saveInfo != null)
                {
                    EditorGUILayout.LabelField("存档ID", saveInfo.SaveId);
                    EditorGUILayout.LabelField("创建时间", saveInfo.FormattedCreationTime);
                    EditorGUILayout.LabelField("修改时间", saveInfo.FormattedLastWriteTime);
                    EditorGUILayout.LabelField("文件大小", saveInfo.FormattedSize);

                    EditorGUILayout.Space();

                    if (_saveMode == SaveMode.FolderBased)
                    {
                        if (!string.IsNullOrEmpty(_selectedDataKey))
                        {
                            // 显示特定数据文件
                            if (GUILayout.Button("在文件浏览器中显示数据文件"))
                            {
                                string filePath = SaveManagerExtensions.GetSaveFilePath(_selectedSaveId, _selectedDataKey);
                                EditorUtility.RevealInFinder(filePath);
                            }
                        }
                        else
                        {
                            // 显示存档文件夹
                            if (GUILayout.Button("在文件浏览器中显示存档文件夹"))
                            {
                                string folderPath = SaveManagerExtensions.GetSaveFolderPath(_selectedSaveId);
                                EditorUtility.RevealInFinder(folderPath);
                            }
                        }
                    }
                    else
                    {
                        // 单文件模式
                        if (GUILayout.Button("在文件浏览器中显示"))
                        {
                            string filePath = Path.Combine(SaveManagerExtensions.GetSavePath(), _selectedSaveId + (_selectedFormat == SaveFormat.Json ? ".json" : ".sav"));
                            EditorUtility.RevealInFinder(filePath);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("无法获取存档信息", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("未选择存档", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 刷新存档列表
        /// </summary>
        private void RefreshSaveList()
        {
            _saveList = SaveManager.GetSaveList();
            _selectedSaveId = null;
            _selectedDataKey = null;
            _dataKeysList.Clear();
        }
        
        private void RefreshDataKeysList(string saveId)
        {
            if (string.IsNullOrEmpty(saveId) || _saveMode != SaveMode.FolderBased)
            {
                _dataKeysList.Clear();
                return;
            }
            
            _dataKeysList = SaveManager.GetSaveDataKeys(saveId);
        }
    }
}