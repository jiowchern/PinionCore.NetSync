# SSL 證書目錄

此目錄用於存放 `wss-proxy` 服務所需的 SSL/TLS 證書。

## 📁 必要文件

請將以下兩個文件放入此目錄：

- **cert.pem**: SSL 證書文件（公鑰）
- **key.pem**: SSL 私鑰文件

## 🔒 安全提醒

⚠️ **重要**: 此目錄及其內容已在 `.gitignore` 中被忽略，**不會被 Git 追蹤**。

- **絕對不要提交私鑰到 Git 儲存庫**
- 私鑰文件應妥善保管，僅存在於生產伺服器
- 定期更新證書（Cloudflare Origin Certificate 有效期最長 15 年）

## 📥 如何取得證書

### 方法一：Cloudflare Origin Certificate（推薦）

1. 登入 Cloudflare Dashboard
2. 選擇您的域名（例如 `pinioncore.dpdns.org`）
3. 前往 **SSL/TLS** → **Origin Server**
4. 點擊 **Create Certificate**
5. 設置如下：
   - **Private key type**: RSA (2048)
   - **Certificate Validity**: 15 years（或您需要的期限）
   - **Hostnames**:
     - `ws-direct.pinioncore.dpdns.org`
     - 或使用通配符 `*.pinioncore.dpdns.org`
6. 點擊 **Create**
7. 複製證書和私鑰：
   - **Origin Certificate** → 儲存為 `cert.pem`
   - **Private Key** → 儲存為 `key.pem`
8. 將兩個文件放入此目錄

### 方法二：Let's Encrypt（免費，需自動續期）

使用 Certbot 在伺服器上執行：

```bash
# 安裝 Certbot (Ubuntu/Debian)
sudo apt-get update
sudo apt-get install certbot

# 申請證書（需要停止佔用 80/443 端口的服務）
sudo certbot certonly --standalone -d ws-direct.pinioncore.dpdns.org

# 證書位置（複製到此目錄）
# cert.pem: /etc/letsencrypt/live/ws-direct.pinioncore.dpdns.org/fullchain.pem
# key.pem: /etc/letsencrypt/live/ws-direct.pinioncore.dpdns.org/privkey.pem
```

**注意**: Let's Encrypt 證書有效期僅 90 天，需設置自動續期。

### 方法三：自簽證書（僅用於測試，不推薦生產環境）

```bash
cd ssl-certs

# 生成自簽證書（有效期 365 天）
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout key.pem \
  -out cert.pem \
  -subj "/CN=ws-direct.pinioncore.dpdns.org"
```

⚠️ 瀏覽器會顯示「不安全」警告，僅適合本地測試。

## ✅ 驗證證書

確認文件存在且可讀：

```bash
# Windows
dir D:\develop\PinionCore.NetSync\Publishs\ssl-certs

# Linux/Mac
ls -lh /path/to/ssl-certs
```

應該看到：
```
cert.pem
key.pem
README.md
```

## 🚀 部署流程

1. 將證書文件放入此目錄
2. 啟動 Docker 服務：
   ```bash
   cd ../
   docker-compose up -d wss-proxy
   ```
3. 檢查日誌：
   ```bash
   docker-compose logs wss-proxy
   ```
4. 測試連接：
   ```bash
   curl -I https://ws-direct.pinioncore.dpdns.org/health
   ```

## 🔄 更新證書

當證書即將過期時：

1. 取得新證書（使用上述方法）
2. 替換此目錄中的 `cert.pem` 和 `key.pem`
3. 重啟 wss-proxy 服務：
   ```bash
   docker-compose restart wss-proxy
   ```

## ❓ 常見問題

### Q: 容器啟動失敗，提示找不到證書？

確認文件名稱完全一致（區分大小寫）：
- ✅ `cert.pem` / `key.pem`
- ❌ `certificate.pem` / `private.pem`

### Q: Nginx 報錯 "permission denied"？

檢查文件權限：
```bash
chmod 644 cert.pem
chmod 600 key.pem
```

### Q: 我應該使用哪種證書？

| 證書類型 | 優點 | 缺點 | 推薦場景 |
|---------|------|------|---------|
| **Cloudflare Origin** | 長期有效、免費、簡單 | 僅適用於 Cloudflare 代理的域名 | ✅ 生產環境（推薦）|
| **Let's Encrypt** | 免費、瀏覽器信任 | 90天需續期、需公網訪問 | 生產環境（需自動化）|
| **自簽證書** | 快速測試 | 瀏覽器警告、不安全 | ❌ 僅開發測試 |
