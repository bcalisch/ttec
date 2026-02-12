#!/bin/sh
set -e

cat > /usr/share/nginx/html/config.js <<EOF
window.__LANDING_CONFIG__ = {
  geoopsUrl: '${GEOOPS_URL:-http://localhost:80}',
  ticketingUrl: '${TICKETING_URL:-http://localhost:81}'
};
EOF
