# =============================================================================
# RentMaq Pro — Multi-stage Dockerfile (.NET 8)
# Build context: raíz del monorepo (../)
# =============================================================================

# ---------------------------------------------------------------------------
# Stage 1: base — imagen runtime compartida
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# ---------------------------------------------------------------------------
# Stage 2: restore — capa de dependencias (cache-friendly)
# Copia solo .csproj + .sln para maximizar cache de NuGet restore.
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
# Stage 3: build — compilación completa
# ---------------------------------------------------------------------------
FROM restore AS build
ARG BUILD_CONFIGURATION=Release

COPY src/ .
RUN dotnet build RentMaq.sln -c $BUILD_CONFIGURATION --no-restore

# ---------------------------------------------------------------------------
# Stage 4: publish — publicación optimizada
# ---------------------------------------------------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish RentMaq.API/RentMaq.API.csproj \
    -c $BUILD_CONFIGURATION \
    --no-build \
    -o /app/publish \
    /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Stage 5: final — imagen de producción mínima
# ---------------------------------------------------------------------------
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Usuario no-root (seguridad)
USER $APP_UID

ENTRYPOINT ["dotnet", "RentMaq.API.dll"]
