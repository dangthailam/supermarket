# Build stage
FROM node:lts-alpine AS build

ARG API_URL
ENV API_URL=${API_URL}

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install packages
RUN npm ci

# Copy source code
COPY . ./

# Replace API_URL placeholder before build
RUN if [ -n "$API_URL" ]; then \
      find src/environments -name "*.ts" -exec sed -i "s|\${API_URL}|${API_URL}|g" {} + ; \
    fi

# Build the app
RUN npm run build

# Production stage
FROM nginx:alpine

# Copy nginx template
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

# Copy built app
COPY --from=build /app/dist/supermarket-web/browser /usr/share/nginx/html

# Railway uses PORT env variable
ENV PORT=8081
EXPOSE $PORT

CMD ["sh", "-c", "envsubst '$PORT' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]