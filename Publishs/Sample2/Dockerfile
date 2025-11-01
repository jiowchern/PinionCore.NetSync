FROM nginx:alpine

# 複製 Unity WebGL 建置檔案（從 app 子目錄）
COPY app/Build /usr/share/nginx/html/Build
COPY app/index.html /usr/share/nginx/html/
COPY app/TemplateData /usr/share/nginx/html/TemplateData

# 複製 nginx 配置（包含 WebGL 所需的 MIME types）
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

# nginx 會自動啟動
