# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 專案概述

**PinionCore.NetSync** 是基於 [PinionCore.Remote](https://github.com/jiowchern/PinionCore.Remote) 框架的 Unity 網路同步套件，提供 Remote Method Invocation (RMI) 和 Soul-Ghost 架構的客戶端-伺服器網路通訊。

### 技術棧
- **Unity 6000.2.9f1** (Unity 2022.2+ 相容)
- **.NET Standard 2.1**
- **C# Source Generators** (PinionCore.Remote.Tools.Protocol.Sources)
- **支援平台**: Standalone、WebGL (需自行實作 WebSocket)

## 儲存庫結構

```
PinionCore.NetSync/
├── PinionCore.NetSync.Develop/     # Unity 開發專案 (Assets、Scenes、ProjectSettings)
│   ├── Assets/
│   │   ├── PinionCore/
│   │   │   ├── Sample1/            # 基礎 TCP/WebSocket/Standalone 範例
│   │   │   └── Sample2-Chat/       # 聊天應用範例 (含 Gateway 模式)
│   │   ├── Scenes/                 # 測試場景
│   │   └── Settings/               # Render Pipeline、Build Profiles
│   └── ProjectSettings/
├── PinionCore.NetSync.Package/     # Unity 套件本體 (Submodule)
│   ├── Runtime/Scripts/
│   │   ├── Links/                  # Server/Client MonoBehaviour、連接器、監聽器
│   │   │   ├── Client.cs           # Unity 客戶端組件
│   │   │   ├── Server.cs           # Unity 伺服器組件
│   │   │   ├── Tcp/                # TCP 連接實作
│   │   │   ├── Web/                # WebSocket 連接實作 (僅伺服器端)
│   │   │   └── Standalone/         # 本地模擬模式
│   │   └── Syncs/                  # Soul-Ghost 同步系統
│   │       ├── Souls/              # 伺服器端權威物件 (Soul.cs, Transform.cs, TrackerSender.cs)
│   │       ├── Ghosts/             # 客戶端代理物件 (Ghost.cs, GhostMonoBehaviour.cs, TrackerReceiver.cs)
│   │       └── Protocols/          # 網路協議介面 (IObject, ITransform, ITracker)
│   └── Editor/Scripts/             # Unity Editor 擴充
└── PinionCore.Remote/              # 核心 RMI 框架 (Submodule)
    ├── PinionCore.Remote/          # 核心抽象 (IProtocol, IAgent, IBinder)
    ├── PinionCore.Remote.Server/   # 伺服器端實作
    ├── PinionCore.Remote.Client/   # 客戶端實作
    ├── PinionCore.Remote.Gateway/  # API 閘道服務
    └── PinionCore.Serialization/   # 序列化框架
```

## 常用指令

### Unity 開發
```bash
# 在 Unity Editor 中開啟專案
# File > Open Project > 選擇 PinionCore.NetSync.Develop/

# 執行範例場景
# 1. Sample1: 開啟 Assets/PinionCore/Sample1/Scenes/*.unity
# 2. Sample2-Chat: 開啟 Assets/Scenes/SampleScene.unity
```

### Submodule 管理
```bash
# 初始化並更新所有 submodule
git submodule update --init --recursive

# 更新 PinionCore.NetSync.Package
cd PinionCore.NetSync.Package
git checkout main
git pull origin main
cd ..
git add PinionCore.NetSync.Package
git commit -m "chore: update PinionCore.NetSync.Package submodule"

# 更新 PinionCore.Remote
cd PinionCore.Remote
git checkout master
git pull origin master
cd ..
git add PinionCore.Remote
git commit -m "chore: update PinionCore.Remote submodule"
```

### 建置與測試 (PinionCore.Remote)
```bash
# 切換到 PinionCore.Remote 子模組
cd PinionCore.Remote

# 還原依賴並建置
dotnet restore
dotnet build --configuration Release --no-restore

# 執行測試 (含覆蓋率)
dotnet test /p:CollectCoverage=true /p:CoverletOutput=../CoverageResults/ /p:MergeWith="../CoverageResults/coverage.json" /p:CoverletOutputFormat="lcov%2cjson" -m:1

# 打包 NuGet 套件
dotnet pack --configuration Release --output ./nupkgs
```

### 發佈 Unity 套件
```bash
# 切換到 Package submodule 並推送變更
cd PinionCore.NetSync.Package
git add .
git commit -m "feat: add new feature"
git push origin main
cd ..

# 更新主儲存庫的 submodule 參考
git add PinionCore.NetSync.Package
git commit -m "chore: update package submodule"
git push
```

## 核心架構

### 1. Soul-Ghost 同步模式

**Server Side (Soul - 權威物件)**:
```csharp
using PinionCore.NetSync.Syncs.Souls;

public class MySoul : Soul  // 繼承 Soul
{
    void Start()
    {
        // 伺服器邏輯 (權威狀態)
        // 自動同步到所有連接的客戶端
    }
}
```

**Client Side (Ghost - 代理物件)**:
```csharp
using PinionCore.NetSync.Syncs.Ghosts;

public class MyGhost : Ghost  // 繼承 Ghost
{
    void Update()
    {
        // 接收並渲染伺服器狀態
        // 透過 TrackerReceiver 處理位置插值
    }
}
```

**核心概念**:
- **Soul**: 伺服器端權威物件，實際執行遊戲邏輯
- **Ghost**: 客戶端代理物件，接收並顯示伺服器狀態
- **Tracker System**: 位置壓縮與軌跡插值，減少頻寬消耗
- **自動綁定**: Soul 透過 `IObject` 介面與 Ghost 自動配對

### 2. 連接架構

**伺服器設置**:
```csharp
using PinionCore.NetSync;

// 1. 添加 Server 組件 (MonoBehaviour)
var server = gameObject.AddComponent<Server>();

// 2. 添加監聽器 (TCP 或 WebSocket)
var listener = gameObject.AddComponent<Tcp.Listener>();
listener.Port = 7777;

// 3. 透過 BinderEvent 處理客戶端連接
server.BinderEvent.AddListener((command) =>
{
    if (command.Status == Server.BinderCommand.OperatorStatus.Add)
    {
        // 客戶端連接：綁定物件到 command.Binder
        var soul = Instantiate(soulPrefab).GetComponent<Soul>();
        command.Binder.Bind<IObject>(soul);
    }
    else
    {
        // 客戶端斷線：清理資源
    }
});
```

**客戶端設置**:
```csharp
using PinionCore.NetSync;

// 1. 添加 Client 組件 (MonoBehaviour)
var client = gameObject.AddComponent<Client>();

// 2. 添加連接器 (TCP 或 WebSocket)
var connector = gameObject.AddComponent<Tcp.Connector>();
connector.Host = "127.0.0.1";
connector.Port = 7777;

// 3. 透過 INotifierQueryable 監聽遠端物件
client.Queryer.QueryNotifier<IObject>().Supply += (obj) =>
{
    // 伺服器發送物件：實例化 Ghost
    var ghost = Instantiate(ghostPrefab).GetComponent<Ghost>();
    ghost.Bind(obj);
};
```

### 3. 傳輸層抽象

| 傳輸層 | 組件 | 適用場景 |
|-------|-----|---------|
| **TCP** | `Tcp.Listener`, `Tcp.Connector` | 可靠、有序傳輸 (預設) |
| **WebSocket** | `Web.Listener`, WebGL 瀏覽器內建 | WebGL 平台、穿越防火牆 |
| **Standalone** | `Standalone.Listener`, `Standalone.Connector` | 本地模擬、單元測試 |

**選擇邏輯** (參考 Sample2-Chat/Client.cs):
```csharp
if (Application.platform == RuntimePlatform.WebGLPlayer && !Application.isEditor)
{
    // WebGL 平台使用 WebSocket
    var state = new WebSocketState(endpoint);
}
else
{
    // 其他平台使用 TCP
    var state = new TcpSocketState(endpoint);
}
```

### 4. Protocol 與 Source Generator

**Protocol 定義** (參考 PinionCore.Remote 框架):
```csharp
// 定義共享介面
public interface IGreeter
{
    PinionCore.Remote.Value<string> SayHello(string request);
}

// 使用 Source Generator 生成協議
public static partial class ProtocolCreator
{
    public static PinionCore.Remote.IProtocol Create()
    {
        PinionCore.Remote.IProtocol protocol = null;
        _Create(ref protocol);
        return protocol;
    }

    [PinionCore.Remote.Protocol.Creator]  // 觸發 Source Generator
    static partial void _Create(ref PinionCore.Remote.IProtocol protocol);
}
```

**Unity 中使用**:
```csharp
// Server/Client 自動使用 ProtocolCreator.Create()
// 參考 Links/ProtocolCreator.cs
```

### 5. Gateway 模式 (可選)

**用途**: 多服務分佈式架構、負載平衡、協議版本管理

**客戶端啟用** (Sample2-Chat/Client.cs:104-112):
```csharp
IAgent agent;
if (useGateway)
{
    var pool = new PinionCore.Remote.Gateway.Hosts.AgentPool(protocol);
    agent = new PinionCore.Remote.Gateway.Agent(pool);
}
else
{
    agent = PinionCore.Remote.Client.Provider.CreateAgent(protocol);
}
```

### 6. StatusMachine 狀態管理模式

**重要**: 範例代碼 (Sample2-Chat) 大量使用 `PinionCore.Utility.StatusMachine` 管理連接狀態。

**核心概念**:
- **IStatus 介面**: 定義狀態生命週期 (`Enter()`, `Update()`, `Leave()`)
- **StatusMachine**: 管理狀態佇列與轉換 (`Push()`, `Update()`, `Termination()`)
- **事件驅動**: 透過事件觸發狀態轉換，避免大量 enum/switch 判斷

**範例** (Client.cs):
```csharp
public class Client : MonoBehaviour, IStatus
{
    readonly StatusMachine _Machine;

    private void Start()
    {
        _Machine.Push(this);  // 推送初始狀態 (IConnect)
    }

    private void Update()
    {
        _Machine.Update();  // 驅動狀態機 (處理轉換與更新)
    }

    void IConnect.Connect(string endpoint, bool gate)
    {
        var connectingState = new TcpSocketState(endpoint);
        connectingState.SuccessEvent += (stream) =>
        {
            _ToSetupMode(stream, gate);  // 成功後轉換到下一狀態
        };
        _Machine.Push(connectingState);  // 推送連接狀態
    }
}
```

**參考範例**:
- `Sample2-Chat/Client.cs`: 連接 -> TCP/WebSocket -> 遊戲循環
- `Sample2-Chat/TcpSocketState.cs`, `WebSocketState.cs`: 連接狀態實作
- `Sample2-Chat/LoopState.cs`: 遊戲循環狀態

**詳細說明**: 參閱 `PinionCore.Remote/CLAUDE.md` 的 StatusMachine 章節

### 7. 位置追蹤與壓縮

**Tracker 系統**:
- **TrackerSender** (Souls): 伺服器端發送壓縮的軌跡資料
- **TrackerReceiver** (Ghosts): 客戶端接收並插值位置
- **ZipTracker**: 壓縮演算法 (Step、FinalState、ZipPosition)

**使用方式** (自動處理):
```csharp
// Soul 端
public class MySoul : Soul
{
    // TrackerSender 自動附加，發送 Transform 變化
}

// Ghost 端
public class MyGhost : Ghost
{
    // TrackerReceiver 自動接收並插值
}
```

## 開發流程

### 新增網路同步物件

1. **定義協議介面** (如需自訂，否則使用內建 `IObject`):
   ```csharp
   public interface IMyObject : IObject
   {
       // 自訂屬性或方法
   }
   ```

2. **建立 Soul 類別** (伺服器端):
   ```csharp
   using PinionCore.NetSync.Syncs.Souls;

   public class MySoul : Soul
   {
       void Start()
       {
           // 伺服器邏輯
       }
   }
   ```

3. **建立 Ghost 類別** (客戶端):
   ```csharp
   using PinionCore.NetSync.Syncs.Ghosts;

   public class MyGhost : Ghost
   {
       void Update()
       {
           // 客戶端渲染
       }
   }
   ```

4. **在 Server 端綁定**:
   ```csharp
   server.BinderEvent.AddListener((command) =>
   {
       if (command.Status == Add)
       {
           var soul = Instantiate(soulPrefab).GetComponent<MySoul>();
           command.Binder.Bind<IMyObject>(soul);
       }
   });
   ```

5. **在 Client 端監聽**:
   ```csharp
   client.Queryer.QueryNotifier<IMyObject>().Supply += (obj) =>
   {
       var ghost = Instantiate(ghostPrefab).GetComponent<MyGhost>();
       ghost.Bind(obj);
   };
   ```

### 測試工作流程

1. **Standalone 模式測試** (無需網路):
   ```csharp
   // 使用 Standalone.Listener 和 Standalone.Connector
   // 在單一 Unity 場景中測試 Server/Client 邏輯
   ```

2. **本地網路測試**:
   - 建置兩個場景：Server 場景 + Client 場景
   - 或使用 ParrelSync 克隆編輯器

3. **WebGL 測試**:
   - 建置 WebGL 版本
   - 確保使用 WebSocket 傳輸 (Application.platform == RuntimePlatform.WebGLPlayer)
   - 伺服器端需實作 WebSocket 監聽器

### Submodule 開發流程

**修改 PinionCore.NetSync.Package**:
```bash
# 1. 進入 submodule
cd PinionCore.NetSync.Package

# 2. 確保在正確分支
git checkout main

# 3. 修改程式碼並提交
git add .
git commit -m "feat: add new transport layer"
git push origin main

# 4. 回到主儲存庫並更新參考
cd ..
git add PinionCore.NetSync.Package
git commit -m "chore: update Package submodule to include new transport"
git push
```

**修改 PinionCore.Remote**:
- 同上，但分支為 `master`
- 通常不需修改，除非需要核心功能變更

## 重要注意事項

### Unity WebGL 限制
- **必須使用 WebSocket**: TCP 不支援，需在 `IConnect.Connect()` 中判斷平台
- **客戶端 WebSocket**: Unity WebGL 使用瀏覽器內建 WebSocket API (非 C# Socket)
- **伺服器端**: 需實作 `Web.Listener` 或使用 Gateway

### Submodule 狀態
- **提交前檢查**: `git status` 應顯示 submodule 在正確的 commit
- **推送順序**: 先推送 submodule 變更，再推送主儲存庫
- **協作**: 其他開發者需執行 `git submodule update` 同步

### StatusMachine 使用
- **資源管理**: 在 `IStatus.Enter()` 註冊資源，`Leave()` 取消註冊
- **避免記憶體洩漏**: 確保事件處理器在 `Leave()` 中取消訂閱
- **適用場景**: 凡是要用 enum 控制程序走向的代碼都應使用此模式

### 網路斷線檢測
- ❌ **錯誤**: 使用 `Agent.Ping` 輪詢檢測斷線 (延遲、不準確、浪費資源)
- ✅ **正確**: 訂閱 `Peer.SocketErrorEvent` 事件 (即時、準確、零開銷)
- **實作**: 在 `ConnectedState.Enter()` 訂閱，`Leave()` 取消訂閱

### 資源釋放模式
- 使用 `_Dispose` 閉包模式處理延遲初始化資源
- 在 `Start()` 中設置 `_Dispose = () => { /* cleanup */ }`
- 在 `Dispose()` / `OnDestroy()` 中呼叫 `_Dispose()`

### 禁止使用 static class
- 這是網路通訊框架，嚴禁使用 `static class` (除了工具類如 `ProtocolCreator`)
- 所有狀態應封裝在實例中，便於測試與多實例場景

### 檔案路徑規範
- **Windows 絕對路徑**: 所有檔案操作必須使用完整的 Windows 路徑格式
- 例如: `D:\develop\PinionCore.NetSync\...` (包含磁碟機代號與反斜線)

## 範例專案參考

### Sample1 (基礎範例)
- **位置**: `Assets/PinionCore/Sample1/`
- **場景**: `Client.unity`, `Server.unity`, `Main.unity`
- **功能**: TCP、WebSocket、Standalone 三種傳輸模式的基本連接測試

### Sample2-Chat (進階範例)
- **位置**: `Assets/PinionCore/Sample2-Chat/`
- **場景**: `Assets/Scenes/SampleScene.unity`
- **功能**:
  - StatusMachine 狀態管理
  - Gateway 模式切換
  - TCP/WebSocket 動態選擇
  - 聊天室 (ILogin, IPlayer, IChatter)
- **關鍵檔案**:
  - `Client.cs`: 主控制器 (IConnect, IStatus)
  - `Controller.cs`: UI 邏輯 (Unity Events)
  - `LoopState.cs`: 遊戲循環狀態
  - `TcpSocketState.cs`, `WebSocketState.cs`: 連接狀態

## 相關資源

- **PinionCore.Remote 文檔**: [README](PinionCore.Remote/README.md)
- **PinionCore.Remote 開發指南**: [CLAUDE.md](PinionCore.Remote/CLAUDE.md)
- **Package README**: [PinionCore.NetSync.Package/README.md](PinionCore.NetSync.Package/README.md)
- **主儲存庫**: https://github.com/jiowchern/PinionCore.NetSync
- **套件儲存庫**: https://github.com/jiowchern/PinionCore.NetSync.Package
- **核心框架**: https://github.com/jiowchern/PinionCore.Remote
