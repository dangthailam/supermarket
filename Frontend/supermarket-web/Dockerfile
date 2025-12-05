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

# Copy built app
COPY --from=build /app/dist/supermarket-web/browser /usr/share/nginx/html

# Expose port
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]