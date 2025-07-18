//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGS.Save.Examples
{
    /// <summary>
    /// 存档系统示例
    /// </summary>
    public class SaveSystemExample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private InputField levelInput;
        [SerializeField] private InputField scoreInput;
        [SerializeField] private Dropdown saveFormatDropdown;
        [SerializeField] private Toggle encryptionToggle;
        [SerializeField] private InputField saveIdInput;
        [SerializeField] private InputField saveNameInput;
        [SerializeField] private InputField saveDescriptionInput;
        [SerializeField] private Text statusText;
        [SerializeField] private Text saveListText;
        [SerializeField] private Dropdown saveModeDropdown;
        [SerializeField] private InputField dataKeyInput;

        [Header("Player References")]
        [SerializeField] private Transform playerTransform;

        private GameSaveData _currentSaveData;

        private void Start()
        {
            // 初始化存档系统
            SaveManager.Initialize();
            UpdateStatus("存档系统已初始化");
            
            // 初始化存档模式下拉菜单
            if (saveModeDropdown != null)
            {
                saveModeDropdown.ClearOptions();
                saveModeDropdown.AddOptions(new List<string> { "单文件模式", "文件夹模式" });
                SetSaveMode();
            }
            
            // 初始化数据键输入框
            if (dataKeyInput != null && string.IsNullOrEmpty(dataKeyInput.text))
            {
                dataKeyInput.text = "main";
            }

            // 创建默认存档数据
            _currentSaveData = new GameSaveData();
            UpdateUIFromData();

            // 更新存档列表
            UpdateSaveList();
        }

        /// <summary>
        /// 设置存档格式
        /// </summary>
        public void SetSaveFormat()
        {
            if (saveFormatDropdown != null)
            {
                SaveFormat format = (SaveFormat)saveFormatDropdown.value;
                SaveManager.SetSaveFormat(format);
                UpdateStatus($"存档格式已设置为: {format}");
            }
        }
        
        /// <summary>
        /// 设置存档模式
        /// </summary>
        public void SetSaveMode()
        {
            if (saveModeDropdown != null)
            {
                SaveMode mode = (SaveMode)saveModeDropdown.value;
                SaveManager.SetSaveMode(mode);
                UpdateStatus($"存档模式已设置为: {mode}");
                
                // 更新存档列表
                UpdateSaveList();
            }
        }

        /// <summary>
        /// 创建新存档
        /// </summary>
        public void CreateNewSave()
        {
            _currentSaveData = new GameSaveData();
            UpdateUIFromData();
            UpdateStatus("已创建新存档数据");
        }

        /// <summary>
        /// 从UI更新数据
        /// </summary>
        public void UpdateDataFromUI()
        {
            if (_currentSaveData == null)
                _currentSaveData = new GameSaveData();

            // 从UI获取数据
            _currentSaveData.playerName = playerNameInput.text;
            
            int level;
            if (int.TryParse(levelInput.text, out level))
                _currentSaveData.level = level;

            float score;
            if (float.TryParse(scoreInput.text, out score))
                _currentSaveData.score = score;

            // 如果有玩家Transform，更新位置和旋转
            if (playerTransform != null)
            {
                _currentSaveData.position = playerTransform.position;
                _currentSaveData.rotation = playerTransform.rotation;
            }

            // 更新保存时间
            _currentSaveData.UpdateSaveTime();

            UpdateStatus("数据已更新");
        }

        /// <summary>
        /// 从数据更新UI
        /// </summary>
        private void UpdateUIFromData()
        {
            if (_currentSaveData == null)
                return;

            playerNameInput.text = _currentSaveData.playerName;
            levelInput.text = _currentSaveData.level.ToString();
            scoreInput.text = _currentSaveData.score.ToString();
        }

        /// <summary>
        /// 保存游戏
        /// </summary>
        public void SaveGame()
        {
            if (string.IsNullOrEmpty(saveIdInput.text))
            {
                UpdateStatus("错误：请输入存档ID");
                return;
            }

            // 更新数据
            UpdateDataFromUI();

            // 设置存档格式
            SaveFormat format = saveFormatDropdown.value == 0 ? SaveFormat.Json : SaveFormat.Binary;
            SaveManager.SetSaveFormat(format);

            // 设置加密
            SaveManager.EnableEncryption(encryptionToggle.isOn);

            // 获取存档ID
            string saveId = saveIdInput.text;
            
            // 获取存档名称和描述
            string saveName = saveNameInput != null ? saveNameInput.text : "";
            string saveDescription = saveDescriptionInput != null ? saveDescriptionInput.text : "";
            
            // 保存游戏
            bool success;
            
            // 检查是否为文件夹模式
            if (saveModeDropdown != null && saveModeDropdown.value == (int)SaveMode.FolderBased && dataKeyInput != null)
            {
                // 获取数据键
                string dataKey = string.IsNullOrEmpty(dataKeyInput.text) ? "main" : dataKeyInput.text;
                
                // 在文件夹模式下保存游戏
                success = SaveManager.SaveGame(saveId, _currentSaveData, dataKey, saveName, saveDescription);
                
                if (success)
                {
                    UpdateStatus($"存档已保存: {saveId}/{dataKey}");
                }
            }
            else
            {
                // 在单文件模式下保存游戏
                success = SaveManager.SaveGame(saveId, _currentSaveData, saveName, saveDescription);
                
                if (success)
                {
                    UpdateStatus($"存档已保存: {saveId}");
                }
            }

            if (success)
            {
                UpdateSaveList();
            }
            else
            {
                UpdateStatus("保存存档失败");
            }
        }

        /// <summary>
        /// 加载游戏
        /// </summary>
        public void LoadGame()
        {
            if (string.IsNullOrEmpty(saveIdInput.text))
            {
                UpdateStatus("错误：请输入存档ID");
                return;
            }

            // 设置存档格式
            SaveFormat format = saveFormatDropdown.value == 0 ? SaveFormat.Json : SaveFormat.Binary;
            SaveManager.SetSaveFormat(format);

            // 设置加密
            SaveManager.EnableEncryption(encryptionToggle.isOn);

            // 获取存档ID
            string saveId = saveIdInput.text;
            
            // 加载游戏
            GameSaveData loadedData;
            
            // 检查是否为文件夹模式
            if (saveModeDropdown != null && saveModeDropdown.value == (int)SaveMode.FolderBased && dataKeyInput != null)
            {
                // 获取数据键
                string dataKey = string.IsNullOrEmpty(dataKeyInput.text) ? "main" : dataKeyInput.text;
                
                // 在文件夹模式下加载游戏
                loadedData = SaveManager.LoadGame<GameSaveData>(saveId, dataKey);
                
                if (loadedData != null)
                {
                    UpdateStatus($"存档已加载: {saveId}/{dataKey}");
                }
            }
            else
            {
                // 在单文件模式下加载游戏
                loadedData = SaveManager.LoadGame<GameSaveData>(saveId);
                
                if (loadedData != null)
                {
                    UpdateStatus($"存档已加载: {saveId}");
                }
            }

            if (loadedData != null)
            {
                _currentSaveData = loadedData;
                UpdateUIFromData();

                // 如果有玩家Transform，更新位置和旋转
                if (playerTransform != null)
                {
                    playerTransform.position = _currentSaveData.position;
                    playerTransform.rotation = _currentSaveData.rotation;
                }
            }
            else
            {
                UpdateStatus($"加载存档失败: {saveId}");
            }
        }

        /// <summary>
        /// 删除存档
        /// </summary>
        public void DeleteSave()
        {
            if (string.IsNullOrEmpty(saveIdInput.text))
            {
                UpdateStatus("错误：请输入存档ID");
                return;
            }

            // 获取存档ID
            string saveId = saveIdInput.text;
            
            // 删除存档
            bool success;
            
            // 检查是否为文件夹模式
            if (saveModeDropdown != null && saveModeDropdown.value == (int)SaveMode.FolderBased && dataKeyInput != null && !string.IsNullOrEmpty(dataKeyInput.text))
            {
                // 获取数据键
                string dataKey = dataKeyInput.text;
                
                // 在文件夹模式下删除特定数据文件
                success = SaveManager.DeleteSave(saveId, dataKey);
                
                if (success)
                {
                    UpdateStatus($"存档数据已删除: {saveId}/{dataKey}");
                }
            }
            else
            {
                // 删除整个存档
                success = SaveManager.DeleteSave(saveId);
                
                if (success)
                {
                    UpdateStatus($"存档已删除: {saveId}");
                }
            }

            if (success)
            {
                UpdateSaveList();
            }
            else
            {
                UpdateStatus("删除存档失败");
            }
        }

        /// <summary>
        /// 更新存档列表
        /// </summary>
        public void UpdateSaveList()
        {
            List<string> saveList = SaveManager.GetSaveList();

            if (saveList.Count > 0)
            {
                string listText = "存档列表:\n";

                // 检查是否为文件夹模式
                bool isFolderMode = saveModeDropdown != null && saveModeDropdown.value == (int)SaveMode.FolderBased;
                
                foreach (string saveId in saveList)
                {
                    SaveFileInfo saveInfo = SaveManager.GetSaveInfo(saveId);
                    if (saveInfo != null)
                    {
                        // 显示存档名称、描述、模式和格式
                        string saveName = !string.IsNullOrEmpty(saveInfo.Name) ? saveInfo.Name : saveId;
                        string saveDesc = !string.IsNullOrEmpty(saveInfo.Description) ? $" - {saveInfo.Description}" : "";
                        string saveMode = $"[{saveInfo.SaveMode}]";
                        string saveFormat = $"[{saveInfo.SaveFormat}]";
                        
                        listText += $"{saveName}{saveDesc} - {saveInfo.FormattedLastWriteTime} ({saveInfo.FormattedSize}) {saveMode} {saveFormat}\n";
                        
                        // 如果是文件夹模式，显示数据键列表
                        if (isFolderMode)
                        {
                            List<string> dataKeys = SaveManager.GetSaveDataKeys(saveId);
                            if (dataKeys != null && dataKeys.Count > 0)
                            {
                                listText += "  数据键:\n";
                                foreach (string dataKey in dataKeys)
                                {
                                    SaveFileInfo keyInfo = SaveManager.GetSaveInfo(saveId, dataKey);
                                    if (keyInfo != null)
                                    {
                                        listText += $"  - {dataKey} - {keyInfo.FormattedLastWriteTime} ({keyInfo.FormattedSize})\n";
                                    }
                                    else
                                    {
                                        listText += $"  - {dataKey}\n";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        listText += $"{saveId}\n";
                    }
                }

                saveListText.text = listText;
            }
            else
            {
                saveListText.text = "没有存档";
            }
        }

        /// <summary>
        /// 删除所有存档
        /// </summary>
        public void DeleteAllSaves()
        {
            // 删除所有存档
            bool success = SaveManager.DeleteAllSaves();
            
            if (success)
            {
                UpdateStatus("所有存档已删除");
                UpdateSaveList();
            }
            else
            {
                UpdateStatus("删除所有存档失败");
            }
        }
        
        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatus(string message)
        {
            statusText.text = message;
            Debug.Log($"[SaveSystemExample] {message}");
        }
    }
}