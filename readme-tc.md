# PinionCore NetSync 套件

PinionCore NetSync 是一套以 Unity 為核心的網路同步框架，能在多種傳輸技術之間同步遊戲狀態。本儲存庫整合了可發佈的執行時套件、示範與開發用的 Unity 專案，以及提供 TCP 與 WebSocket 連線能力的傳輸層子模組。

## 儲存庫結構

| 路徑 | 說明 |
| --- | --- |
| `PinionCore.NetSync.Develop/` | 功能開發、驗證與示範用的 Unity 專案，範例場景位於 `Assets/PinionCore`。 |
| `PinionCore.NetSync.Package/` | 透過 Unity Package Manager 發佈的 NetSync 套件，包含執行時、編輯器工具、分析器、範例與 NUnit 測試。 |
| `PinionCore.Remote/` | Git 子模組，提供底層傳輸層 (TCP、WebSocket、記憶體池)；NetSync 需與此版本保持一致。 |
| `Publishs/` | 封裝後可直接發布或供 CI 驗證的套件資產。 |
| `notes/` | 開發筆記與內部研究資料。 |

## 快速開始

1. 下載專案並初始化子模組：

   ```bash
   git submodule update --init --recursive
   ```

2. 使用 Unity 2022.2 以上版本。透過 Unity Hub 開啟 `PinionCore.NetSync.Develop/PinionCore.NetSync.Develop.sln`，或直接指定該專案資料夾。
3. 專案內含專用的開發組件定義 (`PinionCore.NetSync.Develop`)，可直接引用套件以加速迭代。

## 示範場景

- **Sample 1 (`Assets/PinionCore/Sample1`)** 示範如何連接 `Server`、`Client` 與 Standalone、TCP、WebSocket 各種傳輸連接器。可開啟 `Main` 場景切換協定，或分別開啟 `Client`／`Server` 專注於單方流程。
- **Sample 2 – Chat (`Assets/PinionCore/Sample2-Chat`)** 展示結合協定切換與 UI 回饋的遊戲化流程。

每個範例場景均附有已設定好的元件 Prefab，進入 Play Mode 前請依實際端點調整 Inspector 屬性。

## 套件開發重點

`PinionCore.NetSync.Package` 目錄與發佈版本一致：

- `Runtime/Scripts/Links`：傳輸層適配器。
  - `Standalone/` 提供編輯器內的 loopback 連線。
  - `Tcp/` 與 `Web/` 透過 `PinionCore.Remote` 連接 TCP 與 WebSocket。
  - `Client`、`Server`、`Linstener`、`ProtocolCreator` 管理協定生命週期與 Binder 註冊。
- `Runtime/Scripts/Syncs`：狀態複寫管線。
  - `Souls/` 建立權威端狀態與 Binder 生命週期。
  - `Ghosts/` 處理客戶端資料並提供 notifier 實用工具。
  - `Protocols/Trackers` 提供插值、壓縮與 Zip 編碼。
- `Editor/Scripts`：顯示執行時量測的 Inspector 擴充。
- `Tests/`：針對 Tracker 與網路原件的 NUnit 測試。
- `Analyzers/`：CI 使用的 Roslyn 分析器。

## 建置與測試

使用 Unity Batch 模式進行可重複建置：

```powershell
"<UnityEditorPath>\Unity.exe" -projectPath PinionCore.NetSync.Develop -quit -batchmode -logFile Logs/ci.log
```

可在編輯器或指令列執行 Edit Mode 測試：

```powershell
"<UnityEditorPath>\Unity.exe" -projectPath PinionCore.NetSync.Develop -quit -batchmode -runTests -testPlatform EditMode -testResults Logs/editmode.xml
```

必要時再加入 `-testPlatform PlayMode`。匯入套件後，`PinionCore.NetSync.Package/Tests` 內的 NUnit 測試也會一併執行。

## 發佈流程

1. 以 `git submodule update --remote --merge` 更新 `PinionCore.Remote`，並確認新的 SHA。
2. 檢查 `Packages/com.pinioncore.netsync/package.json` 是否仍指向正確版本。
3. 使用 Unity 套件匯出工具重新產生 `Publishs/` 內的發佈檔。
4. 將使用者能感受到的變更紀錄在 `PinionCore.NetSync.Package/CHANGELOG.md`。

## 程式規範與貢獻

- 採用 C# 9、四個空白縮排，除非原本範圍已使用 `var`，否則優先使用明確型別。
- 盡量維持 asmdef 相依最小化，提交前請執行 `PinionCore.NetSync.Package/Analyzers`。
- 遵循 Conventional Commits（例如 `feat: add web reconnect tracker`）。
- 當變更 Unity 視覺資產時，請附上對應截圖或錄影。

## 授權

PinionCore NetSync 依附帶的 `LICENSE` 授權釋出。
