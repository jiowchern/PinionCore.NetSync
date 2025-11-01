#!/bin/bash

# PinionCore.NetSync - 一鍵部署腳本
# 用途：在新主機上快速部署 Docker + Cloudflare Tunnel

set -e  # 遇到錯誤立即退出

echo "========================================="
echo "PinionCore.NetSync - 部署腳本"
echo "========================================="
echo ""

# 檢查 .env 是否存在
if [ ! -f .env ]; then
    echo "❌ 錯誤：找不到 .env 檔案"
    echo ""
    echo "請依照以下步驟設定："
    echo "1. 複製 .env.example 為 .env"
    echo "   cp .env.example .env"
    echo ""
    echo "2. 編輯 .env 並填入你的 Cloudflare Tunnel Token"
    echo "   nano .env"
    echo ""
    echo "3. 重新執行此腳本"
    echo "   ./deploy.sh"
    echo ""
    exit 1
fi

# 載入環境變數
source .env

# 檢查 CLOUDFLARE_TUNNEL_TOKEN 是否已設定
if [ "$CLOUDFLARE_TUNNEL_TOKEN" == "your_tunnel_token_here" ] || [ -z "$CLOUDFLARE_TUNNEL_TOKEN" ]; then
    echo "❌ 錯誤：尚未設定 Cloudflare Tunnel Token"
    echo ""
    echo "請編輯 .env 檔案並填入正確的 Token："
    echo "   nano .env"
    echo ""
    exit 1
fi

echo "✅ 環境變數檢查通過"
echo ""

# 檢查 Docker 是否安裝
if ! command -v docker &> /dev/null; then
    echo "❌ 錯誤：未安裝 Docker"
    echo ""
    echo "請先安裝 Docker："
    echo "   curl -fsSL https://get.docker.com -o get-docker.sh"
    echo "   sudo sh get-docker.sh"
    echo ""
    exit 1
fi

# 檢查 Docker Compose 是否安裝
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo "❌ 錯誤：未安裝 Docker Compose"
    echo ""
    echo "請先安裝 Docker Compose"
    exit 1
fi

echo "✅ Docker 環境檢查通過"
echo ""

# 檢查 Sample2/app 目錄是否存在
if [ ! -d "Sample2/app" ]; then
    echo "⚠️  警告：找不到 Sample2/app 目錄"
    echo ""
    echo "請確認你已經在 Unity 中建置 WebGL："
    echo "   File > Build Settings > WebGL"
    echo "   Build Location: $(pwd)/Sample2/app"
    echo ""
    read -p "是否繼續部署？(y/N) " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "部署已取消"
        exit 0
    fi
fi

echo "========================================="
echo "開始部署..."
echo "========================================="
echo ""

# 停止舊容器（如果存在）
echo "🛑 停止舊容器..."
docker-compose down || true
echo ""

# 建置並啟動容器
echo "🚀 建置並啟動容器..."
docker-compose up -d --build
echo ""

# 等待容器啟動
echo "⏳ 等待服務啟動..."
sleep 5
echo ""

# 檢查容器狀態
echo "📊 容器狀態："
docker-compose ps
echo ""

# 檢查 cloudflared 日誌
echo "========================================="
echo "Cloudflare Tunnel 連接狀態："
echo "========================================="
docker-compose logs cloudflared | tail -n 20
echo ""

echo "========================================="
echo "✅ 部署完成！"
echo "========================================="
echo ""
echo "🌐 訪問網址："
echo "   https://$DOMAIN"
echo "   https://$DOMAIN/sample2/"
echo ""
echo "📝 查看即時日誌："
echo "   docker-compose logs -f"
echo ""
echo "🛑 停止服務："
echo "   docker-compose down"
echo ""
