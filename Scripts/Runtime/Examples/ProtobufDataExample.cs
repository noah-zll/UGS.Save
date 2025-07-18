//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace UGS.Save.Examples
{
    /// <summary>
    /// Protobuf数据示例类
    /// 展示如何使用Protobuf特性标记数据类以支持Protobuf序列化
    /// </summary>
    [ProtoContract]
    public class ProtobufDataExample
    {
        // 必须为每个需要序列化的字段或属性添加ProtoMember特性
        // 数字参数是唯一标识符，用于标识字段在序列化数据中的位置
        // 建议按顺序编号，但不是必须的
        
        [ProtoMember(1)]
        public string playerName;
        
        [ProtoMember(2)]
        public int level;
        
        [ProtoMember(3)]
        public float health;
        
        [ProtoMember(4)]
        public bool isAlive;
        
        // 集合类型也可以序列化
        [ProtoMember(5)]
        public List<string> inventory;
        
        // 可以序列化属性
        private DateTime _lastSaveTime;
        
        [ProtoMember(6)]
        public DateTime LastSaveTime
        {
            get => _lastSaveTime;
            set => _lastSaveTime = value;
        }
        
        // 嵌套类也需要标记ProtoContract
        [ProtoMember(7)]
        public PlayerStats stats;
        
        // 对于Unity特有类型，需要转换为可序列化的类型
        private Vector3 _position;
        
        [ProtoMember(8)]
        public SerializableVector3 Position
        {
            get => new SerializableVector3(_position);
            set => _position = value.ToVector3();
        }
        
        // 默认构造函数（可以是私有的）
        // Protobuf需要一个无参构造函数来创建对象实例
        private ProtobufDataExample() { }
        
        public ProtobufDataExample(string name, int level)
        {
            playerName = name;
            this.level = level;
            health = 100f;
            isAlive = true;
            inventory = new List<string>();
            _lastSaveTime = DateTime.Now;
            stats = new PlayerStats();
            _position = Vector3.zero;
        }
    }
    
    [ProtoContract]
    public class PlayerStats
    {
        [ProtoMember(1)]
        public int strength;
        
        [ProtoMember(2)]
        public int agility;
        
        [ProtoMember(3)]
        public int intelligence;
        
        public PlayerStats()
        {
            strength = 10;
            agility = 10;
            intelligence = 10;
        }
    }
    
    /// <summary>
    /// 可序列化的Vector3
    /// </summary>
    [ProtoContract]
    public struct SerializableVector3
    {
        [ProtoMember(1)]
        public float x;
        
        [ProtoMember(2)]
        public float y;
        
        [ProtoMember(3)]
        public float z;
        
        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
        
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}