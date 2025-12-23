# Build stage
FROM node:lts-alpine AS build

ARG API_URL
ARG SUPABASE_URL
ARG SUPABASE_ANON_KEY
ENV API_URL=${API_URL}
ENV SUPABASE_URL=${SUPABASE_URL}
ENV SUPABASE_ANON_KEY=${SUPABASE_ANON_KEY}

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install packages
RUN npm ci

# Copy source code
COPY . ./

# Replace placeholders before build
RUN if [ -n "$API_URL" ]; then \
      find src/environments -name "*.ts" -exec sed -i "s|\${API_URL}|${API_URL}|g" {} + ; \
    fi && \
    if [ -n "$SUPABASE_URL" ]; then \
      find src/environments -name "*.ts" -exec sed -i "s|\${SUPABASE_URL}|${SUPABASE_URL}|g" {} + ; \
    fi && \
    if [ -n "$SUPABASE_ANON_KEY" ]; then \
      find src/environments -name "*.ts" -exec sed -i "s|\${SUPABASE_ANON_KEY}|${SUPABASE_ANON_KEY}|g" {} + ; \
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