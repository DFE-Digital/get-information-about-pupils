#!/bin/sh
set -e

# Retrieve CosmosDb TLS certificate
retry_count=0
max_retry_count=30

COSMOSDB_CERT_URL='https://localhost:8081/_explorer/emulator.pem'
COSMOSDB_OUTPUT_CERTIFICATE_PATH='/usr/local/share/ca-certificates/cosmos-db-emulator.crt'

until curl --insecure --silent --fail "$COSMOSDB_CERT_URL"; do
  if [ $retry_count -eq $max_retry_count ]; then
    echo "[ERROR]: Cosmos DB Emulator did not become healthy in time."
    exit 1
  fi
  echo "[DEBUG]: Waiting for Cosmos DB Emulator on $COSMOSDB_CERT_URL attempt: ($retry_count)"
  sleep 5
  retry_count=$((retry_count+1))
done

curl --insecure "$COSMOSDB_CERT_URL" -o "$COSMOSDB_OUTPUT_CERTIFICATE_PATH"

if [ ! -f "$COSMOSDB_OUTPUT_CERTIFICATE_PATH" ]; then
  echo "[ERROR]: Failed to download Cosmos DB certificate."
  exit 2
fi

echo '[DEBUG]: CosmosDb Certificate done'
echo "[DEBUG]: Cosmos DB Emulator is healthy"

# Host certificates update - WireMock and CosmosDb
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
