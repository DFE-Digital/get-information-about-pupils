set windows-shell := ["powershell.exe", "-nop", "-c"]

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
