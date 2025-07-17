# UGS Save System

## 简介

UGS Save System是一个灵活的Unity游戏存档系统，提供了简单易用的API来管理游戏存档。

## 特性

- 支持存储任意数据结构
- 提供完整的存档管理API（创建、获取、覆盖、删除等）
- 支持多种存档格式（JSON、二进制）
- 可自定义存档路径和格式
- 默认使用用户路径存储存档

## 安装

### 通过Unity Package Manager

1. 打开Unity项目
2. 打开Package Manager (Window > Package Manager)
3. 点击 "+" 按钮，选择 "Add package from git URL..."
4. 输入: `git@github.com:noah-zll/UGS.Save.git`

### 手动安装

1. 下载此仓库
2. 将UGS.Save文件夹复制到你的Unity项目的Assets文件夹中

## 快速开始

```csharp
// 初始化存档系统
SaveManager.Initialize();

// 创建存档数据
GameSaveData saveData = new GameSaveData
{
    playerName = "Player1",
    level = 10,
    score = 1500,
    position = new Vector3(10, 0, 5)
};

// 保存存档
SaveManager.SaveGame("save1", saveData);

// 加载存档
GameSaveData loadedData = SaveManager.LoadGame<GameSaveData>("save1");

// 获取存档列表
List<string> saveFiles = SaveManager.GetSaveList();

// 删除存档
SaveManager.DeleteSave("save1");
```

## 高级用法

### 自定义存档路径

```csharp
// 设置自定义存档路径
SaveManager.SetSavePath("C:/CustomSavePath");
```

### 使用二进制格式

```csharp
// 设置存档格式为二进制
SaveManager.SetSaveFormat(SaveFormat.Binary);
```

### 加密存档

```csharp
// 启用存档加密
SaveManager.EnableEncryption(true, "your-encryption-key");
```

## API参考

详细的API文档请参考[API文档](Documentation/API.md)。

## 许可证

本项目采用MIT许可证。详情请参阅[LICENSE](LICENSE)文件。

