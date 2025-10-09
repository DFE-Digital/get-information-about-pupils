# CosmosDb docker emulator

We use the CosmosDb docker emulator as part of our integration-tests at build to ensure our application fulfils behaviour with a similar database over HTTP, before we integrate and deploy

See the [flow here that installs the emulator certificate](https://github.com/DFE-Digital/get-information-about-pupils/blob/93739715c6386aa8049868ecdcf6811291b3cc93/scripts/run_integration_tests.sh#L11) at runtime on the test-runner container

## Improvements

Resiliency against emulator image changes;  Different tags appear available from the mcr repository (latest and vnext), the emulator is still in preview. e.g
`docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview` see [docs](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux#installation). Can we mirror and cache images in GitHub container registry if a new push to MCR breaks + the old MCR image on the :latest tag is removed?

1. get latest digest of mcr, latest digest of our cached image in (ghcr)
2. if newer, tag with date, push
3. our compose points at our ghcr
4. wrap into a daily actions workflow

---

## Issue: CosmosDb Certificate is regenerated everytime

Link related issue in GitHub
Proof with openssl

## Issue: CosmosDb certificate hostname limited to `localhost` as a valid hostname

Proof
Document need to use networkmode:host to ensure
Alternative to run the tests in the cosmosdb container so it can resolve localhost
Alternative to extend the certificate with SAN OR provide our own self-signed certificate (see regenerate everytime issue)
