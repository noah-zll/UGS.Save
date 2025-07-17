# UGS Save System API 文档

## 目录

1. [SaveManager](#savemanager)
2. [SaveFormat](#saveformat)
3. [SaveFileInfo](#savefileinfo)
4. [ISaveSerializer](#isaveserializer)
5. [JsonSaveSerializer](#jsonsaveserializer)
6. [BinarySaveSerializer](#binarysaveserializer)
7. [EncryptionUtility](#encryptionutility)

## SaveManager

`SaveManager` 是存档系统的主要入口，提供了所有存档相关的功能。

### 方法

#### Initialize

```csharp
public static void Initialize(string customPath = "")
```

初始化存档系统。如果不提供自定义路径，将使用 `Application.persistentDataPath/Saves` 作为默认存档路径。

**参数:**
- `customPath`: 自定义存档路径，为空则使用默认路径

#### SetSavePath

```csharp
public static void SetSavePath(string path)
```

设置存档路径。

**参数:**
- `path`: 存档路径

#### SetSaveFormat

```csharp
public static void SetSaveFormat(SaveFormat format)
```

设置存档格式。

**参数:**
- `format`: 存档格式，可选值为 `SaveFormat.Json` 或 `SaveFormat.Binary`

#### EnableEncryption

```csharp
public static void EnableEncryption(bool enable, string encryptionKey = "")
```

启用或禁用存档加密。

**参数:**
- `enable`: 是否启用加密
- `encryptionKey`: 加密密钥，为空则使用默认密钥

#### SaveGame

```csharp
public static bool SaveGame<T>(string saveId, T data) where T : class
```

保存游戏数据。

**参数:**
- `saveId`: 存档ID
- `data`: 要保存的数据

**返回值:**
- 是否保存成功

#### LoadGame

```csharp
public static T LoadGame<T>(string saveId) where T : class
```

加载游戏数据。

**参数:**
- `saveId`: 存档ID

**返回值:**
- 加载的数据，加载失败返回null

#### GetSaveList

```csharp
public static List<string> GetSaveList()
```

获取存档列表。

**返回值:**
- 存档ID列表

#### SaveExists

```csharp
public static bool SaveExists(string saveId)
```

检查存档是否存在。

**参数:**
- `saveId`: 存档ID

**返回值:**
- 是否存在

#### DeleteSave

```csharp
public static bool DeleteSave(string saveId)
```

删除存档。

**参数:**
- `saveId`: 存档ID

**返回值:**
- 是否删除成功

#### DeleteAllSaves

```csharp
public static bool DeleteAllSaves()
```

删除所有存档。

**返回值:**
- 是否删除成功

#### GetSaveInfo

```csharp
public static SaveFileInfo GetSaveInfo(string saveId)
```

获取存档文件信息。

**参数:**
- `saveId`: 存档ID

**返回值:**
- 存档文件信息

### 扩展方法

#### GetSavePath

```csharp
public static string GetSavePath()
```

获取当前存档路径。

**返回值:**
- 当前存档路径

## SaveFormat

`SaveFormat` 是一个枚举，定义了支持的存档格式。

```csharp
public enum SaveFormat
{
    Json,
    Binary
}
```

## SaveFileInfo

`SaveFileInfo` 类包含存档文件的元数据信息。

### 属性

- `SaveId`: 存档ID
- `CreationTime`: 创建时间
- `LastWriteTime`: 最后修改时间
- `SizeInBytes`: 文件大小（字节）
- `FormattedSize`: 格式化的文件大小（如 "1.5 KB"）
- `FormattedCreationTime`: 格式化的创建时间
- `FormattedLastWriteTime`: 格式化的最后修改时间

## ISaveSerializer

`ISaveSerializer` 是存档序列化器接口，定义了序列化和反序列化方法。

### 方法

#### Serialize

```csharp
string Serialize<T>(T data) where T : class;
```

序列化对象为字符串。

**参数:**
- `data`: 要序列化的对象

**返回值:**
- 序列化后的字符串

#### Deserialize

```csharp
T Deserialize<T>(string serializedData) where T : class;
```

反序列化字符串为对象。

**参数:**
- `serializedData`: 序列化的字符串

**返回值:**
- 反序列化后的对象

## JsonSaveSerializer

`JsonSaveSerializer` 是 `ISaveSerializer` 的实现，使用 Unity 的 JsonUtility 进行 JSON 序列化和反序列化。

## BinarySaveSerializer

`BinarySaveSerializer` 是 `ISaveSerializer` 的实现，使用 .NET 的 BinaryFormatter 进行二进制序列化和反序列化。

## EncryptionUtility

`EncryptionUtility` 是一个工具类，提供了加密和解密功能。

### 方法

#### Encrypt

```csharp
public static string Encrypt(string plainText, string password)
```

加密字符串。

**参数:**
- `plainText`: 明文
- `password`: 密码

**返回值:**
- 加密后的字符串

#### Decrypt

```csharp
public static string Decrypt(string encryptedText, string password)
```

解密字符串。

**参数:**
- `encryptedText`: 密文
- `password`: 密码

**返回值:**
- 解密后的字符串