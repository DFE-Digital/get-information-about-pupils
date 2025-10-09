# Integration tests

The integration tests run at build by standing up HTTP dependencies that are as-close substitutes to the real dependencies. Giving us confidence that the application is

The tests

- Have to run in CI on Linux
- Have to be able to be debuggable locally

## Prerequisites

- [Onboarding](https://github.com/DFE-Digital/get-information-about-pupils-wiki/tree/main/onboarding/README.md)

## Running the tests

Follow the steps as per [CI]()

1) build the test image
2) docker-compose up services and run the tests

