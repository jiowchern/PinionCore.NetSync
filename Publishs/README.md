# PinionCore.NetSync - WebGL Samples Docker 部署

## 🚀 公網部署快速開始（Cloudflare Tunnel）

### 前置要求
- 已註冊 Cloudflare 帳號
- 已在 Cloudflare 建立 Tunnel（詳細步驟見下方）
- 域名：`pinioncore.dpdns.org`（或你的域名）

### 一鍵部署

1. **Clone 專案到目標主機**：
   ```bash
   git clone https://github.com/jiowchern/PinionCore.NetSync.git
   cd PinionCore.NetSync/Publishs
   ```

2. **設定 Cloudflare Tunnel Token**：
   ```bash
   # Linux/Mac
   cp .env.example .env
   nano .env  # 填入你的 CLOUDFLARE_TUNNEL_TOKEN

   # Windows
   copy .env.example .env
   notepad .env  # 填入你的 CLOUDFLARE_TUNNEL_TOKEN
   ```

3. **執行一鍵部署腳本**：
   ```bash
   # Linux/Mac
   chmod +x deploy.sh
   ./deploy.sh

   # Windows
   deploy.bat
   ```

4. **訪問你的應用**：
   - https://pinioncore.dpdns.org
   - https://pinioncore.dpdns.org/sample2/

---

## 架構說明

此專案使用多容器架構，透過反向代理提供統一的訪問介面：

```
http://localhost/            → 首頁（列出所有 Sample）
http://localhost/sample2/    → Sample2 WebGL Player
http://localhost/sample3/    → Sample3 WebGL Player（未來新增）
```

### 容器結構

- **cloudflared**：Cloudflare Tunnel 客戶端（公網訪問入口）
- **nginx-proxy**：反向代理 + 首頁（內部 HTTP）
- **sample2**：Sample2 獨立容器
- **sample3**：Sample3 獨立容器（未來新增）

### 網路架構

```
公網訪問:
https://pinioncore.dpdns.org
          ↓
[Cloudflare CDN + DDoS Protection]
          ↓
   cloudflared tunnel
          ↓
    nginx-proxy (內部)
      ↙        ↘
  sample2    sample3
```

## 本地開發快速開始

⚠️ **注意**：以下步驟僅適用於**本地測試**。如需公網部署，請參考上方 [🚀 公網部署快速開始](#公網部署快速開始cloudflare-tunnel)。

### 本地測試（無需 Cloudflare）

1. **臨時修改 docker-compose.yml**：
   ```bash
   # 註解掉 cloudflared 服務，並啟用 nginx-proxy 的 port 映射
   # 或使用 docker-compose.local.yml（見下方）
   ```

2. **啟動服務**：
   ```bash
   cd Publishs
   docker-compose up -d
   ```

3. **訪問應用**：
   - **首頁**：http://localhost/
   - **Sample2**：http://localhost/sample2/

4. **停止服務**：
   ```bash
   docker-compose down
   ```

### 本地測試專用配置

建立 `docker-compose.override.yml`（僅用於本地測試，不提交到 Git）：

```yaml
version: '3.8'

services:
  # 本地測試時禁用 cloudflared
  cloudflared:
    profiles: ["production"]

  # 本地測試時啟用 port 映射
  nginx-proxy:
    ports:
      - "80:80"
```

使用方式：
```bash
# 本地測試
docker-compose up -d

# 公網部署（需要 .env）
docker-compose --profile production up -d
```

## 新增 Sample3

### 步驟 1：複製 Sample2 結構

```bash
cd Publishs
cp -r Sample2 Sample3
```

### 步驟 2：修改 docker-compose.yml

取消註解 `sample3` 服務：

```yaml
  sample3:
    build: ./Sample3
    container_name: pinioncore-sample3
    networks:
      - pinioncore-net
    restart: unless-stopped
```

### 步驟 3：修改 nginx-proxy/nginx.conf

取消註解 Sample3 反向代理規則：

```nginx
location /sample3/ {
    proxy_pass http://sample3/;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_read_timeout 300s;
    proxy_connect_timeout 75s;
}
```

### 步驟 4：修改首頁

編輯 `nginx-proxy/html/index.html`，取消註解 Sample3 卡片並修改內容。

### 步驟 5：重新啟動

```bash
docker-compose up -d --build
```

## 目錄結構

```
Publishs/
├── docker-compose.yml          # 服務編排
├── README.md                   # 本檔案
├── nginx-proxy/                # 反向代理 + 首頁
│   ├── Dockerfile
│   ├── nginx.conf
│   └── html/
│       └── index.html
├── Sample2/                    # Sample2 WebGL
│   ├── app/                    # Unity WebGL 建置（被 git 忽略）
│   │   ├── Build/
│   │   ├── index.html
│   │   └── TemplateData/
│   ├── Dockerfile              # Docker 配置（會被追蹤）
│   └── nginx.conf              # Docker 配置（會被追蹤）
└── Sample3/                    # 未來新增
    ├── app/                    # Unity WebGL 建置（被 git 忽略）
    ├── Dockerfile
    └── nginx.conf
```

**注意**：`Sample*/app/` 目錄包含 Unity WebGL 建置檔案，已在 `.gitignore` 中被忽略。只有 Docker 配置檔案會被 Git 追蹤。

## 技術細節

### Unity WebGL MIME Types

每個 Sample 的 `nginx.conf` 已配置 Unity WebGL 所需的 MIME types：
- `.wasm.gz` → `application/wasm`
- `.js.gz` → `application/javascript`
- `.data.gz` → `application/octet-stream`

### 反向代理規則

- 所有請求透過 `nginx-proxy` 路由
- `/sample2/` → `http://sample2/`（內部 Docker 網路）
- 自動處理 WebSocket 升級（WebGL 網路連接需要）

### 容器優化

- 使用 `nginx:alpine` 減少映像大小
- 設定 `restart: unless-stopped` 確保服務可用性
- 獨立網路 `pinioncore-net` 隔離容器

## 常見問題

### Q: 如何修改 Sample2 後更新？

```bash
# 在 Unity 重新建置 WebGL 後
cd Publishs
docker-compose up -d --build sample2
```

### Q: 如何更改 Port？

修改 `docker-compose.yml` 中 `nginx-proxy` 的 ports：

```yaml
nginx-proxy:
  ports:
    - "8080:80"  # 改為 http://localhost:8080/
```

### Q: 容器啟動失敗？

檢查日誌：
```bash
docker-compose logs nginx-proxy
docker-compose logs sample2
```

常見原因：
- Port 80 被佔用 → 修改 docker-compose.yml 的 ports
- 檔案路徑錯誤 → 確認 Build、TemplateData 目錄存在

### Q: WebGL 加載失敗？

1. 檢查瀏覽器控制台（F12）的錯誤訊息
2. 確認 MIME type 設定正確：
   ```bash
   curl -I http://localhost/sample2/Build/Sample2.wasm.gz
   ```
   應包含 `Content-Type: application/wasm`

## Git 管理

### 什麼會被追蹤？

✅ **會被 Git 追蹤**：
- Docker 配置：`Dockerfile`, `nginx.conf`, `docker-compose.yml`
- 文檔：`README.md`
- nginx-proxy 所有檔案

❌ **會被 Git 忽略**（`.gitignore` 規則：`Publishs/Sample*/app/`）：
- Unity WebGL 建置：`app/Build/`, `app/TemplateData/`, `app/index.html`
- 所有 `Sample*/app/` 目錄內容

### 工作流程

1. **在 Unity 中建置 WebGL**：
   ```
   File > Build Settings > WebGL
   Build Location: D:\develop\PinionCore.NetSync\Publishs\Sample2\app
   ```

2. **測試 Docker 部署**：
   ```bash
   cd Publishs
   docker-compose up -d --build
   ```

3. **提交 Docker 配置變更**：
   ```bash
   git add Publishs/
   git commit -m "chore: update Sample2 Docker config"
   git push
   ```

4. **團隊成員同步**：
   ```bash
   git pull
   # 在 Unity 中自行建置 WebGL 到 Sample2/app/
   docker-compose up -d --build
   ```

## 開發建議

### 本地測試

在 Unity 中使用 `File > Build Settings > WebGL > Build` 建置到 `Sample*/app/` 後：
```bash
cd Publishs
docker-compose up -d --build
```

### 生產部署

1. 使用 Docker Registry 推送映像：
   ```bash
   docker tag pinioncore-sample2 your-registry/pinioncore-sample2:v1.0
   docker push your-registry/pinioncore-sample2:v1.0
   ```

2. 在生產環境使用 Docker Compose 或 Kubernetes 部署

### 安全性

- ✅ **已內建**：使用 Cloudflare Tunnel 自動提供 HTTPS + DDoS 防護
- 限制 CORS（修改 nginx.conf 的 `Access-Control-Allow-Origin`）
- 定期更新 nginx 和 cloudflared 映像

## Cloudflare Tunnel 完整設置教學

### 步驟 1：建立 Cloudflare Tunnel

1. **登入 Cloudflare Zero Trust Dashboard**：
   - 前往 https://one.dash.cloudflare.com/
   - 選擇你的帳號

2. **建立新的 Tunnel**：
   - 左側選單：**Access** → **Tunnels**
   - 點擊 **Create a tunnel**
   - 選擇 **Cloudflared**
   - Tunnel 名稱：`pinioncore-netsync`（或你喜歡的名稱）
   - 點擊 **Save tunnel**

3. **複製 Tunnel Token**：
   - ⚠️ **重要**：這個 Token 只會顯示一次，請立即複製！
   - 格式類似：`eyJhIjoixxxxxxxxxxxxxxxxxxxxxxxx...`
   - 儲存到安全的地方（稍後會用到）

4. **跳過安裝步驟**（我們使用 Docker）：
   - 點擊 **Next**

### 步驟 2：設定 Public Hostname 路由

在 Tunnel 的 **Public Hostname** 頁籤中：

1. **添加首頁路由**：
   - **Subdomain**: `pinioncore`（或留空使用根域名）
   - **Domain**: `dpdns.org`（選擇你的域名）
   - **Path**: 留空
   - **Type**: `HTTP`
   - **URL**: `nginx-proxy:80`
   - 點擊 **Save hostname**

2. **（可選）添加 Sample2 專屬路由**：
   - 如果需要子域名 `sample2.pinioncore.dpdns.org`
   - **Subdomain**: `sample2.pinioncore`
   - **Domain**: `dpdns.org`
   - **Type**: `HTTP`
   - **URL**: `sample2:80`

### 步驟 3：配置環境變數

在目標主機上：

```bash
cd PinionCore.NetSync/Publishs

# 複製環境變數模板
cp .env.example .env

# 編輯 .env
nano .env  # 或使用 vim、notepad 等
```

填入你的 Tunnel Token：

```bash
CLOUDFLARE_TUNNEL_TOKEN=eyJhIjoixxxxxxxxxxxxxxxxxxxxxxxx...
DOMAIN=pinioncore.dpdns.org
```

### 步驟 4：部署

```bash
# Linux/Mac
chmod +x deploy.sh
./deploy.sh

# Windows
deploy.bat
```

### 步驟 5：驗證部署

1. **檢查 Tunnel 狀態**：
   ```bash
   docker-compose logs cloudflared
   ```

   應該看到類似訊息：
   ```
   2025-11-01T08:00:00Z INF Connection registered connIndex=0
   2025-11-01T08:00:00Z INF Starting metrics server on 127.0.0.1:XXXX
   ```

2. **測試訪問**：
   - 開啟瀏覽器，前往 https://pinioncore.dpdns.org
   - 應該看到 Sample 列表首頁

3. **檢查 Cloudflare Dashboard**：
   - 返回 Tunnels 頁面
   - 你的 Tunnel 狀態應該顯示為 **HEALTHY** 🟢

### 常見問題排除

#### Q1: Tunnel 顯示 UNHEALTHY

檢查 Token 是否正確：
```bash
cat .env  # 確認 CLOUDFLARE_TUNNEL_TOKEN
docker-compose logs cloudflared  # 查看錯誤訊息
```

#### Q2: 無法訪問網站（404 或 502）

檢查 Public Hostname 設定：
- Service Type 必須是 `HTTP`（不是 HTTPS）
- URL 必須是 `nginx-proxy:80`（容器名稱:port）

重啟容器：
```bash
docker-compose restart cloudflared
```

#### Q3: WebGL 加載失敗

檢查瀏覽器控制台（F12）：
- 確認 Unity WebGL 檔案已正確建置到 `Sample2/app/`
- 檢查 MIME type 錯誤

重新建置容器：
```bash
docker-compose up -d --build
```

#### Q4: 想更換域名

1. 修改 Cloudflare Dashboard 的 Public Hostname
2. 更新 `.env` 中的 `DOMAIN`
3. 重啟容器：`docker-compose restart cloudflared`

### Tunnel 管理命令

```bash
# 查看即時日誌
docker-compose logs -f cloudflared

# 重啟 Tunnel
docker-compose restart cloudflared

# 停止所有服務
docker-compose down

# 完全重建並啟動
docker-compose up -d --build --force-recreate
```

### Cloudflare Tunnel 優勢

✅ **零端口映射**：不需要在路由器開放任何 port
✅ **自動 HTTPS**：Cloudflare 自動提供 SSL 證書
✅ **DDoS 防護**：免費 CDN + DDoS 防護
✅ **動態 IP 友善**：主機 IP 變動不影響訪問
✅ **隱藏真實 IP**：提升安全性

## 授權

本專案遵循主儲存庫授權條款。
