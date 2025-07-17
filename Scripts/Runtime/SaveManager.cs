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
                default:
                    throw new ArgumentException("不支持的存档格式");
            }

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
        /// 保存游戏数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="saveId">存档ID</param>
        /// <param name="data">要保存的数据</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveGame<T>(string saveId, T data) where T : class
        {
            CheckInitialization();

            try
            {
                string filePath = GetSaveFilePath(saveId);
                string serializedData = _serializer.Serialize(data);

                if (_useEncryption)
                {
                    serializedData = EncryptionUtility.Encrypt(serializedData, _encryptionKey);
                }

                File.WriteAllText(filePath, serializedData);
                Debug.Log($"[SaveManager] 存档保存成功: {saveId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 保存存档失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="saveId">存档ID</param>
        /// <returns>加载的数据，加载失败返回null</returns>
        public static T LoadGame<T>(string saveId) where T : class
        {
            CheckInitialization();

            try
            {
                string filePath = GetSaveFilePath(saveId);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveManager] 存档不存在: {saveId}");
                    return null;
                }

                string serializedData = File.ReadAllText(filePath);

                if (_useEncryption)
                {
                    serializedData = EncryptionUtility.Decrypt(serializedData, _encryptionKey);
                }

                T data = _serializer.Deserialize<T>(serializedData);
                Debug.Log($"[SaveManager] 存档加载成功: {saveId}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 加载存档失败: {ex.Message}");
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
                string[] files = Directory.GetFiles(_savePath);

                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    saveList.Add(fileName);
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
        /// 检查存档是否存在
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <returns>是否存在</returns>
        public static bool SaveExists(string saveId)
        {
            CheckInitialization();
            string filePath = GetSaveFilePath(saveId);
            return File.Exists(filePath);
        }

        /// <summary>
        /// 删除存档
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <returns>是否删除成功</returns>
        public static bool DeleteSave(string saveId)
        {
            CheckInitialization();

            try
            {
                string filePath = GetSaveFilePath(saveId);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveManager] 要删除的存档不存在: {saveId}");
                    return false;
                }

                File.Delete(filePath);
                Debug.Log($"[SaveManager] 存档删除成功: {saveId}");
                return true;
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
                string[] files = Directory.GetFiles(_savePath);

                foreach (string file in files)
                {
                    File.Delete(file);
                }

                Debug.Log($"[SaveManager] 所有存档删除成功，共{files.Length}个");
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
        /// <returns>存档文件信息</returns>
        public static SaveFileInfo GetSaveInfo(string saveId)
        {
            CheckInitialization();

            try
            {
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
                    CreationTime = fileInfo.CreationTime,
                    LastWriteTime = fileInfo.LastWriteTime,
                    SizeInBytes = fileInfo.Length
                };

                return saveInfo;
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
        /// <returns>存档文件路径</returns>
        private static string GetSaveFilePath(string saveId)
        {
            string extension = _serializer is JsonSaveSerializer ? ".json" : ".sav";
            return Path.Combine(_savePath, saveId + extension);
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