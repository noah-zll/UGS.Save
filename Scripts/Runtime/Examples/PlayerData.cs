//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using UnityEngine;
using MemoryPack;

namespace UGS.Save.Examples
{
    /// <summary>
    /// 玩家数据类，用于MemoryPack序列化示例
    /// </summary>
    [MemoryPackable]
    public partial class PlayerData
    {
        public string PlayerName { get; set; }
        public int Level { get; set; }
        public float Health { get; set; }
        public Vector3 Position { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; }
        public Color PlayerColor { get; set; }
        
        // 需要提供一个无参构造函数，并标记为MemoryPack构造函数
        [MemoryPackConstructor]
        public PlayerData()
        {
        }
        
        public PlayerData(string name, int level, float health, Vector3 position, Quaternion rotation, Color playerColor)
        {
            PlayerName = name;
            Level = level;
            Health = health;
            Position = position;
            Rotation = rotation;
            PlayerColor = playerColor;
        }
        
        public override string ToString()
        {
            return $"Player: {PlayerName}, Level: {Level}, Health: {Health}, Position: {Position}, Rotation: {Rotation}, Color: {PlayerColor}";
        }
    }
}