# Repository Guidelines

## 專案結構與模組組織
- `PinionCore.NetSync.Develop/` 為主要 Unity 工程，所有場景、Prefab 與示範邏輯請放在 `Assets/PinionCore` 及對應資料夾。
- `PinionCore.NetSync.Package/Runtime/Scripts` 提供封裝後的同步核心；`Editor/` 內含客製化 Inspector，`Samples/` 與 `Publishs/` 用於發佈範例。
- `PinionCore.NetSync.Package/Tests` 儲存 NUnit 測試；`PinionCore.Remote/` 子模組提供傳輸層，必須與封裝版保持相同版本。

## 建置、測試與開發命令
- 進入專案前先更新子模組：
  ```
  git submodule update --init --recursive
  ```
- 編輯器內可直接透過 Unity Hub 開啟 `PinionCore.NetSync.Develop/PinionCore.NetSync.Develop.sln` 進行腳本調試。
- Batch 模式建置：
  ```
  "<UnityEditorPath>\Unity.exe" -projectPath PinionCore.NetSync.Develop -quit -batchmode -logFile Logs/ci.log
  ```
- 執行編輯器測試需加上 `-runTests -testPlatform EditMode -testResults Logs/editmode.xml`，必要時再附加 `-testPlatform PlayMode`。

## 程式風格與命名慣例
- C# 腳本採四空白縮排、UTF-8 編碼與 C# 9 語言功能，避免混用 `var` 與明確型別。
- 公開成員使用 PascalCase，私有欄位延續現有底線命名（例如 `_binderQueue`）；Unity 屬性與欄位特性分行放置。
- 提交前執行 Analyzer（`PinionCore.NetSync.Package/Analyzers`）確保無警告，並保持 `asmdef` 依賴最小化。

## 測試指引
- 測試類別命名為 `<Feature>Tests`，方法採 `<Scenario>_<Expectation>()` 格式，覆蓋 TCP 與 Web 兩種傳輸流程。
- 測試中建立的 Listener、Connector 需在 `TearDown` 中釋放；記錄輸出集中於 `Logs/` 方便 CI 解析。
- 對於網路協定更新，新增整合測試模擬雙端事件流，並於 PR 描述中貼上結果摘要。

## 提交與 PR 準則
- 依循現有歷史使用 Conventional Commits（例如 `feat: add web reconnect tracker`、`fix: handle tcp peer break`）。
- PR 描述需包含：變更動機、測試或驗證步驟、影響範圍以及相關 issue/任務連結；修改 Unity 視覺資源時補充截圖或錄影。
- 如變更子模組版本，分開提交：一個 commit 調整子模組指標，另一個 commit 更新對應文件或組態。

## 子模組與設定維護
- 子模組更新建議使用 `git submodule update --remote --merge` 檢閱 upstream 變化，再以 `git status` 核對指向的 SHA。
- 更新後確認 `Packages/com.pinioncore.netsync` 內的 `package.json` 與 `.asmdef` 仍引用正確版本，並同步刷新 `Publishs/` 內測試封裝。
- 不直接編輯 `Runtime/Plugins` 下的 DLL；若需修改，請於對應原始庫提出 PR 並重新打包。
