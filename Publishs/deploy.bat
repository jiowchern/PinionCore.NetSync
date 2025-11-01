@echo off
REM PinionCore.NetSync - 一鍵部署腳本 (Windows)
REM 用途：在新主機上快速部署 Docker + Cloudflare Tunnel

echo =========================================
echo PinionCore.NetSync - 部署腳本 (Windows)
echo =========================================
echo.

REM 檢查 .env 是否存在
if not exist .env (
    echo ❌ 錯誤：找不到 .env 檔案
    echo.
    echo 請依照以下步驟設定：
    echo 1. 複製 .env.example 為 .env
    echo    copy .env.example .env
    echo.
    echo 2. 編輯 .env 並填入你的 Cloudflare Tunnel Token
    echo    notepad .env
    echo.
    echo 3. 重新執行此腳本
    echo    deploy.bat
    echo.
    pause
    exit /b 1
)

echo ✅ 找到 .env 檔案
echo.

REM 檢查 Docker 是否安裝
docker --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 錯誤：未安裝 Docker
    echo.
    echo 請先安裝 Docker Desktop for Windows：
    echo    https://www.docker.com/products/docker-desktop
    echo.
    pause
    exit /b 1
)

echo ✅ Docker 環境檢查通過
echo.

REM 檢查 Sample2\app 目錄是否存在
if not exist Sample2\app (
    echo ⚠️  警告：找不到 Sample2\app 目錄
    echo.
    echo 請確認你已經在 Unity 中建置 WebGL：
    echo    File ^> Build Settings ^> WebGL
    echo    Build Location: %CD%\Sample2\app
    echo.
    set /p continue="是否繼續部署？(Y/N) "
    if /i not "%continue%"=="Y" (
        echo 部署已取消
        pause
        exit /b 0
    )
)

echo =========================================
echo 開始部署...
echo =========================================
echo.

REM 停止舊容器（如果存在）
echo 🛑 停止舊容器...
docker-compose down 2>nul
echo.

REM 建置並啟動容器
echo 🚀 建置並啟動容器...
docker-compose up -d --build
if errorlevel 1 (
    echo.
    echo ❌ 部署失敗！
    echo.
    echo 請檢查：
    echo 1. .env 中的 CLOUDFLARE_TUNNEL_TOKEN 是否正確
    echo 2. Docker Desktop 是否正在執行
    echo 3. 是否有其他程式佔用 Port 80
    echo.
    pause
    exit /b 1
)
echo.

REM 等待容器啟動
echo ⏳ 等待服務啟動...
timeout /t 5 /nobreak >nul
echo.

REM 檢查容器狀態
echo 📊 容器狀態：
docker-compose ps
echo.

REM 檢查 cloudflared 日誌
echo =========================================
echo Cloudflare Tunnel 連接狀態：
echo =========================================
docker-compose logs --tail=20 cloudflared
echo.

echo =========================================
echo ✅ 部署完成！
echo =========================================
echo.
echo 🌐 訪問網址：
echo    https://pinioncore.dpdns.org
echo    https://pinioncore.dpdns.org/sample2/
echo.
echo 📝 查看即時日誌：
echo    docker-compose logs -f
echo.
echo 🛑 停止服務：
echo    docker-compose down
echo.
pause
