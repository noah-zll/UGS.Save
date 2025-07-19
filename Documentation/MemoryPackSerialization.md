# MemoryPack 序列化

UGS.Save 系统现在支持使用 MemoryPack 进行高性能的二进制序列化。MemoryPack 是一个极快的二进制序列化库，专为 Unity 和 .NET 设计，提供了比传统 JSON 或二进制序列化更高的性能。

## 特点

- **极高性能**：MemoryPack 比 JSON 和其他二进制序列化方式快数倍
- **内存效率**：生成的二进制数据更小，减少存储空间和传输带宽
- **AOT 兼容**：完全支持 IL2CPP 和 AOT 平台
- **零拷贝**：优化的内存使用，减少 GC 压力

## 安装

MemoryPack 已作为依赖项添加到 UGS.Save 包中。当您导入 UGS.Save 包时，Unity Package Manager 会自动安装 MemoryPack。

## 使用方法

### 1. 准备可序列化的类

使用 MemoryPack 需要在您的数据类上添加 `[MemoryPackable]` 特性，并将类声明为 `partial`：

```csharp
[MemoryPackable]
public partial class PlayerData
{
    public string PlayerName { get; set; }
    public int Level { get; set; }
    public float Health { get; set; }
    
    // 需要提供一个无参构造函数
    public PlayerData()
    {
    }
}
```

### 2. 设置 SaveManager 使用 MemoryPack 格式

```csharp
// 初始化存档系统
SaveManager.Initialize();

// 设置使用 MemoryPack 格式
SaveManager.SetSaveFormat(SaveFormat.MemoryPack);
```

### 3. 保存和加载数据

```csharp
// 保存数据
PlayerData playerData = new PlayerData { PlayerName = "Player1", Level = 10, Health = 100.0f };
SaveManager.SaveGame("save1", playerData);

// 加载数据
PlayerData loadedData = SaveManager.LoadGame<PlayerData>("save1");
```

## 注意事项

1. **类型标记**：所有需要序列化的类必须标记 `[MemoryPackable]` 特性并声明为 `partial`
2. **构造函数**：必须提供无参构造函数
3. **支持的类型**：
   - 基本类型（int, float, string 等）
   - 集合类型（List<T>, Dictionary<K,V> 等）
   - Unity 类型（Vector2, Vector3, Quaternion 等）
   - 嵌套的 MemoryPackable 类型

## 性能比较

与其他序列化方式相比，MemoryPack 通常能提供：

- 比 JSON 快 5-10 倍的序列化/反序列化速度
- 比 Protobuf 快 1.5-3 倍
- 比 Unity 的二进制序列化快 3-8 倍

## 示例

请参考 `Scripts/Runtime/Examples/MemoryPackSaveExample.cs` 文件，了解完整的使用示例。

## 限制

- 所有需要序列化的类型必须在编译时已知
- 不支持多态序列化（除非使用 MemoryPack 的多态序列化特性）
- IL2CPP 平台需要正确配置链接器设置
- **不支持嵌套类型**：标记为 `[MemoryPackable]` 的类不能是嵌套类型（即不能是其他类的内部类）

## 常见错误

### 嵌套类型错误

如果您尝试将 `[MemoryPackable]` 特性应用于嵌套类型，将会收到以下错误：

```
error MEMPACK002: The MemoryPackable object 'PlayerData' must be not nested type
```

**解决方案**：将嵌套类移动到独立的文件中，使其成为顶级类。例如，不要将 `PlayerData` 类定义在 `MonoBehaviour` 类内部，而是将其定义为单独的类文件。

### 多构造函数错误

如果您的 `[MemoryPackable]` 类有多个构造函数，将会收到以下错误：

```
error MEMPACK004: The MemoryPackable object 'PlayerData' must annotate with [MemoryPackConstructor] when exists multiple constructors
```

**解决方案**：当类有多个构造函数时，必须使用 `[MemoryPackConstructor]` 特性标记其中一个构造函数，通常是无参构造函数：

```csharp
[MemoryPackable]
public partial class PlayerData
{
    public string PlayerName { get; set; }
    
    // 标记为MemoryPack构造函数
    [MemoryPackConstructor]
    public PlayerData()
    {
    }
    
    // 其他构造函数
    public PlayerData(string name)
    {
        PlayerName = name;
    }
}
