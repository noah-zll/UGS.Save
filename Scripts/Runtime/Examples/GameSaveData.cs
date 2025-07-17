//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGS.Save.Examples
{
    /// <summary>
    /// 游戏存档数据示例类
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>
        /// 玩家名称
        /// </summary>
        public string playerName;

        /// <summary>
        /// 玩家等级
        /// </summary>
        public int level;

        /// <summary>
        /// 玩家分数
        /// </summary>
        public float score;

        /// <summary>
        /// 游戏时间（秒）
        /// </summary>
        public float playTime;

        /// <summary>
        /// 最后保存时间
        /// </summary>
        public string saveTime;

        /// <summary>
        /// 玩家位置
        /// </summary>
        public Vector3Data position;

        /// <summary>
        /// 玩家旋转
        /// </summary>
        public QuaternionData rotation;

        /// <summary>
        /// 已解锁的成就列表
        /// </summary>
        public List<string> unlockedAchievements = new List<string>();

        /// <summary>
        /// 物品库存
        /// </summary>
        public List<InventoryItem> inventory = new List<InventoryItem>();

        /// <summary>
        /// 玩家状态数据
        /// </summary>
        public PlayerStatus playerStatus = new PlayerStatus();

        /// <summary>
        /// 创建一个新的游戏存档数据
        /// </summary>
        public GameSaveData()
        {
            playerName = "Player";
            level = 1;
            score = 0;
            playTime = 0;
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            position = new Vector3Data();
            rotation = new QuaternionData();
        }

        /// <summary>
        /// 更新保存时间
        /// </summary>
        public void UpdateSaveTime()
        {
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 添加物品到库存
        /// </summary>
        public void AddItem(string itemId, int count = 1)
        {
            InventoryItem existingItem = inventory.Find(item => item.itemId == itemId);
            if (existingItem != null)
            {
                existingItem.count += count;
            }
            else
            {
                inventory.Add(new InventoryItem { itemId = itemId, count = count });
            }
        }

        /// <summary>
        /// 解锁成就
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!unlockedAchievements.Contains(achievementId))
            {
                unlockedAchievements.Add(achievementId);
            }
        }

        public override string ToString()
        {
            return $"Player: {playerName}, Level: {level}, Score: {score}, PlayTime: {playTime}s";
        }
    }

    /// <summary>
    /// 可序列化的Vector3
    /// </summary>
    [Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3Data()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public Vector3Data(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static implicit operator Vector3(Vector3Data data)
        {
            return data.ToVector3();
        }

        public static implicit operator Vector3Data(Vector3 vector)
        {
            return new Vector3Data(vector);
        }
    }

    /// <summary>
    /// 可序列化的Quaternion
    /// </summary>
    [Serializable]
    public class QuaternionData
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public QuaternionData()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 1;
        }

        public QuaternionData(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }

        public static implicit operator Quaternion(QuaternionData data)
        {
            return data.ToQuaternion();
        }

        public static implicit operator QuaternionData(Quaternion quaternion)
        {
            return new QuaternionData(quaternion);
        }
    }

    /// <summary>
    /// 库存物品
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public string itemId;
        public int count;
    }

    /// <summary>
    /// 玩家状态
    /// </summary>
    [Serializable]
    public class PlayerStatus
    {
        public float health = 100;
        public float mana = 100;
        public float stamina = 100;
        public bool isAlive = true;
    }
}