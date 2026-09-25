# Integration tests

The integration tests run at build by standing up HTTP dependencies that are as-close substitutes to the real dependencies. Giving us confidence that the application is

The tests

- Have to run in CI on Linux
- Have to be debuggable locally

## Prerequisites

- [Onboarding](https://github.com/DFE-Digital/get-information-about-pupils-wiki/tree/main/onboarding/README.md)

## Related documentation

- [Search index stub](./search-index-stub.md)

## Running the tests

Tests that stub their dependencies in-process — such as the search tests — need no infrastructure
at all:

```sh
dotnet test app/tests/DfE.GIAP.Core.IntegrationTests/DfE.GIAP.Core.IntegrationTests.csproj --filter "FullyQualifiedName~SearchByKeyWords"
```

Tests in the `CosmosDbIntegrationTests` collection need the Cosmos DB emulator. The test fixture
starts it on demand via [Testcontainers](https://dotnet.testcontainers.org/), so the only
prerequisite is a running container engine (Docker Desktop, Colima, Podman) reachable by the
current user. `dotnet test` then runs the whole suite as usual:

```sh
dotnet test app/tests/DfE.GIAP.Core.IntegrationTests/DfE.GIAP.Core.IntegrationTests.csproj
```

## Note: Restoring from the private DFE-DIGITAL feed

As we have some NuGet packages referenced in the GitHub DFE-DIGITAL feed. We must produce a PAT token when restoring enabling access to the feed, and the package.

We also must provide a `package-source mapping` else restoring in CI fails. // **TODO understand this failure better**

This means we have to provide a `nuget.config` than using `dotnet nuget source` part of the `dotnet cli` to achieve a restore

This means passing `--configfile` to point at the nuget.config whenever we `dotnet restore`, as
[CI](../../.github/actions/setup/action.yml) does.