# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 專案概述

PinionCore NetSync 是一個基於 [PinionCore.Remote](https://github.com/jiowchern/PinionCore.Remote) 的 Unity 網路同步套件,實現了 Remote Method Invocation (RMI) 模式。這是一個 Unity Package Manager (UPM) 套件,提供客戶端-伺服器架構的網路同步功能。

**核心技術棧:**
- Unity 2022.2+
- .NET/C#
- PinionCore.Remote 框架 (通過預編譯 DLL 整合)
- 支援多種網路傳輸層: TCP、WebSocket、Standalone (本地模式)

## 架構設計

### Soul-Ghost 模式 (核心概念)

這個專案使用獨特的 **Soul-Ghost** 架構模式來實現網路物件同步:

- **Soul (靈魂)**: 伺服器端的權威物件,擁有完整的遊戲邏輯和狀態
  - 位於 `Runtime/Scripts/Syncs/Souls/`
  - `Soul.cs` - 伺服器端物件基類,通過 GameObject.GetInstanceID() 生成唯一 ID
  - `TrackerSender.cs` - 負責發送位置追蹤資料到客戶端
  - `Transform.cs` - 同步 Unity Transform 組件
  - `Viewport.cs` - 管理可見範圍,決定哪些物件需要同步給哪些客戶端

- **Ghost (幽靈)**: 客戶端的代理物件,接收並呈現伺服器狀態
  - 位於 `Runtime/Scripts/Syncs/Ghosts/`
  - `Ghost.cs` - 客戶端代理物件,通過 INotifier<T> 監聽伺服器物件
  - `TrackerReceiver.cs` - 接收並應用位置追蹤資料
  - `Transform.cs` - 應用同步的 Transform 資料
  - `GhostProvider.cs` - 自動創建/銷毀 Ghost 實例

**工作流程:**
1. 伺服器上的 `Soul` 物件狀態變化
2. `TrackerSender` 壓縮並發送變化資料
3. 網路層傳輸 (TCP/WebSocket/Standalone)
4. 客戶端的 `Ghost` 通過 `TrackerReceiver` 接收並重建狀態

### 網路層架構

**分層設計** (位於 `Runtime/Scripts/Links/`):
1. **傳輸層抽象** - `IStreamable` 介面定義統一的網路傳輸接口
2. **實現層** - 三種傳輸實現:
   - `Tcp/` - TCP 連接實現
   - `Web/` - WebSocket 實現
   - `Standalone/` - 本地內存傳輸 (用於單機或測試)
3. **管理層**:
   - `Client.cs` - 客戶端管理器,封裝 PinionCore.Remote.Ghost.IAgent
   - `Server.cs` - 伺服器管理器,使用 PinionCore.Remote.Soul.SyncService

### Protocol 協議生成

- `ProtocolCreator.cs` 使用 **partial method** 模式
- 協議由 `[PinionCore.Remote.Protocol.Creater]` 屬性標記的方法在編譯時生成
- 通過 `IProtocol.VersionCode` 確保客戶端-伺服器版本一致性

### 位置追蹤壓縮系統

**Tracker 系統** (位於 `Runtime/Scripts/Syncs/Protocols/Trackers/`):
- `Tracker.cs` - 基於時間的位置插值追蹤器
- `ZipTracker.cs` / `ZipPosition.cs` - 位置資料壓縮,將 float 轉換為整數範圍以減少頻寬
- `Step.cs` - 記錄位置點和重複次數
- `FinalState.cs` - 定義追蹤結束行為 (Continue/Stop)

## 常用開發命令

### 構建

**注意**: 直接使用 `dotnet build` 會失敗,因為這個專案依賴 Unity 生成的預編譯 DLL (位於 `Runtime/Plugins/`)。

正確的構建流程:
1. 在 Unity Editor 中打開專案
2. Unity 會自動編譯 C# 腳本
3. 預編譯的 PinionCore.* DLL 必須存在於 `Runtime/Plugins/` 和 `Editor/Plugins/`

### 測試

在 Unity Test Runner 中執行:
- **測試位置**: `Packages/com.pinioncore.netsync/Tests/`
- `TrackerTests.cs` - 位置追蹤和壓縮算法測試
- `NetworkTests.cs` - 網路功能測試

使用 Unity Test Runner:
1. Window > General > Test Runner
2. 選擇 EditMode 或 PlayMode
3. 執行測試

## 關鍵檔案位置

```
Packages/com.pinioncore.netsync/
├── Runtime/
│   ├── Scripts/
│   │   ├── Links/              # 網路層實現
│   │   │   ├── Client.cs       # 客戶端核心
│   │   │   ├── Server.cs       # 伺服器核心
│   │   │   ├── ProtocolCreator.cs  # 協議生成入口
│   │   │   ├── Tcp/            # TCP 傳輸
│   │   │   ├── Web/            # WebSocket 傳輸
│   │   │   └── Standalone/     # 本地傳輸
│   │   └── Syncs/              # 同步系統
│   │       ├── Souls/          # 伺服器端物件
│   │       ├── Ghosts/         # 客戶端代理物件
│   │       └── Protocols/      # 同步協議定義
│   └── Plugins/                # PinionCore.Remote 預編譯 DLL
├── Editor/
│   ├── Scripts/                # Unity Editor 擴展
│   └── Plugins/                # Editor 專用 DLL
└── Tests/                      # 單元測試
```

## 擴展開發指南

### 添加新的網路傳輸層

1. 在 `Runtime/Scripts/Links/` 創建新資料夾
2. 實現 `IStreamable` 介面:
   - `IAwaitableSource<int> Send(byte[], int, int)`
   - `IAwaitableSource<int> Receive(byte[], int, int)`
3. 實現 `IListenable` 介面處理連接事件
4. 創建對應的 `Connector` 和 `Listener` 類別
5. 在 `Editor/Scripts/` 添加自定義 Inspector

### 添加新的同步協議

1. 在 `Runtime/Scripts/Syncs/Protocols/` 定義新介面 (繼承 `IObject`)
2. 在 Soul 端實現該介面並通過 `IBinderProvider` 註冊
3. 在 Ghost 端使用 `INotifier<T>` 訂閱該介面
4. 重新生成 Protocol (需要重新在 Unity 中編譯)

### User 管理系統

- `User.cs` - 代表一個連接的玩家
- `UserProvider.cs` - 管理 User 生命週期
- Soul 物件可以通過 `UserEnter(User)` / `UserLeave(User)` 接收玩家連接事件

## 重要注意事項

1. **DLL 依賴**: 所有 `Runtime/Plugins/` 中的 DLL 是必需的,缺少任何一個會導致編譯錯誤
2. **協議版本**: Client 和 Server 必須使用相同的 Protocol.VersionCode,否則連接會失敗
3. **Unity 限制**: 這是 Unity Package,必須在 Unity 環境中使用,不能作為獨立 .NET 專案構建
4. **MonoBehaviour 生命週期**: Client/Server 都繼承 MonoBehaviour,需要手動在 Update() 中調用其 Update() 方法
5. **日誌系統**: 使用 `Client.EnableLog` / `Server.EnableLog` 啟用 PinionCore 內部日誌

## Git 分支策略

- **main**: 主分支,穩定版本
- 最近提交顯示正在修復網路傳輸問題 ("fixed packet transfer error")
