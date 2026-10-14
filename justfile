export DOTNET_WATCH_SUPPRESS_MSBUILD_INCREMENTALISM := 'true'
export DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER := 'true'
export DOTNET_WATCH_RESTART_ON_RUDE_EDIT := 'true'

set windows-shell := ["powershell.exe", "-nop", "-c"]

docker-nuget-config := env('DOCKER_NUGET_CONFIG', 'app/nuget.config')
user-secrets-id := 'd4e9d00c-c90a-4862-a2de-9e169b13be02'

default:
  @just --list

# Install local tools
install-tools:
  @dotnet tool restore

# Restore dependencies
[working-directory: 'app']
restore:
  @dotnet restore

# Run the GIAP web project and watch for file changes
[working-directory: 'app/src/DfE.GIAP.Web']
watch-giap-web:
  @dotnet watch

# Set a configuration entry in user secrets for the web application
set-secret key value:
  @dotnet user-secrets set "{{key}}" "{{value}}" --id {{user-secrets-id}}

[working-directory: 'app']
package *ARGS:
  @dotnet publish src/DfE.GIAP.Web/DfE.GIAP.Web.csproj {{ARGS}}

# Build the Docker image using an authenticated NuGet config
docker-build *ARGS:
  @docker build app --file app/Dockerfile --secret "id=nuget_config,src={{docker-nuget-config}}" {{ARGS}}
