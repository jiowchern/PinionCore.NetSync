# 快速開始指南

## 🌐 公網部署（推薦）

### 在目標主機上執行

```bash
# 1. Clone 專案
git clone https://github.com/jiowchern/PinionCore.NetSync.git
cd PinionCore.NetSync/Publishs

# 2. 設定 Cloudflare Tunnel Token
cp .env.example .env
nano .env  # 填入 CLOUDFLARE_TUNNEL_TOKEN

# 3. 一鍵部署
chmod +x deploy.sh
./deploy.sh

# 4. 訪問
# https://pinioncore.dpdns.org
```

### Windows

```batch
REM 1. Clone 專案
git clone https://github.com/jiowchern/PinionCore.NetSync.git
cd PinionCore.NetSync\Publishs

REM 2. 設定 Cloudflare Tunnel Token
copy .env.example .env
notepad .env

REM 3. 一鍵部署
deploy.bat
```

---

## 💻 本地測試

```bash
# 1. 建立本地測試配置
cp docker-compose.override.yml.example docker-compose.override.yml

# 2. 啟動服務
docker-compose up -d

# 3. 訪問
# http://localhost/
# http://localhost/sample2/

# 4. 停止
docker-compose down
```

---

## 🔧 常用命令

```bash
# 查看日誌
docker-compose logs -f

# 重啟服務
docker-compose restart

# 重建容器
docker-compose up -d --build

# 完全清理並重建
docker-compose down
docker-compose up -d --build --force-recreate

# 檢查 Tunnel 狀態
docker-compose logs cloudflared | tail -20
```

---

## 📋 檢查清單

### 部署前
- [ ] 已在 Cloudflare 建立 Tunnel
- [ ] 已複製 Tunnel Token
- [ ] 已設定 Public Hostname 路由（`nginx-proxy:80`）
- [ ] 已在 Unity 建置 WebGL 到 `Sample2/app/`

### 部署後
- [ ] `docker-compose ps` 顯示所有容器 Up
- [ ] `docker-compose logs cloudflared` 顯示 "Connection registered"
- [ ] Cloudflare Dashboard 顯示 Tunnel 為 HEALTHY
- [ ] 可以訪問 https://pinioncore.dpdns.org

---

## ❓ 快速排錯

| 問題 | 解決方法 |
|------|---------|
| Tunnel UNHEALTHY | 檢查 .env 中的 Token 是否正確 |
| 404/502 錯誤 | 檢查 Public Hostname 是否設為 `nginx-proxy:80` |
| WebGL 無法加載 | 確認 `Sample2/app/` 目錄存在 |
| Port 80 被佔用 | 本地測試改用 `ports: ["8080:80"]` |
| 無法連接 Docker | 確認 Docker Desktop 正在執行 |

---

## 📚 詳細文檔

完整說明請參閱 [README.md](README.md)

- [Cloudflare Tunnel 設置教學](README.md#cloudflare-tunnel-完整設置教學)
- [目錄結構說明](README.md#目錄結構)
- [Git 管理規則](README.md#git-管理)
- [新增 Sample3](README.md#新增-sample3)
