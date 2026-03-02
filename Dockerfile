# =====================================================
# Stage 0: Set version arguments
# =====================================================
ARG DOTNET_VERSION=8.0
ARG NODEJS_VERSION_MAJOR=24

# =====================================================
# Stage 1: Build frontend assets (DfE / GOV.UK)
# =====================================================
FROM node:${NODEJS_VERSION_MAJOR}-bookworm-slim AS assets
WORKDIR /app

COPY DfE.GIAP.All/src/DfE.GIAP.Web/package*.json /app/Web/
COPY DfE.GIAP.All/src/DfE.GIAP.Web/gulpfile.js /app/Web/
COPY DfE.GIAP.All/src/DfE.GIAP.Web/Scripts /app/Web/Scripts/
COPY DfE.GIAP.All/src/DfE.GIAP.Web/Styles /app/Web/Styles/
COPY DfE.GIAP.All/src/DfE.GIAP.Web/wwwroot /app/Web/wwwroot/

RUN cd /app/Web && npm i -g gulp && npm ci
RUN gulp default --gulpfile /app/Web/gulpfile.js --cwd /app/Web

# =====================================================
# Stage 2: Build Web project
# =====================================================
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

ARG PROJECT_PATH="DfE.GIAP.All"

# Copy only what's needed for restore so that it can be cached
COPY ${PROJECT_PATH}/Directory.Build.props src/
COPY ${PROJECT_PATH}/Directory.Packages.props src/
COPY ${PROJECT_PATH}/src/DfE.GIAP.Core/DfE.GIAP.Core.csproj src/DfE.GIAP.Core/
COPY ${PROJECT_PATH}/src/DfE.GIAP.Web/DfE.GIAP.Web.csproj src/DfE.GIAP.Web/

COPY ${PROJECT_PATH}/src src/

# Use BuildKit to mount nuget.config and restore
RUN --mount=type=secret,id=nuget_config \
    dotnet restore src/DfE.GIAP.Web/DfE.GIAP.Web.csproj \
    --configfile /run/secrets/nuget_config

RUN dotnet build src/DfE.GIAP.Web/DfE.GIAP.Web.csproj -c $BUILD_CONFIGURATION -o /app/build

# =====================================================
# Stage 3: Publish Web .DLL
# =====================================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish src/DfE.GIAP.Web/DfE.GIAP.Web.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# =====================================================
# Stage 4: Run Web
# =====================================================
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-noble AS final
WORKDIR /app

COPY --from=publish /app/publish .

COPY --from=assets /app/Web/wwwroot wwwroot/

# .NET 8+ container images no longer listen on Kestrel defaults (5000) but 8080
EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "DfE.GIAP.Web.dll"]