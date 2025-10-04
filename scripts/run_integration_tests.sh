#!/bin/bash
set -e

# Resolve absolute path to the directory containing this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

retry_count=0
max_retry_count=50

until curl --insecure --silent --fail "https://localhost:8081/_explorer/emulator.pem"; do
  if [ $retry_count -eq $max_retry_count ]; then
    echo "Cosmos DB Emulator did not become healthy in time."
    exit 1
  fi
  echo "Waiting for Cosmos DB Emulator... ($retry_count)"
  sleep 5
  retry_count=$((retry_count+1))
done

echo "Cosmos DB Emulator is healthy ..."

echo "Getting certificate ..."

curl -k https://localhost:8081/_explorer/emulator.pem -o /usr/local/share/ca-certificates/cosmos-db-emulator.crt

## WireMock certificate

chmod +x "$SCRIPT_DIR/generate_wiremock_server_cert.sh"
"$SCRIPT_DIR/generate_wiremock_server_cert.sh"

update-ca-certificates

# Run integration tests using absolute path
 dotnet test "$SCRIPT_DIR/../DfE.GIAP.All/tests/DfE.GIAP.Core.IntegrationTests/DfE.GIAP.Core.IntegrationTests.csproj" --no-build --no-restore --nologo --logger "console;verbosity=detailed"

#tail -f /dev/null
