# =============================================================================
# RentMaq Pro — API Dockerfile (.NET 8, Multi-stage)
# Build context: raiz del monorepo (./)
#
# Multi-arch: las imagenes mcr.microsoft.com/dotnet/sdk:8.0 y aspnet:8.0
# publican manifiestos para linux/amd64 y linux/arm64.
# NO se usa --platform en FROM: Docker resuelve la arquitectura del host
# automaticamente. .NET 8 JIT es cross-platform, no requiere -r <RID>.
#
# Nota sobre dependencias nativas:
# Si en el futuro se necesita libgdiplus (generacion de PDFs con System.Drawing),
# agregar en el stage runtime:
#   RUN apt-get update && apt-get install -y --no-install-recommends libgdiplus \
#       && rm -rf /var/lib/apt/lists/*
# =============================================================================

# ---------------------------------------------------------------------------
# Stage 1: restore — capa de dependencias (cache-friendly)
# Copia solo .csproj + .sln para maximizar cache de NuGet restore.
# Cambios en codigo fuente no invalidan esta capa.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY src/RentMaq.sln ./
COPY src/RentMaq.Domain/RentMaq.Domain.csproj             RentMaq.Domain/
COPY src/RentMaq.Application/RentMaq.Application.csproj   RentMaq.Application/
COPY src/RentMaq.Infrastructure/RentMaq.Infrastructure.csproj RentMaq.Infrastructure/
COPY src/RentMaq.API/RentMaq.API.csproj                   RentMaq.API/

RUN dotnet restore RentMaq.sln

# ---------------------------------------------------------------------------
# Stage 2: build — compilacion completa
# ---------------------------------------------------------------------------
FROM restore AS build
ARG BUILD_CONFIGURATION=Release

COPY src/ .
RUN dotnet build RentMaq.sln -c $BUILD_CONFIGURATION --no-restore

# ---------------------------------------------------------------------------
# Stage 3: publish — publicacion optimizada
# UseAppHost=false: genera DLL portable, no binario nativo.
# Esto permite que la misma imagen corra en amd64 y arm64 via JIT.
# ---------------------------------------------------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish RentMaq.API/RentMaq.API.csproj \
    -c $BUILD_CONFIGURATION \
    --no-build \
    -o /app/publish \
    /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Stage 4: runtime — imagen minima de produccion
# aspnet:8.0 incluye el runtime de .NET 8 y ASP.NET Core.
# Tamano: ~220 MB (vs ~800 MB del SDK).
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080 8081

# Copiar artefactos publicados
COPY --from=publish /app/publish .

# Usuario no-root (seguridad). APP_UID es definido por la imagen base de Microsoft.
USER $APP_UID

ENTRYPOINT ["dotnet", "RentMaq.API.dll"]
