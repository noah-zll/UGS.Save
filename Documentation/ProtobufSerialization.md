# Protobuf 序列化支持

本文档介绍如何在 UGS Save System 中使用 Protobuf 序列化格式。

## 安装依赖

要使用 Protobuf 序列化，您需要先安装 protobuf-net 包。

### 通过 NuGet 安装（推荐）

1. 在 Unity 中，打开 Package Manager（菜单：Window > Package Manager）
2. 点击 + 按钮，选择 "Add package from git URL..."
3. 输入 `com.unity.nuget.newtonsoft-json`，点击 Add
4. 在您的项目中添加 NuGet 包管理器后，创建一个 `Packages.config` 文件，添加以下内容：

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="protobuf-net" version="3.2.26" targetFramework="netstandard2.1" />
</packages>
```

5. 使用 NuGet CLI 或 Visual Studio 安装依赖包

### 手动安装

1. 从 [protobuf-net GitHub 仓库](https://github.com/protobuf-net/protobuf-net/releases) 下载最新版本
2. 将 DLL 文件复制到您的 Unity 项目的 `Assets/Plugins` 目录中

## 使用 Protobuf 序列化

### 配置存档系统使用 Protobuf

```csharp
// 初始化存档系统
SaveManager.Initialize();

// 设置使用 Protobuf 格式
SaveManager.SetSaveFormat(SaveFormat.Protobuf);
```

### 为数据类添加 Protobuf 特性

要使用 Protobuf 序列化，您需要为数据类添加相应的特性：

```csharp
using ProtoBuf;

[ProtoContract]
public class PlayerData
{
    [ProtoMember(1)]
    public string playerName;
    
    [ProtoMember(2)]
    public int level;
    
    [ProtoMember(3)]
    public float health;
    
    [ProtoMember(4)]
    public List<string> inventory;
}
```

### 重要说明

1. 所有需要序列化的类必须标记 `[ProtoContract]` 特性
2. 所有需要序列化的字段或属性必须标记 `[ProtoMember(序号)]` 特性，序号必须是唯一的正整数
3. 序列化的类必须有一个无参构造函数（可以是私有的）
4. Unity 的特殊类型（如 Vector3、Quaternion 等）需要特殊处理，建议转换为可序列化的普通类型

## Protobuf 序列化的优势

- **性能高效**：比 JSON 和二进制序列化更快，生成的数据更小
- **跨平台兼容**：可以与其他使用 Protobuf 的系统无缝集成
- **向前兼容**：可以在不破坏现有数据的情况下添加新字段
- **类型安全**：强类型定义，减少运行时错误

## 注意事项

- Protobuf 不支持多态序列化，除非显式配置
- 循环引用需要特殊处理
- 使用 Protobuf 时，数据类的结构变更需要谨慎，以保持向前兼容性