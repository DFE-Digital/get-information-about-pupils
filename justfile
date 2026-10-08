set windows-shell := ["powershell.exe", "-nop", "-c"]

docker-nuget-config := env('DOCKER_NUGET_CONFIG', 'app/nuget.config')

default:
  @just --list

# Install local tools
install-tools:
  @dotnet tool restore

# Restore dependencies
[working-directory: 'app']
restore:
  @dotnet restore

[working-directory: 'app']
package *ARGS:
  @dotnet publish src/DfE.GIAP.Web/DfE.GIAP.Web.csproj {{ARGS}}

# Build the Docker image using an authenticated NuGet config
docker-build *ARGS:
  @docker build app --file app/Dockerfile --secret "id=nuget_config,src={{docker-nuget-config}}" {{ARGS}}
