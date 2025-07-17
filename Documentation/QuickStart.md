# UGS Save System 快速入门指南

## 安装

### 通过Unity Package Manager

1. 打开Unity项目
2. 打开Package Manager (Window > Package Manager)
3. 点击 "+" 按钮，选择 "Add package from git URL..."
4. 输入: `git@github.com/ugs/UGS.Save.git`

### 手动安装

1. 下载此仓库
2. 将UGS.Save文件夹复制到你的Unity项目的Assets文件夹中

## 基本用法

### 初始化存档系统

在使用存档系统之前，需要先初始化它。通常在游戏启动时进行初始化：

```csharp
private void Start()
{
    // 使用默认路径初始化
    SaveManager.Initialize();
    
    // 或者使用自定义路径
    // SaveManager.Initialize("C:/MyGameSaves");
}
```

### 创建存档数据类

创建一个可序列化的类来存储游戏数据：

```csharp
[Serializable]
public class GameData
{
    public string playerName;
    public int level;
    public float score;
    public Vector3Data playerPosition;
    
    // 自定义构造函数
    public GameData()
    {
        playerName = "Player";
        level = 1;
        score = 0;
        playerPosition = new Vector3Data();
    }
}

[Serializable]
public class Vector3Data
{
    public float x, y, z;
    
    public Vector3Data()
    {
        x = y = z = 0;
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
}
```

### 保存游戏数据

```csharp
public void SaveGame()
{
    // 创建游戏数据
    GameData gameData = new GameData();
    gameData.playerName = "John";
    gameData.level = 10;
    gameData.score = 1500.5f;
    gameData.playerPosition = new Vector3Data(transform.position);
    
    // 保存游戏数据
    SaveManager.SaveGame("save1", gameData);
}
```

### 加载游戏数据

```csharp
public void LoadGame()
{
    // 加载游戏数据
    GameData gameData = SaveManager.LoadGame<GameData>("save1");
    
    if (gameData != null)
    {
        // 使用加载的数据
        Debug.Log($"Player: {gameData.playerName}, Level: {gameData.level}");
        transform.position = gameData.playerPosition.ToVector3();
    }
    else
    {
        Debug.LogWarning("No save data found!");
    }
}
```

### 获取存档列表

```csharp
public void ListSaves()
{
    List<string> saveList = SaveManager.GetSaveList();
    
    Debug.Log($"Found {saveList.Count} saves:");
    foreach (string saveId in saveList)
    {
        SaveFileInfo info = SaveManager.GetSaveInfo(saveId);
        Debug.Log($"{saveId} - {info.FormattedLastWriteTime} ({info.FormattedSize})");
    }
}
```

### 删除存档

```csharp
public void DeleteSave(string saveId)
{
    if (SaveManager.SaveExists(saveId))
    {
        SaveManager.DeleteSave(saveId);
        Debug.Log($"Save '{saveId}' deleted.");
    }
    else
    {
        Debug.LogWarning($"Save '{saveId}' does not exist.");
    }
}
```

## 高级用法

### 使用二进制格式

```csharp
// 设置存档格式为二进制
SaveManager.SetSaveFormat(SaveFormat.Binary);

// 保存游戏数据（将使用二进制格式）
SaveManager.SaveGame("save1", gameData);
```

### 启用加密

```csharp
// 启用加密，使用默认密钥
SaveManager.EnableEncryption(true);

// 或者使用自定义密钥
SaveManager.EnableEncryption(true, "my-secret-key");

// 保存游戏数据（将被加密）
SaveManager.SaveGame("save1", gameData);
```

### 自定义存档路径

```csharp
// 设置自定义存档路径
SaveManager.SetSavePath("C:/MyGameSaves");
```

## 编辑器工具

UGS Save System提供了一个编辑器窗口，可以方便地管理存档文件。

1. 打开编辑器窗口：Window > UGS > Save Manager
2. 在窗口中，你可以：
   - 设置存档路径
   - 选择存档格式
   - 启用/禁用加密
   - 查看存档列表
   - 删除存档
   - 查看存档详细信息