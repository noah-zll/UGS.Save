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
    /// Protobuf存档示例
    /// 展示如何使用Protobuf格式保存和加载游戏数据
    /// </summary>
    public class ProtobufSaveExample : MonoBehaviour
    {
        [Header("UI引用")]
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private InputField levelInput;
        [SerializeField] private Toggle isAliveToggle;
        [SerializeField] private InputField healthInput;
        [SerializeField] private InputField inventoryInput;
        [SerializeField] private Text statusText;
        
        [Header("存档设置")]
        [SerializeField] private string saveId = "protobuf_example";
        
        private ProtobufDataExample _playerData;
        
        private void Start()
        {
            // 初始化存档系统
            SaveManager.Initialize();
            
            // 设置使用Protobuf格式
            SaveManager.SetSaveFormat(SaveFormat.Protobuf);
            
            // 创建默认数据
            _playerData = new ProtobufDataExample("玩家", 1);
            
            // 更新UI
            UpdateUI();
            
            // 显示状态信息
            ShowStatus("初始化完成，使用Protobuf格式");
        }
        
        /// <summary>
        /// 保存游戏数据
        /// </summary>
        public void SaveGame()
        {
            // 从UI更新数据
            UpdateDataFromUI();
            
            try
            {
                // 保存数据
                SaveManager.SaveGame(saveId, _playerData, "player_data");
                ShowStatus("保存成功！");
            }
            catch (System.Exception ex)
            {
                ShowStatus($"保存失败: {ex.Message}");
                Debug.LogError($"保存失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 加载游戏数据
        /// </summary>
        public void LoadGame()
        {
            try
            {
                // 加载数据
                _playerData = SaveManager.LoadGame<ProtobufDataExample>(saveId, "player_data");
                
                Debug.Log($"加载数据: {_playerData}");

                // 更新UI
                UpdateUI();
                
                ShowStatus("加载成功！");
            }
            catch (System.Exception ex)
            {
                ShowStatus($"加载失败: {ex.Message}");
                Debug.LogError($"加载失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 删除存档
        /// </summary>
        public void DeleteSave()
        {
            try
            {
                SaveManager.DeleteSave(saveId);
                ShowStatus("存档已删除！");
            }
            catch (System.Exception ex)
            {
                ShowStatus($"删除失败: {ex.Message}");
                Debug.LogError($"删除失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 从UI更新数据
        /// </summary>
        private void UpdateDataFromUI()
        {
            _playerData.playerName = playerNameInput.text;
            
            if (int.TryParse(levelInput.text, out int level))
            {
                _playerData.level = level;
            }
            
            _playerData.isAlive = isAliveToggle.isOn;
            
            if (float.TryParse(healthInput.text, out float health))
            {
                _playerData.health = health;
            }
            
            // 更新物品栏
            string[] items = inventoryInput.text.Split(',');
            _playerData.inventory = new List<string>();
            foreach (string item in items)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    _playerData.inventory.Add(item.Trim());
                }
            }
            
            // 更新位置（示例）
            _playerData.Position = new SerializableVector3(transform.position);
            
            // 更新保存时间
            _playerData.LastSaveTime = System.DateTime.Now;
        }
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            playerNameInput.text = _playerData.playerName;
            levelInput.text = _playerData.level.ToString();
            isAliveToggle.isOn = _playerData.isAlive;
            healthInput.text = _playerData.health.ToString();
            
            // 更新物品栏显示
            if (_playerData.inventory != null && _playerData.inventory.Count > 0)
            {
                inventoryInput.text = string.Join(", ", _playerData.inventory);
            }
            else
            {
                inventoryInput.text = "";
            }
        }
        
        /// <summary>
        /// 显示状态信息
        /// </summary>
        private void ShowStatus(string message)
        {
            statusText.text = message;
            Debug.Log($"[ProtobufSaveExample] {message}");
        }
    }
}