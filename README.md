# PinionCore NetSync - Development Repository

**Unity 網路同步套件開發環境**

這是 [PinionCore.NetSync.Package](https://github.com/jiowchern/PinionCore.NetSync.Package) Unity Package 的開發 Repository，採用模組化架構設計。

## 📦 Repository 架構

```
PinionCore.NetSync/
├── PinionCore.NetSync.Develop/     # Unity 開發專案（測試與範例）
└── PinionCore.NetSync.Package/     # Unity Package (Git Submodule)
```

### PinionCore.NetSync.Package (Submodule)

獨立的 Unity Package Repository，通過 Git Submodule 管理。

- **GitHub**: https://github.com/jiowchern/PinionCore.NetSync.Package
- **用途**: 實際發布的 Unity Package
- **特點**: 可通過 Git URL 直接安裝到任何 Unity 專案

### PinionCore.NetSync.Develop

Unity 開發測試專案，用於：
- 測試 Package 功能
- 開發範例場景
- 整合測試

## 🚀 開發者快速開始

### 克隆此開發環境

```bash
git clone --recurse-submodules https://github.com/jiowchern/PinionCore.NetSync.git
cd PinionCore.NetSync
```

如果已經克隆但忘記 `--recurse-submodules`:

```bash
git submodule update --init --recursive
```

### 在 Unity 中打開開發專案

1. 打開 Unity Hub
2. 添加專案：`PinionCore.NetSync/PinionCore.NetSync.Develop`
3. Unity 會自動載入 Package (通過 `file://` 引用)

## 💻 開發工作流程

### 修改 Package 程式碼

```bash
cd PinionCore.NetSync.Package

# 編輯 Runtime/, Editor/, Tests/ 中的程式碼
# Unity 會即時反映變更

# 提交變更
git add .
git commit -m "feat: add new feature"
git push origin main

# 發布新版本
git tag v1.0.0
git push origin v1.0.0
```

### 更新主 Repository

```bash
cd PinionCore.NetSync

# 更新 Submodule 引用到最新提交
git add PinionCore.NetSync.Package
git commit -m "Update package to latest version"
git push
```

## 📚 使用 Package (給其他開發者)

這個 Repository 是**開發環境**。如果你只想使用 Package，請參考以下方式：

### 通過 Git URL 安裝

在你的 Unity 專案 `Packages/manifest.json` 中添加：

```json
{
  "dependencies": {
    "com.pinioncore.netsync": "https://github.com/jiowchern/PinionCore.NetSync.Package.git"
  }
}
```

### 安裝特定版本

```json
{
  "dependencies": {
    "com.pinioncore.netsync": "https://github.com/jiowchern/PinionCore.NetSync.Package.git#v0.0.1"
  }
}
```

### 通過 Unity Package Manager UI

1. Window > Package Manager
2. 點擊 "+" > "Add package from git URL..."
3. 輸入: `https://github.com/jiowchern/PinionCore.NetSync.Package.git`

## 🌟 擴展新 Package

此架構支持多 Package 開發。要添加新 Package:

```bash
cd PinionCore.NetSync

# 添加新的 Submodule
git submodule add https://github.com/你的名字/PinionCore.NewPackage.git

# 在開發專案的 manifest.json 中引用
cd PinionCore.NetSync.Develop/Packages
# 編輯 manifest.json:
{
  "dependencies": {
    "com.pinioncore.netsync": "file:../../PinionCore.NetSync.Package",
    "com.pinioncore.newpackage": "file:../../PinionCore.NewPackage"
  }
}
```

## 📖 詳細文檔

- **Package 使用說明**: [PinionCore.NetSync.Package](https://github.com/jiowchern/PinionCore.NetSync.Package)
- **開發指南**: [CLAUDE.md](CLAUDE.md)

## 📄 授權

MIT License - 詳見 [LICENSE](LICENSE)

## 🔗 相關連結

- [PinionCore.Remote](https://github.com/jiowchern/PinionCore.Remote) - 核心 RMI 框架
- [Package Repository](https://github.com/jiowchern/PinionCore.NetSync.Package) - 獨立 Package
