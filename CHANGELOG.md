# 更新日志

所有UGS Save System的显著更改都将记录在此文件中。

## [1.2.0] - 2024-05-15

### 新增

- 添加MemoryPack序列化支持
- 新增MemoryPackSaveSerializer类
- 添加MemoryPack序列化文档和示例代码
- 添加com.cysharp.memorypack依赖
- 将JSON序列化从Unity内置JsonUtility更改为Newtonsoft.Json

## [1.1.0] - 2023-12-15

### 新增

- 添加Protobuf序列化支持
- 新增ProtobufSaveSerializer类
- 添加Protobuf序列化文档和示例代码
- 添加protobuf-net依赖

## [1.0.0] - 2023-07-17

### 新增

- 初始版本发布
- 支持存储任意数据结构
- 提供完整的存档管理API（创建、获取、覆盖、删除等）
- 支持多种存档格式（JSON、二进制）
- 可自定义存档路径和格式
- 默认使用用户路径存储存档
- 支持存档加密
- 提供编辑器扩展工具
- 包含示例代码和文档