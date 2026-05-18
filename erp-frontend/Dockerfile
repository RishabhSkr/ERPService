# ============================================
# STAGE 1: BUILD (Node image — React app build karo)
# ============================================
FROM node:22-alpine AS build
WORKDIR /app

# Pehle sirf package files copy karo (npm install cache ke liye)
COPY package.json package-lock.json ./
RUN npm ci
# npm ci = clean install (package-lock.json se exact versions)
# npm install se better — faster + deterministic

# Ab source code copy karo aur build
COPY . .
RUN npm run build
# Result: /app/dist/ folder mein static HTML/JS/CSS

# ============================================
# STAGE 2: SERVE (Nginx — lightweight web server)
# ============================================
FROM nginx:alpine AS runtime

# Custom nginx config (SPA routing + API proxy)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Build stage se dist/ files copy karo
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
