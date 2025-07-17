//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System;

namespace UGS.Save
{
    /// <summary>
    /// 存档文件信息
    /// </summary>
    [Serializable]
    public class SaveFileInfo
    {
        /// <summary>
        /// 存档ID
        /// </summary>
        public string SaveId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// 获取格式化的文件大小
        /// </summary>
        public string FormattedSize
        {
            get
            {
                if (SizeInBytes < 1024)
                    return $"{SizeInBytes} B";
                else if (SizeInBytes < 1024 * 1024)
                    return $"{SizeInBytes / 1024f:F2} KB";
                else
                    return $"{SizeInBytes / (1024f * 1024f):F2} MB";
            }
        }

        /// <summary>
        /// 获取格式化的创建时间
        /// </summary>
        public string FormattedCreationTime => CreationTime.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// 获取格式化的最后修改时间
        /// </summary>
        public string FormattedLastWriteTime => LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

        public override string ToString()
        {
            return $"SaveId: {SaveId}, Created: {FormattedCreationTime}, Modified: {FormattedLastWriteTime}, Size: {FormattedSize}";
        }
    }
}