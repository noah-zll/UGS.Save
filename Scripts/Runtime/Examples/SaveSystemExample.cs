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
        [SerializeField] private Text statusText;
        [SerializeField] private Text saveListText;

        [Header("Player References")]
        [SerializeField] private Transform playerTransform;

        private GameSaveData _currentSaveData;

        private void Start()
        {
            // 初始化存档系统
            SaveManager.Initialize();
            UpdateStatus("存档系统已初始化");

            // 创建默认存档数据
            _currentSaveData = new GameSaveData();
            UpdateUIFromData();

            // 更新存档列表
            UpdateSaveList();
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

            // 保存游戏
            bool success = SaveManager.SaveGame(saveIdInput.text, _currentSaveData);

            if (success)
            {
                UpdateStatus($"存档已保存: {saveIdInput.text}");
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

            // 加载游戏
            GameSaveData loadedData = SaveManager.LoadGame<GameSaveData>(saveIdInput.text);

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

                UpdateStatus($"存档已加载: {saveIdInput.text}");
            }
            else
            {
                UpdateStatus($"加载存档失败: {saveIdInput.text}");
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

            // 删除存档
            bool success = SaveManager.DeleteSave(saveIdInput.text);

            if (success)
            {
                UpdateStatus($"存档已删除: {saveIdInput.text}");
                UpdateSaveList();
            }
            else
            {
                UpdateStatus($"删除存档失败: {saveIdInput.text}");
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

                foreach (string saveId in saveList)
                {
                    SaveFileInfo saveInfo = SaveManager.GetSaveInfo(saveId);
                    if (saveInfo != null)
                    {
                        listText += $"{saveId} - {saveInfo.FormattedLastWriteTime} ({saveInfo.FormattedSize})\n";
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
        /// 更新状态文本
        /// </summary>
        private void UpdateStatus(string message)
        {
            statusText.text = message;
            Debug.Log($"[SaveSystemExample] {message}");
        }
    }
}