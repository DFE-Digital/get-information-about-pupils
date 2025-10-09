# CosmosDb docker emulator

We use the CosmosDb docker emulator as part of our integration-tests at build to ensure our application fulfils behaviour with a similar database over HTTP, before we integrate and deploy

See the [flow here that installs the emulator certificate](https://github.com/DFE-Digital/get-information-about-pupils/blob/93739715c6386aa8049868ecdcf6811291b3cc93/scripts/run_integration_tests.sh#L11) at runtime on the test-runner container

## Documentation

- [Integration tests](./README.md)
- [GitHub repository for cosmosdb-emulator](https://github.com/Azure/azure-cosmos-db-emulator-docker?tab=readme-ov-file#linux-based-emulator-preview) **note** source code not available
- [Microsoft docs for cosmosdb-emulator](https://learn.microsoft.com/en-gb/azure/cosmos-db/emulator-linux)

---

## Issue: CosmosDb Certificate is regenerated everytime

This means we are required to GET the certificate at runtime (which can take up to 3 minutes) instead of mounting the certificate in as a secret to our test-runner container

- [Issue requesting](https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/230)
- **TODO** can we override the default cert via `AZURE_COSMOS_EMULATOR_CERTIFICATE`. Emulator `start.sh` implies we can if we pass ENV

Proof

```sh

# Start the emulator
docker run -d --publish 8080:8080 --publish 1234:1234 mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
# returns the container id back e.g. 308c510ff2...

# Exec into a shell
docker exec -it 308 /bin/sh

# Copy the certificate - emulators entrypoint for start.sh points to as a default-cert
cp /tmp/cosmos/appdata/default.sslcert.pfx /tmp/first_run.pfx

# Stop the container 
docker stop 308

# Start it again
docker start 308

# Copy the second certificate
cp /tmp/cosmos/appdata/default.sslcert.pfx /tmp/second_run.pfx

# Proof - Hash both files to verify they are different
sha256sum /tmp/first_run.pfx /tmp/second_run.pfx

# Proof - Stat both files and the Modified Date will also show to have changed
stat /tmp/first_run.pfx
stat /tmp/second_run.pfx
```

## Issue: CosmosDb certificate hostname limited to `localhost` as a valid hostname

This forces us to

- connect to the emulator through `localhost`
- use `networkmode:host` for container-networking

as any container trying to connect with the cosmosdb-container under another hostname e.g. via docker-networking will fail TLS validation

- **TODO** try `cosmosdbemulatormtls.localhost` as a domain to connect to when using docker-networking as it appears in SAN
- **TODO** can we use either of `/alternativenames=$EMULATOR_IP_ADDRESS,$EMULATOR_OTHER_IP_ADDRESSES` as indicated by start.sh to yield a certificate that includes other SAN?
- **TODO** try alternative provide our own self-signed certificate for the emulator to use - see regenerate everytime issue ^^
- **TODO** (unlikely) try alternative to run the tests in the cosmosdb container so it can resolve localhost
- **TODO** (unlikely) try alternative to extend the existing certificate with SAN

Proof

```sh

# Start the emulator
docker run -d --publish 8080:8080 --publish 1234:1234 mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
# returns the container id back e.g. 308c510ff2...

# wait until the certificate is returned ~ 2 mins
curl -k https://localhost:8081/_explorer/emulator.pem > cosmosdb-cert.pem

# Use openssl to inspect the certificate
 openssl x509 -in cosmos_cert.pem -text -nooout
# Example below
Certificate:
...
        Subject: CN = localhost 
            X509v3 Subject Alternative Name: critical
                DNS:308c510ff272.DOMAIN, DNS:localhost, IP Address:172.17.0.2, IP Address:127.0.0.1, IP Address:172.17.0.2, IP Address:172.17.0.2, IP Address:172.17.0.2, DNS:172.17.0.2, DNS:127.0.0.1, DNS:172.17.0.2, DNS:172.17.0.2, DNS:cosmosdbemulatormtls.localhost
            X509v3 Key Usage: critical
                Digital Signature, Key Encipherment, Certificate Sign
    Signature Algorithm: sha1WithRSAEncryption
```

## Future improvements

Resiliency against emulator image changes on the mcr registry  
Different tags appear available from the mcr repository (`latest` and `vnext-preview`), the emulator is still in its preview.
Can we mirror and cache images in GitHub container registry if a new push to MCR:latest breaks + the old MCR image on the :latest tag is removed? This'll break our builds.

1. get latest digest of mcr, latest digest of our cached image in (ghcr)
2. if newer, tag with date, push to our scoped repository
3. our compose points at our ghcr
4. wrap into a daily actions workflow

**TODO** does mcr keep older images on `:latest` e.g can we use older `sha256` digests
