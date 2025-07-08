FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DfE.GIAP.Web/DfE.GIAP.Web.csproj", "DfE.GIAP.Web/"]
COPY ["DfE.GIAP.Common/DfE.GIAP.Common.csproj", "DfE.GIAP.Common/"]
COPY ["DfE.GIAP.Core/DfE.GIAP.Domain.csproj", "DfE.GIAP.Core/"]
COPY ["DfE.GIAP.Service/DfE.GIAP.Service.csproj", "DfE.GIAP.Service/"]
RUN dotnet restore "../DfE.GIAP.Web/DfE.GIAP.Web.csproj"
COPY . .
WORKDIR "/src/DfE.GIAP.Web"
RUN dotnet build "./DfE.GIAP.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DfE.GIAP.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DfE.GIAP.Web.dll"]
