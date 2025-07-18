//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UGS.Save
{
    /// <summary>
    /// 存档管理器，提供存档系统的主要功能接口
    /// </summary>
    public static class SaveManager
    {
        private static ISaveSerializer _serializer = new JsonSaveSerializer();
        private static string _savePath = "";
        private static bool _isInitialized = false;
        private static bool _useEncryption = false;
        private static string _encryptionKey = "UGS_DEFAULT_KEY";
        private static SaveMode _saveMode = SaveMode.SingleFile;
        private static SaveFormat _currentFormat = SaveFormat.Json;
        private static Dictionary<string, Dictionary<string, object>> _folderBasedSaveCache = new Dictionary<string, Dictionary<string, object>>();
        private static Dictionary<string, SaveMetadata> _metadataCache = new Dictionary<string, SaveMetadata>();
        private const string METADATA_KEY = "_metadata";

        /// <summary>
        /// 初始化存档系统
        /// </summary>
        /// <param name="customPath">自定义存档路径，为空则使用默认路径</param>
        public static void Initialize(string customPath = "")
        {
            if (_isInitialized)
                return;

            if (string.IsNullOrEmpty(customPath))
            {
                _savePath = Path.Combine(Application.persistentDataPath, "Saves");
            }
            else
            {
                _savePath = customPath;
            }

            // 确保存档目录存在
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            _isInitialized = true;
            Debug.Log($"[SaveManager] 初始化完成，存档路径: {_savePath}");
        }

        /// <summary>
        /// 设置存档路径
        /// </summary>
        /// <param name="path">存档路径</param>
        public static void SetSavePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("存档路径不能为空");

            _savePath = path;

            // 确保存档目录存在
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            Debug.Log($"[SaveManager] 存档路径已更改为: {_savePath}");
        }

        /// <summary>
        /// 设置存档格式
        /// </summary>
        /// <param name="format">存档格式</param>
        public static void SetSaveFormat(SaveFormat format)
        {
            switch (format)
            {
                case SaveFormat.Json:
                    _serializer = new JsonSaveSerializer();
                    break;
                case SaveFormat.Binary:
                    _serializer = new BinarySaveSerializer();
                    break;
                case SaveFormat.Protobuf:
                    _serializer = new ProtobufSaveSerializer();
                    break;
                default:
                    throw new ArgumentException("不支持的存档格式");
            }

            _currentFormat = format;
            Debug.Log($"[SaveManager] 存档格式已设置为: {format}");
        }

        /// <summary>
        /// 启用或禁用存档加密
        /// </summary>
        /// <param name="enable">是否启用加密</param>
        /// <param name="encryptionKey">加密密钥，为空则使用默认密钥</param>
        public static void EnableEncryption(bool enable, string encryptionKey = "")
        {
            _useEncryption = enable;

            if (!string.IsNullOrEmpty(encryptionKey))
            {
                _encryptionKey = encryptionKey;
            }

            Debug.Log($"[SaveManager] 存档加密已{(enable ? "启用" : "禁用")}");
        }
        
        /// <summary>
        /// 设置存档模式
        /// </summary>
        /// <param name="mode">存档模式</param>
        public static void SetSaveMode(SaveMode mode)
        {
            _saveMode = mode;
            Debug.Log($"[SaveManager] 存档模式已设置为: {mode}");
        }

        /// <summary>
        /// 保存游戏数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="saveId">存档ID</param>
        /// <param name="data">要保存的数据</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveGame<T>(string saveId, T data) where T : class
        {
            return SaveGame(saveId, data, "main", "", "");
        }
        
        /// <summary>
        /// 保存游戏数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="saveId">存档ID</param>
        /// <param name="data">要保存的数据</param>
        /// <param name="saveName">存档名称</param>
        /// <param name="description">存档描述</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveGame<T>(string saveId, T data, string saveName, string description) where T : class
        {
            return SaveGame(saveId, data, "main", saveName, description);
        }
        
        /// <summary>
        /// 保存游戏数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="saveId">存档ID</param>
        /// <param name="data">要保存的数据</param>
        /// <param name="dataKey">数据键（仅在文件夹模式下使用，默认为"main"）</param>
        /// <param name="saveName">存档名称（为空则使用saveId）</param>
        /// <param name="description">存档描述</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveGame<T>(string saveId, T data, string dataKey = "main", string saveName = "", string description = "") where T : class
        {
            CheckInitialization();

            try
            {
                // 创建或更新元数据
                SaveMetadata metadata = GetOrCreateMetadata(saveId, saveName, description);
                metadata.LastWriteTime = DateTime.Now;
                metadata.SaveMode = _saveMode;
                metadata.SaveFormat = _currentFormat;
                
                // 保存元数据
                SaveMetadata(saveId, metadata);
                
                // 保存游戏数据
                string filePath = GetSaveFilePath(saveId, dataKey);
                string serializedData = _serializer.Serialize(data);

                if (_useEncryption)
                {
                    serializedData = EncryptionUtility.Encrypt(serializedData, _encryptionKey);
                }
                
                try
                {
                    // 确保目录存在
                    string directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(filePath, serializedData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveManager] 保存游戏数据失败: {ex.Message}\n路径: {filePath}");
                    return false;
                }
                
                // 如果是文件夹模式，更新缓存
                if (_saveMode == SaveMode.FolderBased)
                {
                    if (!_folderBasedSaveCache.ContainsKey(saveId))
                    {
                        _folderBasedSaveCache[saveId] = new Dictionary<string, object>();
                    }
                    _folderBasedSaveCache[saveId][dataKey] = data;
                }
                
                Debug.Log($"[SaveManager] 存档保存成功: {saveId}{(_saveMode == SaveMode.FolderBased ? "/" + dataKey : "")}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 保存存档失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取或创建存档元数据
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="saveName">存档名称（为空则使用saveId）</param>
        /// <param name="description">存档描述</param>
        /// <returns>存档元数据</returns>
        private static SaveMetadata GetOrCreateMetadata(string saveId, string saveName = "", string description = "")
        {
            // 先从缓存中查找
            if (_metadataCache.ContainsKey(saveId))
            {
                SaveMetadata metadata = _metadataCache[saveId];
                
                // 如果提供了新的名称或描述，则更新
                if (!string.IsNullOrEmpty(saveName))
                {
                    metadata.Name = saveName;
                }
                
                if (!string.IsNullOrEmpty(description))
                {
                    metadata.Description = description;
                }
                
                return metadata;
            }
            
            // 尝试从文件加载
            SaveMetadata loadedMetadata = LoadMetadata(saveId);
            if (loadedMetadata != null)
            {
                // 如果提供了新的名称或描述，则更新
                if (!string.IsNullOrEmpty(saveName))
                {
                    loadedMetadata.Name = saveName;
                }
                
                if (!string.IsNullOrEmpty(description))
                {
                    loadedMetadata.Description = description;
                }
                
                _metadataCache[saveId] = loadedMetadata;
                return loadedMetadata;
            }
            
            // 创建新的元数据
            SaveMetadata newMetadata = new SaveMetadata(saveId, saveName, description);
            _metadataCache[saveId] = newMetadata;
            return newMetadata;
        }
        
        /// <summary>
        /// 保存存档元数据
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="metadata">元数据</param>
        /// <returns>是否保存成功</returns>
        private static bool SaveMetadata(string saveId, SaveMetadata metadata)
        {
            try
            {
                // 更新缓存
                _metadataCache[saveId] = metadata;
                
                // 序列化元数据
                ISaveSerializer jsonSerializer = new JsonSaveSerializer(); // 元数据始终使用JSON格式
                string serializedData = jsonSerializer.Serialize(metadata);
                
                if (_useEncryption)
                {
                    serializedData = EncryptionUtility.Encrypt(serializedData, _encryptionKey);
                }
                
                // 保存元数据文件
                string metadataPath;
                if (_saveMode == SaveMode.FolderBased)
                {
                    string saveFolderPath = GetSaveFolderPath(saveId);
                    // 确保存档文件夹存在
                    if (!Directory.Exists(saveFolderPath))
                    {
                        Directory.CreateDirectory(saveFolderPath);
                    }
                    metadataPath = Path.Combine(saveFolderPath, METADATA_KEY + ".json");
                }
                else
                {
                    // 在单文件模式下，元数据文件与存档文件放在同一目录，但使用不同的文件名
                    // 确保存档目录存在
                    if (!Directory.Exists(_savePath))
                    {
                        Directory.CreateDirectory(_savePath);
                    }
                    metadataPath = Path.Combine(_savePath, saveId + "_" + METADATA_KEY + ".json");
                }
                
                try
                {
                    // 确保元数据文件的目录存在
                    string metadataDirectory = Path.GetDirectoryName(metadataPath);
                    if (!string.IsNullOrEmpty(metadataDirectory) && !Directory.Exists(metadataDirectory))
                    {
                        Directory.CreateDirectory(metadataDirectory);
                    }
                    
                    File.WriteAllText(metadataPath, serializedData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveManager] 保存元数据失败: {ex.Message}\n路径: {metadataPath}");
                    throw; // 重新抛出异常以便上层处理
                }
                Debug.Log($"[SaveManager] 元数据保存成功: {saveId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 保存元数据失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 加载存档元数据
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <returns>元数据，加载失败返回null</returns>
        public static SaveMetadata LoadMetadata(string saveId)
        {
            try
            {
                // 先从缓存中查找
                if (_metadataCache.ContainsKey(saveId))
                {
                    return _metadataCache[saveId];
                }
                
                // 确定元数据文件路径
                string metadataPath;
                if (_saveMode == SaveMode.FolderBased)
                {
                    string saveFolderPath = GetSaveFolderPath(saveId);
                    metadataPath = Path.Combine(saveFolderPath, METADATA_KEY + ".json");
                }
                else
                {
                    metadataPath = Path.Combine(_savePath, saveId + "_" + METADATA_KEY + ".json");
                }
                
                // 检查文件是否存在
                if (!File.Exists(metadataPath))
                {
                    return null;
                }
                
                // 读取并反序列化
                string serializedData = File.ReadAllText(metadataPath);
                
                if (_useEncryption)
                {
                    serializedData = EncryptionUtility.Decrypt(serializedData, _encryptionKey);
                }
                
                ISaveSerializer jsonSerializer = new JsonSaveSerializer(); // 元数据始终使用JSON格式
                SaveMetadata metadata = jsonSerializer.Deserialize<SaveMetadata>(serializedData);
                
                // 更新缓存
                _metadataCache[saveId] = metadata;
                
                return metadata;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 加载元数据失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 加载游戏数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="saveId">存档ID</param>
        /// <param name="dataKey">数据键（仅在文件夹模式下使用，默认为"main"）</param>
        /// <returns>加载的数据，加载失败返回null</returns>
        public static T LoadGame<T>(string saveId, string dataKey = "main") where T : class
        {
            CheckInitialization();

            try
            {
                // 加载元数据以确定存档模式和格式
                SaveMetadata metadata = LoadMetadata(saveId);
                SaveMode originalMode = _saveMode;
                SaveFormat originalFormat = _currentFormat;
                
                // 如果找到元数据，根据元数据设置模式和格式
                if (metadata != null)
                {
                    SetSaveMode(metadata.SaveMode);
                    SetSaveFormat(metadata.SaveFormat);
                }
                
                // 如果是文件夹模式且缓存中有数据，直接返回缓存
                if (_saveMode == SaveMode.FolderBased && _folderBasedSaveCache.ContainsKey(saveId) && 
                    _folderBasedSaveCache[saveId].ContainsKey(dataKey) && 
                    _folderBasedSaveCache[saveId][dataKey] is T)
                {
                    Debug.Log($"[SaveManager] 从缓存加载存档成功: {saveId}/{dataKey}");
                    return (T)_folderBasedSaveCache[saveId][dataKey];
                }
                
                string filePath = GetSaveFilePath(saveId, dataKey);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveManager] 存档不存在: {saveId}{(_saveMode == SaveMode.FolderBased ? "/" + dataKey : "")}");
                    
                    // 恢复原始模式和格式
                    if (metadata != null)
                    {
                        SetSaveMode(originalMode);
                        SetSaveFormat(originalFormat);
                    }
                    
                    return null;
                }

                string serializedData = File.ReadAllText(filePath);

                if (_useEncryption)
                {
                    serializedData = EncryptionUtility.Decrypt(serializedData, _encryptionKey);
                }

                T data = _serializer.Deserialize<T>(serializedData);
                
                // 如果是文件夹模式，更新缓存
                if (_saveMode == SaveMode.FolderBased)
                {
                    if (!_folderBasedSaveCache.ContainsKey(saveId))
                    {
                        _folderBasedSaveCache[saveId] = new Dictionary<string, object>();
                    }
                    _folderBasedSaveCache[saveId][dataKey] = data;
                }
                
                Debug.Log($"[SaveManager] 存档加载成功: {saveId}{(_saveMode == SaveMode.FolderBased ? "/" + dataKey : "")}");
                
                // 恢复原始模式和格式
                if (metadata != null)
                {
                    SetSaveMode(originalMode);
                    SetSaveFormat(originalFormat);
                }
                
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 加载存档失败: {ex.Message}");
                
                // 恢复原始模式和格式
                SaveMetadata metadata = LoadMetadata(saveId);
                if (metadata != null)
                {
                    SaveMode originalMode = _saveMode;
                    SaveFormat originalFormat = _currentFormat;
                    SetSaveMode(originalMode);
                    SetSaveFormat(originalFormat);
                }
                
                return null;
            }
        }

        /// <summary>
        /// 获取存档列表
        /// </summary>
        /// <returns>存档ID列表</returns>
        public static List<string> GetSaveList()
        {
            CheckInitialization();

            try
            {
                List<string> saveList = new List<string>();
                
                if (_saveMode == SaveMode.FolderBased)
                {
                    // 在文件夹模式下，每个子文件夹代表一个存档
                    string[] directories = Directory.GetDirectories(_savePath);
                    foreach (string directory in directories)
                    {
                        string dirName = Path.GetFileName(directory);
                        saveList.Add(dirName);
                    }
                }
                else
                {
                    // 在单文件模式下，每个文件代表一个存档
                    string[] files = Directory.GetFiles(_savePath);
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        saveList.Add(fileName);
                    }
                }

                return saveList;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 获取存档列表失败: {ex.Message}");
                return new List<string>();
            }
        }
        
        /// <summary>
        /// 获取存档中的数据键列表（仅在文件夹模式下有效）
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <returns>数据键列表</returns>
        public static List<string> GetSaveDataKeys(string saveId)
        {
            CheckInitialization();
            
            if (_saveMode != SaveMode.FolderBased)
            {
                Debug.LogWarning("[SaveManager] GetSaveDataKeys方法仅在文件夹模式下有效");
                return new List<string>();
            }
            
            try
            {
                List<string> dataKeys = new List<string>();
                string saveFolderPath = GetSaveFolderPath(saveId);
                
                if (!Directory.Exists(saveFolderPath))
                {
                    Debug.LogWarning($"[SaveManager] 存档不存在: {saveId}");
                    return dataKeys;
                }
                
                string[] files = Directory.GetFiles(saveFolderPath);
                string extension = _serializer is JsonSaveSerializer ? ".json" : ".sav";
                
                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    dataKeys.Add(fileName);
                }
                
                return dataKeys;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 获取存档数据键列表失败: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// 检查存档是否存在
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="dataKey">数据键（仅在文件夹模式下使用，默认为"main"）</param>
        /// <returns>是否存在</returns>
        public static bool SaveExists(string saveId, string dataKey = "main")
        {
            CheckInitialization();
            
            if (_saveMode == SaveMode.FolderBased)
            {
                string saveFolderPath = GetSaveFolderPath(saveId);
                if (!Directory.Exists(saveFolderPath))
                {
                    return false;
                }
                
                if (dataKey == null)
                {
                    // 只检查存档文件夹是否存在
                    return true;
                }
                else
                {
                    // 检查特定数据文件是否存在
                    string filePath = GetSaveFilePath(saveId, dataKey);
                    return File.Exists(filePath);
                }
            }
            else
            {
                string filePath = GetSaveFilePath(saveId);
                return File.Exists(filePath);
            }
        }

        /// <summary>
        /// 删除存档
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="dataKey">数据键（仅在文件夹模式下使用，为null时删除整个存档文件夹）</param>
        /// <returns>是否删除成功</returns>
        public static bool DeleteSave(string saveId, string dataKey = null)
        {
            CheckInitialization();

            try
            {
                if (_saveMode == SaveMode.FolderBased)
                {
                    string saveFolderPath = GetSaveFolderPath(saveId);
                    
                    // 检查存档文件夹是否存在
                    if (!Directory.Exists(saveFolderPath))
                    {
                        Debug.LogWarning($"[SaveManager] 存档不存在: {saveId}");
                        return false;
                    }
                    
                    // 如果dataKey为null，删除整个存档文件夹
                    if (dataKey == null)
                    {
                        Directory.Delete(saveFolderPath, true);
                        
                        // 清除缓存
                        if (_folderBasedSaveCache.ContainsKey(saveId))
                        {
                            _folderBasedSaveCache.Remove(saveId);
                        }
                        
                        // 清除元数据缓存
                        if (_metadataCache.ContainsKey(saveId))
                        {
                            _metadataCache.Remove(saveId);
                        }
                        
                        Debug.Log($"[SaveManager] 删除存档文件夹成功: {saveId}");
                        return true;
                    }
                    else
                    {
                        // 删除特定数据文件
                        string filePath = GetSaveFilePath(saveId, dataKey);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            
                            // 更新缓存
                            if (_folderBasedSaveCache.ContainsKey(saveId) && 
                                _folderBasedSaveCache[saveId].ContainsKey(dataKey))
                            {
                                _folderBasedSaveCache[saveId].Remove(dataKey);
                            }
                            
                            Debug.Log($"[SaveManager] 删除存档数据成功: {saveId}/{dataKey}");
                            return true;
                        }
                        else
                        {
                            Debug.LogWarning($"[SaveManager] 存档数据不存在: {saveId}/{dataKey}");
                            return false;
                        }
                    }
                }
                else
                {
                    // 单文件模式
                    string filePath = GetSaveFilePath(saveId);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        
                        // 删除元数据文件
                        string metadataPath = Path.Combine(_savePath, saveId + "_" + METADATA_KEY + ".json");
                        if (File.Exists(metadataPath))
                        {
                            File.Delete(metadataPath);
                        }
                        
                        // 清除元数据缓存
                        if (_metadataCache.ContainsKey(saveId))
                        {
                            _metadataCache.Remove(saveId);
                        }
                        
                        Debug.Log($"[SaveManager] 删除存档成功: {saveId}");
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"[SaveManager] 存档不存在: {saveId}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 删除存档失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除所有存档
        /// </summary>
        /// <returns>是否删除成功</returns>
        public static bool DeleteAllSaves()
        {
            CheckInitialization();

            try
            {
                if (_saveMode == SaveMode.FolderBased)
                {
                    // 在文件夹模式下，删除所有子文件夹
                    string[] directories = Directory.GetDirectories(_savePath);
                    foreach (string directory in directories)
                    {
                        Directory.Delete(directory, true);
                    }
                    
                    // 清除缓存
                    _folderBasedSaveCache.Clear();
                    _metadataCache.Clear();
                    
                    Debug.Log($"[SaveManager] 所有存档删除成功，共{directories.Length}个");
                }
                else
                {
                    // 在单文件模式下，删除所有文件
                    string[] files = Directory.GetFiles(_savePath);
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                    
                    // 清除元数据缓存
                    _metadataCache.Clear();
                    
                    Debug.Log($"[SaveManager] 所有存档删除成功，共{files.Length}个");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 删除所有存档失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取存档文件信息
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="dataKey">数据键（仅在文件夹模式下使用，为null时获取整个存档文件夹信息）</param>
        /// <returns>存档文件信息</returns>
        public static SaveFileInfo GetSaveInfo(string saveId, string dataKey = null)
        {
            CheckInitialization();

            try
            {
                // 尝试加载元数据
                SaveMetadata metadata = LoadMetadata(saveId);
                
                if (_saveMode == SaveMode.FolderBased)
                {
                    string saveFolderPath = GetSaveFolderPath(saveId);
                    if (!Directory.Exists(saveFolderPath))
                    {
                        Debug.LogWarning($"[SaveManager] 存档不存在: {saveId}");
                        return null;
                    }
                    
                    // 如果dataKey为null，获取整个存档文件夹信息
                    if (dataKey == null)
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(saveFolderPath);
                        long totalSize = 0;
                        DateTime latestWriteTime = DateTime.MinValue;
                        
                        // 计算文件夹总大小和最新修改时间
                        foreach (FileInfo file in dirInfo.GetFiles())
                        {
                            totalSize += file.Length;
                            if (file.LastWriteTime > latestWriteTime)
                            {
                                latestWriteTime = file.LastWriteTime;
                            }
                        }
                        
                        SaveFileInfo saveInfo = new SaveFileInfo
                        {
                            SaveId = saveId,
                            CreationTime = metadata != null ? metadata.CreationTime : dirInfo.CreationTime,
                            LastWriteTime = metadata != null ? metadata.LastWriteTime : 
                                           (latestWriteTime != DateTime.MinValue ? latestWriteTime : dirInfo.LastWriteTime),
                            SizeInBytes = totalSize
                        };
                        
                        // 如果有元数据，添加名称和描述
                        if (metadata != null)
                        {
                            saveInfo.Name = metadata.Name;
                            saveInfo.Description = metadata.Description;
                            saveInfo.SaveMode = metadata.SaveMode;
                            saveInfo.SaveFormat = metadata.SaveFormat;
                        }
                        
                        return saveInfo;
                    }
                    else
                    {
                        // 获取特定数据文件信息
                        string filePath = GetSaveFilePath(saveId, dataKey);
                        if (!File.Exists(filePath))
                        {
                            Debug.LogWarning($"[SaveManager] 存档数据不存在: {saveId}/{dataKey}");
                            return null;
                        }
                        
                        FileInfo fileInfo = new FileInfo(filePath);
                        SaveFileInfo saveInfo = new SaveFileInfo
                        {
                            SaveId = $"{saveId}/{dataKey}",
                            CreationTime = fileInfo.CreationTime,
                            LastWriteTime = fileInfo.LastWriteTime,
                            SizeInBytes = fileInfo.Length
                        };
                        
                        // 如果有元数据，添加名称和描述
                        if (metadata != null)
                        {
                            saveInfo.Name = metadata.Name;
                            saveInfo.Description = metadata.Description;
                            saveInfo.SaveMode = metadata.SaveMode;
                            saveInfo.SaveFormat = metadata.SaveFormat;
                        }
                        
                        return saveInfo;
                    }
                }
                else
                {
                    // 单文件模式
                    string filePath = GetSaveFilePath(saveId);
                    if (!File.Exists(filePath))
                    {
                        Debug.LogWarning($"[SaveManager] 存档不存在: {saveId}");
                        return null;
                    }
                    
                    FileInfo fileInfo = new FileInfo(filePath);
                    SaveFileInfo saveInfo = new SaveFileInfo
                    {
                        SaveId = saveId,
                        CreationTime = metadata != null ? metadata.CreationTime : fileInfo.CreationTime,
                        LastWriteTime = metadata != null ? metadata.LastWriteTime : fileInfo.LastWriteTime,
                        SizeInBytes = fileInfo.Length
                    };
                    
                    // 如果有元数据，添加名称和描述
                    if (metadata != null)
                    {
                        saveInfo.Name = metadata.Name;
                        saveInfo.Description = metadata.Description;
                        saveInfo.SaveMode = metadata.SaveMode;
                        saveInfo.SaveFormat = metadata.SaveFormat;
                    }
                    
                    return saveInfo;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 获取存档信息失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取存档文件路径
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="dataKey">数据键（仅在文件夹模式下使用）</param>
        /// <returns>存档文件路径</returns>
        private static string GetSaveFilePath(string saveId, string dataKey = "main")
        {
            try
            {
                string extension = _serializer is JsonSaveSerializer ? ".json" : ".sav";
                
                // 确保基础存档目录存在
                if (!Directory.Exists(_savePath))
                {
                    Directory.CreateDirectory(_savePath);
                }
                
                if (_saveMode == SaveMode.FolderBased)
                {
                    string saveFolderPath = Path.Combine(_savePath, saveId);
                    // 确保存档文件夹存在
                    if (!Directory.Exists(saveFolderPath))
                    {
                        Directory.CreateDirectory(saveFolderPath);
                    }
                    return Path.Combine(saveFolderPath, dataKey + extension);
                }
                else
                {
                    return Path.Combine(_savePath, saveId + extension);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 获取存档文件路径失败: {ex.Message}\n存档ID: {saveId}, 数据键: {dataKey}");
                throw; // 重新抛出异常以便上层处理
            }
        }
        
        /// <summary>
        /// 获取存档文件夹路径
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <returns>存档文件夹路径</returns>
        private static string GetSaveFolderPath(string saveId)
        {
            try
            {
                // 确保基础存档目录存在
                if (!Directory.Exists(_savePath))
                {
                    Directory.CreateDirectory(_savePath);
                }
                
                string folderPath = Path.Combine(_savePath, saveId);
                
                // 确保存档文件夹存在
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                
                return folderPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 获取存档文件夹路径失败: {ex.Message}\n存档ID: {saveId}");
                throw; // 重新抛出异常以便上层处理
            }
        }

        /// <summary>
        /// 检查初始化状态
        /// </summary>
        private static void CheckInitialization()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }
    }
}