FROM nginx:alpine

# 複製反向代理配置
COPY nginx.conf /etc/nginx/conf.d/default.conf

# 複製首頁
COPY html/ /usr/share/nginx/html/

EXPOSE 80
