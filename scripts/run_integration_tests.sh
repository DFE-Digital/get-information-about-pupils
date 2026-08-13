#!/bin/sh
set -e

# Note: no certificate handling is needed. The Cosmos DB emulator is started per test run by
# Testcontainers and served over plain HTTP, and the Azure Search index is stubbed in-process.

# Note: coverlet code-coverage requires a build - source mapping
dotnet test DfE.GIAP.All/tests/DfE.GIAP.Core.IntegrationTests/DfE.GIAP.Core.IntegrationTests.csproj \
  --nologo \
  --no-restore \
  --logger "console;verbosity=diagnostic" \
  -p:CollectCoverage=true \
  -p:CoverletOutputFormat=cobertura \
  -p:Exclude="[DfE.GIAP.SharedTests*]*" \
  -p:CoverletOutput="/coverage-integration/cobertura.xml"
