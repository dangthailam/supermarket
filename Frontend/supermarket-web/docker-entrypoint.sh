#!/bin/sh

# Replace placeholder with environment variable
if [ -n "$API_URL" ]; then
  echo "Setting API_URL to: $API_URL"
  find /usr/share/nginx/html -type f -name "*.js" -exec sed -i "s|__API_URL_PLACEHOLDER__|$API_URL|g" {} +
else
  echo "API_URL not set, using default"
  find /usr/share/nginx/html -type f -name "*.js" -exec sed -i "s|__API_URL_PLACEHOLDER__|https://supermarket-production-9d9b.up.railway.app/|g" {} +
fi

# Start nginx
nginx -g 'daemon off;'
