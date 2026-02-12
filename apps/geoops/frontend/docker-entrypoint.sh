#!/bin/sh
# Write runtime config for Angular
# PUBLIC_API_URL set → Angular calls API directly (CORS mode, used in Azure)
# PUBLIC_API_URL empty → Angular uses relative URLs (nginx proxy mode, used in docker-compose)
cat > /usr/share/nginx/html/browser/assets/config.js << JSEOF
window.__APP_CONFIG__ = {
  apiBaseUrl: '${PUBLIC_API_URL:-}',
};
JSEOF

# Generate nginx config (proxy still works for docker-compose)
envsubst '${API_URL}' < /etc/nginx/nginx.conf.template > /etc/nginx/conf.d/default.conf
exec nginx -g 'daemon off;'
