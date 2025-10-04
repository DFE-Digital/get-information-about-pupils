#!/bin/bash
set -e

# CosmosDb certificate

retry_count=0
max_retry_count=50

COSMOSDB_CERT_URL='https://localhost:8081/_explorer/emulator.pem'

until curl --insecure --silent --fail "$COSMOSDB_CERT_URL"; do
  if [ $retry_count -eq $max_retry_count ]; then
    echo "Cosmos DB Emulator did not become healthy in time."
    exit 1
  fi
  echo "Waiting for Cosmos DB Emulator... ($retry_count)"
  sleep 3
  retry_count=$((retry_count+1))
done

echo "Cosmos DB Emulator is healthy ..."

echo 'Outputting certificate to host certificate store ...'

curl -k "$COSMOSDB_CERT_URL" -o /usr/local/share/ca-certificates/cosmos-db-emulator.crt


# Resolve absolute path to the directory containing this script

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

## WireMock certificate

WIREMOCK_SCRIPT_PATH="$SCRIPT_DIR/generate_wiremock_server_cert.sh"
chmod +x "$WIREMOCK_SCRIPT_PATH" && /bin/bash -c $WIREMOCK_SCRIPT_PATH

update-ca-certificates

# Run integration tests using absolute path
 dotnet test "$SCRIPT_DIR/../DfE.GIAP.All/tests/DfE.GIAP.Core.IntegrationTests/DfE.GIAP.Core.IntegrationTests.csproj" --no-build --no-restore --nologo --logger "console;verbosity=detailed"

#tail -f /dev/null
