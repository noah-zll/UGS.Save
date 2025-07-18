//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;

namespace UGS.Save
{
    /// <summary>
    /// 存档元数据信息
    /// </summary>
    [Serializable]
    public class SaveMetadata
    {
        /// <summary>
        /// 存档ID
        /// </summary>
        public string SaveId { get; set; }
        
        /// <summary>
        /// 存档名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 存档描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 存档模式
        /// </summary>
        public SaveMode SaveMode { get; set; }
        
        /// <summary>
        /// 存档格式
        /// </summary>
        public SaveFormat SaveFormat { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastWriteTime { get; set; }
        
        /// <summary>
        /// 获取格式化的创建时间
        /// </summary>
        public string FormattedCreationTime => CreationTime.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// 获取格式化的最后修改时间
        /// </summary>
        public string FormattedLastWriteTime => LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
        
        public SaveMetadata()
        {
            CreationTime = DateTime.Now;
            LastWriteTime = DateTime.Now;
            SaveMode = SaveMode.SingleFile;
            SaveFormat = SaveFormat.Json;
        }
        
        public SaveMetadata(string saveId, string name = "", string description = "")
        {
            SaveId = saveId;
            Name = string.IsNullOrEmpty(name) ? saveId : name;
            Description = description;
            CreationTime = DateTime.Now;
            LastWriteTime = DateTime.Now;
            SaveMode = SaveMode.SingleFile;
            SaveFormat = SaveFormat.Json;
        }
        
        public override string ToString()
        {
            return $"SaveId: {SaveId}, Name: {Name}, Mode: {SaveMode}, Format: {SaveFormat}, Created: {FormattedCreationTime}";
        }
    }
}