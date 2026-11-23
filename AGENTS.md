# AGENTS.md

This file provides guidance to coding agents working with code in this repository.

GIAP (Get Information About Pupils) is a DfE ASP.NET Core (.NET 10) service for searching the National Pupil Database, plus a Hangfire Worker for background jobs. Issues are tracked on [DFE-Digital/teaching-record-team-project-board](https://github.com/DFE-Digital/teaching-record-team-project-board), not this repo.

## Commands

Restore needs the authenticated DfE-Digital GitHub Packages NuGet feed (see README prerequisites). Dev commands live in the `justfile` (`just` lists them); the `Makefile` is for Terraform/AKS operations only.

```sh
dotnet build
dotnet test                                                    # all test projects
dotnet test tests/DfE.GIAP.Core.UnitTests                      # one project
dotnet test tests/DfE.GIAP.Core.UnitTests --filter "FullyQualifiedName~GetMyPupilsUseCase"   # one class/test
dotnet format --verify-no-changes                              # CI lint; run `dotnet format` to fix

just install-tools                 # dotnet tool restore (dotnet-ef)
just set-secret <key> <value>      # User Secrets, shared by all projects via UserSecretsId in Directory.Build.props
just watch-giap-web                # run web app with dotnet watch (Local profile)
just watch-giap-worker             # run Worker with dotnet watch
just ef migrations add <Name>      # EF Core migration, TitleCase name, created in Core/Common/Infrastructure/Postgres/Migrations
just ef database update            # apply migrations locally (the app never migrates at startup)
```

Integration tests (`tests/DfE.GIAP.Core.IntegrationTests`) start PostgreSQL, the Cosmos DB emulator and Azurite via Testcontainers, so they need a running container engine. Search tests stub Azure Search in-process and need nothing. CI runs `Core.UnitTests` and `Web.Tests` as unit suites, integration tests separately.

`TreatWarningsAsErrors` is on for `DfE.GIAP.Core` and its test projects, so warnings break the build there.

## Consolidation programme

Most current work is part of [GIAP consolidation](https://github.com/DFE-Digital/teaching-record-team-project-board/issues/491). The end state:

- PostgreSQL is the only data store; Cosmos DB and Azure Search are removed.
- The Azure Data Factory + Databricks pipeline is replaced by Hangfire jobs in the Worker that read uploads from blob storage and write straight to Postgres.
- Web app and Worker run in AKS (Teacher Services cloud, S189 subscriptions), with IaC and CI/CD for everything.

The plan sets constraints on what you change:

1. **Run both pipelines side by side.** The old pipeline keeps writing to Cosmos and the web app keeps reading Cosmos and Azure Search until verification is clean. The new pipeline must not move, delete or claim anything the old pipeline uses, and it writes only to Postgres and its own storage account. Never write to the legacy `giapdownloads` container or reuse `BlobStorageOptions` for new-pipeline blobs.
2. **Verify against Cosmos.** Tooling will compare Postgres with the pipeline-fed Cosmos containers (`pupil-noskill`, `pupil-premium-v2`, `further-education-v2`, `reference`) and the prepared downloads. So the new pipeline matches what the old one actually writes, not what it was meant to write. Any deliberate difference must be documented in `docs/file-ingestion.md` (see the "Old pipeline / Now" tables there).
3. **Web app changes go behind feature flags** (Postgres search, reading pupil data from Postgres, migrating `news`/`application-data`/`users`/`mypupils`), so they can be switched on and back at switch-over.
4. Switch-over and decommissioning come last. Until then, Cosmos/Search code is live and must keep working.

The old pipeline's source is the `dfe-datapipeline-dbr` repo (Databricks notebooks such as `data_update_pipeline`, `processPupilPremiumFile`, `fe_data_update_pipeline`, `create_partitions`). It is the reference for behaviour being ported.

## Architecture

- `src/DfE.GIAP.Web` — MVC app (controllers under `Features/`, GOV.UK Frontend via `GovUk.Frontend.AspNetCore`, SCSS in `Styles/` compiled at build by AspNetCore.SassCompiler; no npm step). Auth is DfE Sign-in (DSI).
- `src/DfE.GIAP.Core` — all feature logic, organised by feature folder (`MyPupils`, `NewsArticles`, `Search`, `Downloads`, `Users`, `Ingest`, `PupilData`, `Gias`, …) plus `Common`. The web features follow clean-architecture layering (`Application` / `Domain` / `Infrastructure`); the newer pipeline folders (`Ingest`, `PupilData`, `Gias`) are flat. Every feature exposes a `CompositionRoot.cs` with an `Add…` extension method that `Program.cs` calls.
- Use cases implement `IUseCase<TRequest, TResponse>` / `IUseCaseRequestOnly<TRequest>` from the DfE.CleanArchitecture NuGet packages (globally imported in `Core/GlobalUsings.cs`). Each lives in `Application/UseCases/<Name>/` with its `…Request`/`…Response` types and is registered in the feature's composition root.
- `src/DfE.GIAP.Worker` — Hangfire host. Recurring jobs are registered in `Jobs/RecurringJobsRegistrar.cs`, but job classes live in `src/DfE.GIAP.Core/Jobs` so the web app's local Hangfire dashboard can load them. Hangfire uses its own `hangfire` schema in the GIAP Postgres DB (not EF-managed).
- `src/DfE.GIAP.Verification` — console app that compares what the new pipeline wrote to Postgres with what the old one wrote to Cosmos DB, for the consolidation's verification. Shipped as `/apps/verify` and run in the Worker pod; see `docs/verification.md`. When the new pipeline deliberately differs from the old one, add an accepted difference rule there as well as documenting it.

### Data stores

- **PostgreSQL** via EF Core `GiapDbContext` (`Core/Common/Infrastructure/Postgres`, with `Models/`, `Mappings/` and `Migrations/`). Connection string `ConnectionStrings:GiapDb`. New Postgres extensions must also be allowlisted on the Azure Flexible Server in `terraform/application/database.tf` (as `pg_trgm` is), or the migration fails in deployed environments.
- **Cosmos DB** and **Azure AI Search**: legacy, accessed through DfE component-library NuGet packages.
- **Blob storage**: the legacy downloads account (`BlobStorageOptions`, `giapdownloads`), and the new pipeline's ingest account (`ConnectionStrings:IngestStorage`, created in `terraform/application/storage.tf`) with `ingest`, `metadata`, `downloads` and `verification` (verification reports) containers. Locally use Azurite (`UseDevelopmentStorage=true`).

### Pupil data schema

Designed to mirror what the old pipeline writes to Cosmos:

| Source | Pupil table (key) | Records table (key) |
|---|---|---|
| NPD | `npd_pupils` (`pupil_matching_ref`) | `npd_pupil_records` (`pupil_matching_ref`, `dataset`, `academic_year`) |
| Pupil premium | `pupil_premium_pupils` (`pupil_matching_ref`) | none |
| Further education | `further_education_learners` (`uln`) | `further_education_learner_records` (`uln`, `dataset`, `academic_year`) |

Identity fields (UPN/ULN, names, DOB, sex, LA, establishment number, URN) are columns; each record's `data` is a JSONB array of the pupil's rows from the file, with values kept as strings exactly as in the file. UPN isn't unique. Name columns have `pg_trgm` GIN indexes for search. GIAS reference data lives in `local_authorities`, `establishments`, `establishment_groups` and `establishment_group_links`.

### File ingestion

`PollIngestUploadsJob` (every minute) finds zips in `<dataset>/upload/` and tracks them in `ingest_files`; `ProcessIngestFileJob` runs on a single-worker `ingest` queue under a distributed lock, with no automatic retries, and hands each data file to the `IIngestDatasetProcessor` registered for its dataset (in `PupilData/CompositionRoot.cs`). Results go to `ingest_runs` and `ingest_rejected_rows`. `RequeueFailedIngestFilesJob` is never scheduled; trigger it from the dashboard.

Patterns the processors follow:

- A reader (built on the shared `DataFileReader`: tab-separated UTF-8) validates the columns and rejects bad rows, recording why; it doesn't drop them silently. A writer then streams the rows with binary `COPY` (`BinaryCopy`) into a temp table and combines them in SQL in one transaction, so a whole file is never held in memory.
- Writing a file must be idempotent, so a replay leaves the same data.
- NPD files have a maturity rule (U < A < F): a less mature file than the one stored fails. Pupil premium rejects `A` files and older files; FE has no maturity rule.

Read `docs/file-ingestion.md` before changing any of this. It is the source of truth for the rules and the differences from the old pipeline, so update it alongside the code.

`ImportGiasDataJob` (daily, 06:00 UTC) replaces the GIAS reference data from the GIAS CSV extracts (Windows-1252). It validates both extracts fully before touching the database and replaces the data in one transaction under an advisory lock. It refuses an extract with fewer than 90% of the stored rows. Data is loaded unfiltered; consumers apply the old pipeline's filters (GOR `W`/`Z`, Pre-LGR, open MAT/SAT) at query time.

CsvHelper is pinned to 33.1.0 on purpose: 34.x is an API rewrite.

## Tests

xUnit + FluentAssertions. `Core.UnitTests` favours hand-written test doubles (`TestDoubles/`) and Moq; `Web.Tests` uses Moq and NSubstitute. Shared builders/fakes are in `tests/DfE.GIAP.SharedTests`; container fixtures in `tests/DfE.GIAP.SharedTests.Infrastructure`. Integration tests join xUnit collections (`PostgresIntegrationTestCollectionMarker`, `CosmosDbIntegrationTestCollectionMarker`) to share a container. Ingestion features are covered end to end by `*IngestTests` (Postgres + Azurite) alongside reader unit tests and writer integration tests.

## Pull requests

PR bodies follow this shape:

- Reference the board issue: `Closes DFE-Digital/teaching-record-team-project-board#<n>`, or `Part of …` when it covers only some of the issue.
- **Summary:** what changed, often with a `diff`/`text` tree or call-flow sketch, plus any deliberate differences from the old pipeline.
- **Evidence:** before/after behaviour and the tests that prove it, with test counts.
- **Merge Danger:** **Door** (one-way/two-way) and **Blast Radius**, including migrations, Terraform changes and new required configuration.

## Deployment

Deployed to AKS with Terraform (`terraform/application`). Web and Worker ship in one Docker image (`/apps/web`, `/apps/worker`); EF migrations run before rollout as a Kubernetes job using the migrations bundle (`/apps/efbundle`). `main.yml` deploys to AKS `development` then `test` on merge; `deploy.yml` is a manual AKS deploy, and `deploy-as.yml` is the manual deploy for the legacy App Service. Environment config is in `global_config/` and `terraform/application/config/`.
