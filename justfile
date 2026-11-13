export DOTNET_WATCH_SUPPRESS_MSBUILD_INCREMENTALISM := 'true'
export DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER := 'true'
export DOTNET_WATCH_RESTART_ON_RUDE_EDIT := 'true'

set windows-shell := ["powershell.exe", "-nop", "-c"]

docker-nuget-config := env('DOCKER_NUGET_CONFIG', 'nuget.config')
user-secrets-id := 'd4e9d00c-c90a-4862-a2de-9e169b13be02'

default:
  @just --list

# Install local tools
install-tools:
  @dotnet tool restore

# Restore dependencies
restore:
  @dotnet restore

# Run the GIAP web project and watch for file changes
[working-directory: 'src/DfE.GIAP.Web']
watch-giap-web:
  @dotnet watch

# Run the GIAP Worker project and watch for file changes
[working-directory: 'src/DfE.GIAP.Worker']
watch-giap-worker:
  @dotnet watch

# Set a configuration entry in user secrets for the web application
set-secret key value:
  @dotnet user-secrets set "{{key}}" "{{value}}" --id {{user-secrets-id}}

package *ARGS:
  @dotnet publish src/DfE.GIAP.Web/DfE.GIAP.Web.csproj {{ARGS}}

# Build the Docker image using an authenticated NuGet config
docker-build *ARGS:
  @docker build . --file Dockerfile --secret "id=nuget_config,src={{docker-nuget-config}}" {{ARGS}}

# Run the EF Core Command-line Tools for the Core project
[working-directory: 'src/DfE.GIAP.Core']
ef *ARGS:
  @dotnet dotnet-ef {{ARGS}}
