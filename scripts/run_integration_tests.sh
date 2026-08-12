#!/bin/sh
set -e

# Note: the Cosmos DB emulator is started per test run by Testcontainers and served over
# plain HTTP, so it needs no certificate handling here. Only WireMock's CA is installed.
update-ca-certificates

# Note: coverlet code-coverage requires a build - source mapping
dotnet test DfE.GIAP.All/tests/DfE.GIAP.Core.IntegrationTests/DfE.GIAP.Core.IntegrationTests.csproj \
  --nologo \
  --no-restore \
  --logger "console;verbosity=diagnostic" \
  -p:CollectCoverage=true \
  -p:CoverletOutputFormat=cobertura \
  -p:Exclude="[DfE.GIAP.SharedTests*]*" \
  -p:CoverletOutput="/coverage-integration/cobertura.xml"