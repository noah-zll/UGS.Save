//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using UnityEngine;

namespace UGS.Save.Examples
{
    /// <summary>
    /// MemoryPack存档示例
    /// </summary>
    public class MemoryPackSaveExample : MonoBehaviour
    {
        [SerializeField] private string _saveId = "memorypack_example";
        
        private void Start()
        {
            // 初始化存档系统
            SaveManager.Initialize();
            
            // 设置使用MemoryPack格式
            SaveManager.SetSaveFormat(SaveFormat.MemoryPack);
        }
        
        /// <summary>
        /// 保存游戏数据
        /// </summary>
        public void SaveGame()
        {
            // 创建玩家数据
            PlayerData playerData = new PlayerData(
                "MemoryPackPlayer",
                10,
                100.0f,
                new Vector3(1.0f, 2.0f, 3.0f),
                Quaternion.Euler(30, 45, 60),
                new Color(1.0f, 0.5f, 0.2f, 1.0f)
            );
            
            // 保存数据
            bool success = SaveManager.SaveGame(_saveId, playerData, "MemoryPack示例", "使用MemoryPack序列化的存档示例");
            
            if (success)
            {
                Debug.Log("[MemoryPackSaveExample] 保存成功!");
            }
            else
            {
                Debug.LogError("[MemoryPackSaveExample] 保存失败!");
            }
        }
        
        /// <summary>
        /// 加载游戏数据
        /// </summary>
        public void LoadGame()
        {
            // 加载数据
            PlayerData playerData = SaveManager.LoadGame<PlayerData>(_saveId);
            
            if (playerData != null)
            {
                Debug.Log($"[MemoryPackSaveExample] 加载成功! {playerData}");
            }
            else
            {
                Debug.LogError("[MemoryPackSaveExample] 加载失败或存档不存在!");
            }
        }
    }
}