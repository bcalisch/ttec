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
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
