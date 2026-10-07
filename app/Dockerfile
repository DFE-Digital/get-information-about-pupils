FROM mcr.microsoft.com/dotnet/sdk:10.0.302-alpine3.23@sha256:d8ee39817ca03a3757288e83c37ed73cc969a286c603b827c7cbe33add1c2d1c AS build
WORKDIR /source

RUN apk add --no-cache gcompat

COPY . ./
RUN --mount=type=secret,id=nuget_config,target=/source/nuget.config,required=true \
    dotnet restore src/DfE.GIAP.Web/DfE.GIAP.Web.csproj --configfile /source/nuget.config
RUN dotnet publish src/DfE.GIAP.Web/DfE.GIAP.Web.csproj \
    --no-restore --configuration Release --output /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine3.23@sha256:1201dde897ab436b7c6b386f6dbd4f9a3ca0245f9c5a8aac8f8bcdccb4c7d484
ARG GIT_SHA
WORKDIR /app

COPY --from=build /publish/ .

RUN apk add --no-cache tzdata icu-data-full icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV GIT_SHA=${GIT_SHA}
ENV ASPNETCORE_HTTP_PORTS=3000

USER $APP_UID

ENTRYPOINT ["dotnet", "DfE.GIAP.Web.dll"]
