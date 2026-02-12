# Stage 1: Build
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npx ng build --configuration=production

# Stage 2: Runtime
FROM nginx:alpine
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html/browser
COPY nginx.conf.template /etc/nginx/nginx.conf.template
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Remove default nginx config
RUN rm -f /etc/nginx/conf.d/default.conf

EXPOSE 80

# Default API_URL for Docker Compose; override in Azure
ENV API_URL=http://api:8080

ENTRYPOINT ["/docker-entrypoint.sh"]
