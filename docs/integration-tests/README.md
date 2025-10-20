# Integration tests

The integration tests run at build by standing up HTTP dependencies that are as-close substitutes to the real dependencies. Giving us confidence that the application is

The tests

- Have to run in CI on Linux
- Have to be debuggable locally

## Prerequisites

- [Onboarding](https://github.com/DFE-Digital/get-information-about-pupils-wiki/tree/main/onboarding/README.md)

## Related documentation

- [CosmosDb emulator](./cosmosdb-docker-emulator.md)
- [Wiremock](./wiremock.md)

## Running the tests

Currently the method to execute the tests uses `docker compose`

Follow the steps in [CI](../../.github/workflows/web-application-cicd.yml)

1) Update the nuget.config in src with a valid NUGET_USERNAME and NUGET_PAT
2) docker build the test-runner image
3) docker-compose up services and runs the tests

## Note: Restoring from the private DFE-DIGITAL feed

As we have some NuGet packages referenced in the GitHub DFE-DIGITAL feed. We must produce a PAT token when restoring enabling access to the feed, and the package.

We also must provide a `package-source mapping` else restoring in CI fails. // **TODO understand this failure better**

This means we have to provide a `nuget.config` than using `dotnet nuget source` part of the `dotnet cli` to achieve a restore

This is shown in the building of the docker-image when restoring packages by

- Mounting a secret in using docker buildkit with `docker build`
- Passing `--config-file` to the nuget.config when needing to `dotnet restore`

## Issue: Using git bash for Windows and `docker exec -it /bin/bash`

When using [`git for windows`](https://github.com/git-for-windows/git) it rewrites linux paths to Windows. So commands passing linux paths `/bin/sh` as below fail;

```sh
 docker exec -it /bin/sh 8444
```

Use Powershell instead for these specfic commands