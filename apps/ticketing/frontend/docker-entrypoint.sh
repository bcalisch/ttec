#!/bin/sh
set -e

# Write runtime config for the Angular app
cat > /usr/share/nginx/html/assets/config.js <<EOF
window.__APP_CONFIG__ = {
  apiBaseUrl: '${PUBLIC_API_URL:-}',
  geoopsAppUrl: '${GEOOPS_APP_URL:-}'
};
EOF

# Process nginx template
envsubst '${BACKEND_HOST}' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf
