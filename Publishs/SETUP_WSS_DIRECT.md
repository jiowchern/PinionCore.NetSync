# WSS 直連設置指南

此文件說明如何設置 `wss://ws-direct.pinioncore.dpdns.org` 直連模式，繞過 Cloudflare Tunnel 直接連接到您的伺服器。

## 📋 設置清單

- [ ] 1. 在 Cloudflare 設置 DNS A 記錄（灰雲）
- [ ] 2. 取得 SSL 證書（Cloudflare Origin Certificate）
- [ ] 3. 放置證書文件到 `ssl-certs/` 目錄
- [ ] 4. 啟動 wss-proxy Docker 服務
- [ ] 5. 驗證連接

---

## 步驟 1: Cloudflare DNS 設置

### 操作步驟

1. **登入 Cloudflare Dashboard**
   - 前往 https://dash.cloudflare.com/
   - 選擇您的域名（例如管理 `pinioncore.dpdns.org` 的主域名）

2. **新增/修改 DNS 記錄**
   - 點擊左側選單 **DNS** → **Records**
   - 如果 `ws-direct` 記錄已存在（CNAME），請刪除它
   - 點擊 **Add record**
   - 設置如下：
     ```
     Type: A
     Name: ws-direct
     IPv4 address: 125.229.192.110
     Proxy status: 🔘 灰色雲朵 (DNS only)
     TTL: Auto
     ```
   - **重要**: 必須點擊橘色雲朵切換成灰色雲朵
   - 點擊 **Save**

3. **驗證 DNS 解析**
   - 等待 1-5 分鐘讓 DNS 傳播
   - 執行命令驗證：
     ```bash
     # Windows
     nslookup ws-direct.pinioncore.dpdns.org

     # Linux/Mac
     dig ws-direct.pinioncore.dpdns.org
     ```
   - 應該看到解析結果為 `125.229.192.110`

---

## 步驟 2: 取得 Cloudflare Origin Certificate

### 為什麼使用 Origin Certificate？

✅ **優點**:
- 免費且長期有效（最長 15 年）
- 無需自動續期
- 專為 Cloudflare 設計
- 簡單快速

❌ **限制**:
- 僅適用於經過 Cloudflare 的域名
- 瀏覽器不直接信任（但沒關係，因為 Cloudflare 會處理客戶端連接）

### 取得證書步驟

1. **登入 Cloudflare Dashboard**
   - 前往 https://dash.cloudflare.com/
   - 選擇您的域名

2. **前往 SSL/TLS 設置**
   - 左側選單點擊 **SSL/TLS**
   - 點擊 **Origin Server** 子選單

3. **創建證書**
   - 點擊 **Create Certificate** 按鈕
   - 設置如下：
     ```
     Private key type: RSA (2048)
     Certificate Validity: 15 years (推薦，避免頻繁更新)
     Hostnames:
       - ws-direct.pinioncore.dpdns.org
       或使用通配符:
       - *.pinioncore.dpdns.org
       - pinioncore.dpdns.org
     ```
   - 點擊 **Create**

4. **下載證書和私鑰**

   ⚠️ **重要**: 這是唯一一次可以看到私鑰，請立即複製！

   - **第一個文本框（Origin Certificate）**:
     ```
     複製內容 → 儲存為 D:\develop\PinionCore.NetSync\Publishs\ssl-certs\cert.pem
     ```
     內容格式類似：
     ```
     -----BEGIN CERTIFICATE-----
     MIIEpDCCAoy...
     -----END CERTIFICATE-----
     ```

   - **第二個文本框（Private Key）**:
     ```
     複製內容 → 儲存為 D:\develop\PinionCore.NetSync\Publishs\ssl-certs\key.pem
     ```
     內容格式類似：
     ```
     -----BEGIN PRIVATE KEY-----
     MIIEvgIBADA...
     -----END PRIVATE KEY-----
     ```

5. **點擊 OK 完成**

---

## 步驟 3: 放置證書文件

### 文件結構

確保 `ssl-certs/` 目錄中有以下文件：

```
D:\develop\PinionCore.NetSync\Publishs\ssl-certs\
├── cert.pem       # Origin Certificate（公鑰）
├── key.pem        # Private Key（私鑰）
└── README.md      # 說明文件
```

### 驗證文件

#### Windows:
```powershell
cd D:\develop\PinionCore.NetSync\Publishs
dir ssl-certs
```

應該看到 `cert.pem` 和 `key.pem` 兩個文件。

#### Linux/Mac:
```bash
ls -lh ssl-certs/
```

### 檢查文件內容

```bash
# 檢查證書
head -n 2 ssl-certs/cert.pem
# 應顯示: -----BEGIN CERTIFICATE-----

# 檢查私鑰
head -n 2 ssl-certs/key.pem
# 應顯示: -----BEGIN PRIVATE KEY-----
```

---

## 步驟 4: 啟動 Docker 服務

### 啟動 wss-proxy

```bash
cd D:\develop\PinionCore.NetSync\Publishs

# 啟動 wss-proxy 服務（會自動啟動依賴的 router）
docker-compose up -d wss-proxy

# 檢查服務狀態
docker-compose ps
```

應該看到：
```
NAME                      STATUS
pinioncore-wss-proxy      Up
pinioncore-router         Up (healthy)
pinioncore-chatserver     Up
```

### 檢查日誌

```bash
# 查看 wss-proxy 日誌
docker-compose logs wss-proxy

# 查看 router 日誌
docker-compose logs router

# 即時監控所有日誌
docker-compose logs -f
```

**正常日誌示例**:
```
pinioncore-wss-proxy  | nginx: [warn] the "user" directive makes sense only if the master process runs with super-user privileges, ignored in /etc/nginx/nginx.conf:...
pinioncore-router     | Gateway Router listening on 0.0.0.0:8001 (TCP)
pinioncore-router     | Gateway Router listening on 0.0.0.0:8002 (WebSocket)
```

### 常見錯誤排查

#### 錯誤 1: 找不到證書
```
nginx: [emerg] cannot load certificate "/etc/nginx/certs/cert.pem"
```
**解決**: 確認 `ssl-certs/cert.pem` 和 `key.pem` 存在且文件名正確。

#### 錯誤 2: 端口被佔用
```
Error starting userland proxy: listen tcp4 0.0.0.0:443: bind: address already in use
```
**解決**:
- Windows: 檢查 IIS 或其他 Web 服務是否佔用 443 端口
- 修改 `docker-compose.yml` 改用其他端口（如 `8443:443`）

#### 錯誤 3: 證書格式錯誤
```
nginx: [emerg] PEM_read_bio_X509_AUX("/etc/nginx/certs/cert.pem") failed
```
**解決**: 重新從 Cloudflare 複製證書，確保包含完整的 `-----BEGIN CERTIFICATE-----` 和 `-----END CERTIFICATE-----` 標記。

---

## 步驟 5: 驗證連接

### 測試 1: 健康檢查

```bash
# Windows PowerShell
Invoke-WebRequest -Uri https://ws-direct.pinioncore.dpdns.org/health

# Linux/Mac
curl -I https://ws-direct.pinioncore.dpdns.org/health
```

**預期輸出**:
```
HTTP/1.1 200 OK
Server: nginx/1.25...
Content-Type: text/plain
Content-Length: 3
```

### 測試 2: SSL 證書驗證

```bash
openssl s_client -connect ws-direct.pinioncore.dpdns.org:443
```

**預期輸出** (部分):
```
Certificate chain
 0 s:CN=*.pinioncore.dpdns.org
   i:C=US, O=Cloudflare, Inc., OU=Origin SSL Certificate
---
Server certificate
-----BEGIN CERTIFICATE-----
...
```

### 測試 3: WebSocket 連接

使用瀏覽器控制台測試：

```javascript
// 開啟瀏覽器控制台 (F12)
const ws = new WebSocket("wss://ws-direct.pinioncore.dpdns.org");

ws.onopen = () => console.log("WebSocket 連接成功！");
ws.onerror = (e) => console.error("WebSocket 錯誤:", e);
ws.onclose = () => console.log("WebSocket 已關閉");
```

**預期輸出**:
```
WebSocket 連接成功！
```

### 測試 4: Unity WebGL 客戶端測試

修改客戶端連接端點：

```csharp
// Sample2-Chat/Client.cs 或您的連接邏輯
string endpoint = "wss://ws-direct.pinioncore.dpdns.org";
```

重新建置並測試 WebGL 應用。

---

## 🔧 進階配置

### 修改 WebSocket 端口

如果需要使用非標準端口（例如 8443）：

**docker-compose.yml**:
```yaml
wss-proxy:
  ports:
    - "8443:443"  # 改為 8443
```

**客戶端連接**:
```javascript
wss://ws-direct.pinioncore.dpdns.org:8443
```

### 禁用 HTTP 重定向

如果不需要 HTTP → HTTPS 重定向：

**wss-proxy.conf** (移除此區塊):
```nginx
# 註解或刪除此部分
# server {
#     listen 80;
#     server_name ws-direct.pinioncore.dpdns.org;
#     return 301 https://$server_name$request_uri;
# }
```

**docker-compose.yml**:
```yaml
wss-proxy:
  ports:
    - "443:443"  # 移除 "80:80"
```

### 啟用訪問日誌

查看完整的 WebSocket 連接日誌：

```bash
# 進入容器
docker exec -it pinioncore-wss-proxy sh

# 查看訪問日誌
tail -f /var/log/nginx/access.log

# 查看錯誤日誌
tail -f /var/log/nginx/error.log
```

---

## 📊 架構對比

### 原始架構 (Cloudflare Tunnel)
```
客戶端 → Cloudflare CDN → Cloudflare Tunnel → nginx-proxy → router:8002
```

### 新架構 (WSS 直連)
```
客戶端 → 125.229.192.110:443 → wss-proxy (SSL終止) → router:8002
```

### 性能比較

| 指標 | Cloudflare Tunnel | WSS 直連 |
|-----|------------------|---------|
| **延遲** | +20-50ms (經過 CDN) | 直連延遲 |
| **頻寬** | 無限制（Cloudflare 免費） | 取決於您的網路 |
| **DDoS 防護** | ✅ 有 | ❌ 無 |
| **SSL 管理** | ✅ 自動 | ⚠️ 需手動更新 |
| **IP 隱藏** | ✅ 是 | ❌ 暴露真實 IP |
| **適用場景** | 公開服務、需防護 | 私有服務、低延遲優先 |

---

## 🔒 安全建議

### 1. 防火牆規則

僅允許 Cloudflare IP 訪問 443 端口（如果仍使用 Cloudflare DNS）：

```bash
# Linux iptables 示例
# 下載 Cloudflare IP 列表
curl https://www.cloudflare.com/ips-v4 -o cloudflare-ips.txt

# 僅允許 Cloudflare IP
iptables -A INPUT -p tcp --dport 443 -s <Cloudflare_IP> -j ACCEPT
iptables -A INPUT -p tcp --dport 443 -j DROP
```

### 2. 定期更新證書

設置日曆提醒在證書過期前更新（Cloudflare Origin Certificate 可設 15 年）。

### 3. 監控異常流量

使用日誌分析工具監控 WebSocket 連接異常：

```bash
# 統計連接數
docker exec pinioncore-wss-proxy sh -c "tail -n 1000 /var/log/nginx/access.log | wc -l"
```

---

## ❓ 常見問題

### Q1: 為什麼瀏覽器顯示證書不受信任？

**A**: 如果使用 Cloudflare Origin Certificate 且 DNS 是灰雲（DNS only），瀏覽器可能不信任該證書。

**解決方案**:
- 選項 A: 改用 Let's Encrypt 證書（瀏覽器信任）
- 選項 B: 將 DNS 改為橘雲（Proxied），但這會失去直連優勢
- 選項 C: 僅用於 WebSocket 連接（非瀏覽器訪問），Unity WebGL 通常不驗證證書

### Q2: 可以同時使用 Tunnel 和直連嗎？

**A**: 可以！保留兩個端點：
- `wss://ws.pinioncore.dpdns.org` → Cloudflare Tunnel（安全、有防護）
- `wss://ws-direct.pinioncore.dpdns.org` → 直連（低延遲）

客戶端可根據需求選擇：
```csharp
string endpoint = lowLatencyMode
    ? "wss://ws-direct.pinioncore.dpdns.org"
    : "wss://ws.pinioncore.dpdns.org";
```

### Q3: 如何撤銷或更換證書？

**A**: 在 Cloudflare Dashboard:
1. SSL/TLS → Origin Server
2. 找到舊證書，點擊 **Revoke**
3. 創建新證書（重複步驟 2）
4. 更新 `ssl-certs/` 文件
5. 重啟服務: `docker-compose restart wss-proxy`

### Q4: 證書過期了怎麼辦？

**A**: Cloudflare Origin Certificate 最長 15 年，到期前：
1. 重新執行步驟 2 創建新證書
2. 替換 `cert.pem` 和 `key.pem`
3. 重啟服務

---

## 📞 需要幫助？

如果遇到問題：

1. **檢查日誌**: `docker-compose logs wss-proxy router`
2. **驗證 DNS**: `nslookup ws-direct.pinioncore.dpdns.org`
3. **測試端口**: `telnet 125.229.192.110 443`
4. **檢查證書**: `openssl s_client -connect ws-direct.pinioncore.dpdns.org:443`

---

## 🎉 完成！

設置完成後，您的客戶端可以使用：

```
wss://ws-direct.pinioncore.dpdns.org
```

直接連接到您的 Gateway Router，繞過 Cloudflare Tunnel，獲得最低延遲！
