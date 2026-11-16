FROM mcr.microsoft.com/dotnet/sdk:10.0.302-alpine3.23@sha256:d8ee39817ca03a3757288e83c37ed73cc969a286c603b827c7cbe33add1c2d1c AS build
WORKDIR /source

RUN apk add --no-cache gcompat

COPY . ./
RUN dotnet tool restore
RUN --mount=type=secret,id=nuget_config,target=/source/nuget.config,required=true \
    dotnet restore src/DfE.GIAP.Web/DfE.GIAP.Web.csproj --configfile /source/nuget.config && \
    dotnet restore src/DfE.GIAP.Worker/DfE.GIAP.Worker.csproj --configfile /source/nuget.config
RUN dotnet publish src/DfE.GIAP.Web/DfE.GIAP.Web.csproj \
    --no-restore --configuration Release --output /publish
RUN dotnet publish src/DfE.GIAP.Worker/DfE.GIAP.Worker.csproj \
    --no-restore --configuration Release --output /publish-worker
RUN --mount=type=secret,id=nuget_config,target=/source/nuget.config,required=true \
    dotnet ef migrations bundle --project src/DfE.GIAP.Core/DfE.GIAP.Core.csproj \
    --configuration Release --context GiapDbContext --output /efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine3.23@sha256:1201dde897ab436b7c6b386f6dbd4f9a3ca0245f9c5a8aac8f8bcdccb4c7d484
ARG GIT_SHA

COPY --from=build /publish/ /apps/web/
COPY --from=build /publish-worker/ /apps/worker/
COPY --from=build /efbundle /apps/efbundle

RUN apk add --no-cache tzdata icu-data-full icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV GIT_SHA=${GIT_SHA}
ENV SENTRY_RELEASE=${GIT_SHA}
ENV ASPNETCORE_HTTP_PORTS=3000

USER $APP_UID
