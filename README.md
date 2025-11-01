# PinionCore.NetSync

> Unity 網路同步套件開發專案 - 基於 [PinionCore.Remote](https://github.com/jiowchern/PinionCore.Remote) 框架實現的 RMI 與 Soul-Ghost 架構

[![Unity Version](https://img.shields.io/badge/Unity-2022.2%2B-blue)](https://unity.com/)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-purple)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## 📦 使用套件

**如果你是 Unity 開發者，想要在專案中使用這個套件：**

👉 **請前往 [PinionCore.NetSync.Package](https://github.com/jiowchern/PinionCore.NetSync.Package)** 查看完整的安裝與使用說明

或直接透過 Unity Package Manager 安裝：
```
https://github.com/jiowchern/PinionCore.NetSync.Package.git
```

---

## 🎮 線上 Demo

**立即體驗 PinionCore.NetSync 的即時多人聊天範例：**

### **👉 [https://proxy.pinioncore.dpdns.org/sample2](https://proxy.pinioncore.dpdns.org/sample2)**

#### 使用說明
1. 開啟 Demo 網址
2. **點擊 "Connect to pinioncore.dpdns.org" 按鈕進行連接**
3. 輸入您的暱稱
4. 開始體驗即時多人聊天功能

#### Demo 展示特性
- ✅ **WebGL 平台** 的 WebSocket 連接
- ✅ **即時多人同步**（多玩家聊天）
- ✅ **Remote Method Invocation (RMI)** 遠端方法呼叫
- ✅ **Soul-Ghost 架構** 的網路同步

---

## 🏗️ 專案說明

這是 **PinionCore.NetSync 的開發專案儲存庫**，包含：

- 🔧 **Unity 開發專案** (`PinionCore.NetSync.Develop/`)
- 📦 **套件本體** (Submodule: `PinionCore.NetSync.Package/`)
- 🧪 **範例專案** (`Sample1`, `Sample2-Chat`)
- 📚 **開發文檔** (`CLAUDE.md`)

### 儲存庫結構

```
PinionCore.NetSync/
├── PinionCore.NetSync.Develop/     # Unity 開發專案
│   ├── Assets/
│   │   ├── PinionCore/
│   │   │   ├── Sample1/            # 基礎範例
│   │   │   └── Sample2-Chat/       # 聊天應用範例
│   │   └── Scenes/
│   └── ProjectSettings/
├── PinionCore.NetSync.Package/     # Unity 套件本體 (Submodule)
│   ├── Runtime/
│   │   ├── Links/                  # Server/Client 組件
│   │   └── Syncs/                  # Soul-Ghost 同步系統
│   └── Editor/
└── PinionCore.Remote/              # 核心 RMI 框架 (Submodule)
```

---

## 🚀 開發環境設定

### 克隆儲存庫

```bash
# 克隆主儲存庫（包含所有子模組）
git clone --recursive https://github.com/jiowchern/PinionCore.NetSync.git
cd PinionCore.NetSync

# 如果已經克隆但沒有子模組
git submodule update --init --recursive
```

### 開啟 Unity 專案

1. 開啟 Unity Hub
2. **Add** → 選擇 `PinionCore.NetSync.Develop/` 資料夾
3. 使用 **Unity 6000.2.9f1**（或 Unity 2022.2+）開啟

### 執行範例場景

#### Sample1（基礎範例）
- **位置**: `Assets/PinionCore/Sample1/Scenes/`
- **場景**: `Client.unity`, `Server.unity`, `Main.unity`
- **功能**: TCP、WebSocket、Standalone 三種傳輸模式測試

#### Sample2-Chat（進階範例）
- **位置**: `Assets/Scenes/SampleScene.unity`
- **功能**: 多人聊天室、StatusMachine 狀態管理、Gateway 模式

---

## 🔧 Submodule 管理

### 更新 Package Submodule

```bash
# 進入 Package submodule
cd PinionCore.NetSync.Package

# 確保在 main 分支
git checkout main
git pull origin main

# 回到主儲存庫並更新參考
cd ..
git add PinionCore.NetSync.Package
git commit -m "chore: update Package submodule"
git push
```

### 更新 PinionCore.Remote Submodule

```bash
cd PinionCore.Remote
git checkout master
git pull origin master
cd ..
git add PinionCore.Remote
git commit -m "chore: update PinionCore.Remote submodule"
git push
```

---

## 📚 範例專案

### Sample1 - 基礎範例

**位置**: `Assets/PinionCore/Sample1/`

**功能**:
- ✅ TCP 連接測試
- ✅ WebSocket 連接測試
- ✅ Standalone 本地模擬
- ✅ 簡單的 Soul-Ghost 同步示範

**適合**:
- 初次使用者
- 測試不同傳輸層
- 理解基本架構

### Sample2-Chat - 進階範例

**位置**: `Assets/PinionCore/Sample2-Chat/`

**功能**:
- ✅ StatusMachine 狀態管理
- ✅ Gateway 模式切換
- ✅ TCP/WebSocket 動態選擇
- ✅ 多人聊天室（ILogin, IPlayer, IChatter）
- ✅ WebGL 建置支援

**關鍵檔案**:
- `Client.cs`: 主控制器（IConnect, IStatus）
- `Controller.cs`: UI 邏輯（Unity Events）
- `LoopState.cs`: 遊戲循環狀態
- `TcpSocketState.cs`, `WebSocketState.cs`: 連接狀態實作

**適合**:
- 實際專案參考
- StatusMachine 模式學習
- WebGL 部署參考

---

## 📖 開發文檔

- 📘 [CLAUDE.md](CLAUDE.md) - **完整開發指南**（架構、工作流程、最佳實踐）
- 📗 [Package README](PinionCore.NetSync.Package/README.md) - 套件使用說明
- 📙 [PinionCore.Remote 文檔](PinionCore.Remote/README.md) - 核心 RMI 框架
- 📕 [PinionCore.Remote 開發指南](PinionCore.Remote/CLAUDE.md) - 框架開發文檔

---

## 🤝 貢獻指南

歡迎提交 Issue 或 Pull Request！

### 開發流程

1. **Fork 主儲存庫**
2. **建立功能分支** (`git checkout -b feature/amazing-feature`)
3. **提交變更** (`git commit -m 'feat: add amazing feature'`)
4. **推送到分支** (`git push origin feature/amazing-feature`)
5. **建立 Pull Request**

### Submodule 開發

**修改 Package**:
```bash
cd PinionCore.NetSync.Package
git checkout main
# ... 修改程式碼 ...
git add .
git commit -m "feat: add new feature"
git push origin main
cd ..
git add PinionCore.NetSync.Package
git commit -m "chore: update Package submodule"
```

**修改 PinionCore.Remote**:
- 通常不需修改，除非需要核心功能變更
- 分支為 `master`

---

## 🛠️ 技術規格

- **Unity 版本**: Unity 6000.2.9f1（Unity 2022.2+ 相容）
- **.NET 標準**: .NET Standard 2.1
- **程式碼生成**: C# Source Generators（PinionCore.Remote.Tools.Protocol.Sources）
- **支援平台**: Windows, macOS, Linux, WebGL

---

## 🔗 相關連結

- 📦 [套件儲存庫](https://github.com/jiowchern/PinionCore.NetSync.Package) - **Unity Package Manager 套件**
- 🏗️ [核心框架](https://github.com/jiowchern/PinionCore.Remote) - PinionCore.Remote RMI 框架
- 🎮 [線上 Demo](https://proxy.pinioncore.dpdns.org/sample2) - WebGL 多人聊天範例

---

## 📝 授權

本專案採用 [MIT License](LICENSE) 授權。

---

## 🙏 致謝

- 基於 [PinionCore.Remote](https://github.com/jiowchern/PinionCore.Remote) 框架開發
- 使用 C# Source Generators 自動生成網路協議
