# =============================================================================
# RentMaq Pro — Web Dockerfile (Angular 18 + nginx)
# Build context: src/RentMaq.Web/
#
# Multi-arch: node:20-alpine y nginx:alpine publican manifiestos
# para linux/amd64 y linux/arm64. NO se usa --platform.
#
# En produccion, los static files pueden servirse directamente desde nginx
# o integrarse en el host del API. Este Dockerfile es para desarrollo local
# y preview de builds de produccion.
# =============================================================================

# ---------------------------------------------------------------------------
# Stage 1: build — compilar Angular con Node.js 20
# npm ci: instalacion determinista desde package-lock.json
# ng build: compilacion AOT con optimizaciones de produccion
# ---------------------------------------------------------------------------
FROM node:20-alpine AS build
WORKDIR /app

# Capa de dependencias (cache-friendly)
COPY package.json package-lock.json ./
RUN npm ci --ignore-scripts

# Copia del codigo fuente y compilacion
COPY . .
RUN npx ng build --configuration=production

# ---------------------------------------------------------------------------
# Stage 2: runtime — servir con nginx (imagen minima, multi-arch)
# nginx:alpine: ~7 MB, soporta amd64 + arm64.
# ---------------------------------------------------------------------------
FROM nginx:alpine AS runtime

# Configuracion de nginx para SPA (redirige rutas a index.html)
RUN echo 'server { \
    listen 80; \
    root /usr/share/nginx/html; \
    index index.html; \
    location / { \
        try_files $uri $uri/ /index.html; \
    } \
    # Proxy API calls al backend (solo en contenedor) \
    location /api/ { \
        proxy_pass http://api:8080/api/; \
        proxy_set_header Host $host; \
        proxy_set_header X-Real-IP $remote_addr; \
    } \
}' > /etc/nginx/conf.d/default.conf

# Copiar artefactos compilados de Angular
# El path de salida depende de angular.json; ajustar si es diferente.
COPY --from=build /app/dist/rent-maq-web/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
