# Verification

We only switch over to the new data pipeline once what it writes has been proven to match what the old pipeline
writes. The verification tool (`src/DfE.GIAP.Verification`) compares the two and reports every difference, so each one
can be fixed or explained and accepted.

It's a console app shipped in the Docker image as `/apps/verify`. It's run in the Worker pod, which can already reach
Postgres and has the configuration it needs.

## What's compared

What the new pipeline wrote is the "new" side and what the old pipeline wrote is the "old" side.

| Check | New | Old |
| --- | --- | --- |
| `records` | `npd_pupils` and `npd_pupil_records` | Cosmos DB `pupil-noskill` |
| | `pupil_premium_pupils` | Cosmos DB `pupil-premium-v2` |
| | `further_education_learners` and `further_education_learner_records` | Cosmos DB `further-education-v2` |
| `data-files` | Each data file's latest `ingest_runs` row | The document the old pipeline writes to the Cosmos DB `reference` container for each data file |
| | `ingest_rejected_rows` | `giap_exceptions_<run ID>.csv` in `<folder>/report/` of the `giapdataprocessing` container |
| `downloads` | The `downloads` container | The `giapdownloads` container |

GIAS reference data isn't compared.

### Records

Each pupil (by `PupilMatchingRef`) or learner (by `ULN`) is compared in both directions: whether it's on each side,
every field of it, and every value of every row of every dataset. The fields and datasets are named as the old
pipeline names them, e.g. `Surname` and `KS2`. Cosmos DB's own fields (`id` and those starting `_`) aren't compared.

A dataset's rows are compared as a multiset. Rows that match are paired first; the rows left over are paired with the
row they have the most values in common with, so a wrong value is reported as that value rather than as a missing row.
Rows in a different order are reported separately, as `row-order`.

### Values

The new pipeline keeps values as the strings in the data file, but the old pipeline wrote the types Spark inferred. So a
value is either equal, equivalent or different:

- **Equivalent**: the values only differ in their type, e.g. `"007"` and `7` (`number-equivalent`), `"true"` and
  `true` (`boolean-equivalent`), or `"2010-03-01"` and `"2010-03-01T00:00:00.000Z"` (`date-equivalent`). Slashed dates
  are read day-first, so a date Spark read month-first is different, not equivalent.
- **Different** (`value-different`): anything else.

A missing field and a `null` are the same. A field that's only on one side is `field-missing-in-new` or
`field-missing-in-old`.

### Data files

For each data file, the latest result on each side is compared: whether it succeeded (the old pipeline only writes a
`reference` document for a data file that succeeded) and, if both succeeded, the counts of source rows, rejected rows,
rows that passed and unique records (`records_written` on the new side). Children looked after files aren't compared, as
the old pipeline only split them into prepared downloads.

The rejected rows are compared as a multiset, by their values; the reasons aren't compared, as the pipelines word them
differently. The old pipeline's file doesn't have the row numbers. It's found from the old pipeline's run ID, if it's
given with `--legacy-run-id`, or else as the only file in the folder last modified while the data file was being
processed, according to its `reference` document. If neither finds it, the data file's rejected rows are reported as
`not-compared`.

### Prepared downloads

Each file is compared by path: whether it's on each side, its BOM, encoding and line endings, its header, and its rows
as a multiset.

## Running it

```sh
kubectl exec -n <namespace> deploy/<worker deployment> -- \
  dotnet /apps/verify/DfE.GIAP.Verification.dll compare --scope keys --file ks2/NPD_KS2_2025_U.zip
```

| Option | |
| --- | --- |
| `--scope keys` | Only compare what's in the zips given with `--file`: the pupils and learners in their data files (including rows that were rejected), their data files, and prepared downloads changed since the first of them was uploaded. Use this in test, where there's other data. |
| `--scope full` | Compare everything on both sides. |
| `--file <path>` | A zip, as its path in `ingest_files`, e.g. `ks2/NPD_KS2_2025_U.zip`. Can be repeated. |
| `--legacy-run-id <data file name>=<run ID>` | The old pipeline's Data Factory run ID for a data file, to find its rejected rows. Can be repeated. |
| `--check records\|data-files\|downloads` | Only run these checks. Can be repeated. |
| `--detail` | Also write every difference, with its values, to `details.csv`. |
| `--batch-size`, `--concurrency` | How many records to read at a time (default 250), and how many batches at once (default 2). |
| `--max-ru <n>` | The most Cosmos DB request units to use a second. Set this in production, and run out of hours, so the web app keeps the throughput it needs. Throttled requests are also retried for as long as Cosmos DB says to wait. |
| `--output <folder>` | Also write the report to a local folder. |
| `--rules <path>` | Use a different accepted differences file. |

It exits with `0` if there are no differences that haven't been accepted, `1` if there are, and `2` if it couldn't run.

## Configuration

The tool reads the same configuration as the Worker, plus:

| Key | |
| --- | --- |
| `RepositoryOptions:EndpointUri`, `RepositoryOptions:PrimaryKey` | The Cosmos DB account, as for the web app. The database and container names are in the tool's `appsettings.json`. |
| `BlobStorageOptions` | The old downloads storage account, as for the web app. Locally, `Verification:LegacyDownloadsStorage` can be a connection string instead, e.g. for Azurite. |
| `Verification:LegacyProcessingStorage` | A SAS URL with read and list permissions for the old pipeline's `giapdataprocessing` container. Locally, it can be a connection string. |

The old storage accounts aren't managed by Terraform, so `Verification--LegacyProcessingStorage` is added to the `app`
Key Vault by hand, in test and production only. It reaches the Worker pod on its next deploy.

## The report

The report is written to the `verification` container of the ingest storage account, in `reports/<time>/`:

- `summary.md`: the differences grouped by comparison, dataset, field and kind, with a count and up to 20 example keys
  for each, and how many records, data files, rejected rows and files were compared. It's also written to standard
  output.
- `summary.json`: the same, as JSON.
- `details.csv`: every difference with its values, with `--detail`.

The summary has no values, only keys, so it's safe to show in a GitHub workflow's job summary. `details.csv` has values,
which in production are personal data; it stays in the production storage account.

## Accepting differences

A difference that's been explained is accepted with a rule in `src/DfE.GIAP.Verification/accepted-differences.json`.
Accepted differences are still in the report, with the reason, but don't fail the comparison.

```json
{
    "kind": "number-equivalent",
    "comparison": "downloads",
    "dataset": "*",
    "field": "UPN",
    "newValue": "\"0\\d+\"",
    "reason": "Values aren't converted to the types Spark inferred, e.g. a value with leading zeros keeps them.",
    "docs": "docs/file-ingestion.md#prepared-downloads"
}
```

`kind`, `reason` and `docs` are required. `comparison`, `dataset` and `field` can be omitted or `*` to match anything.
`newValue` and `oldValue` are regular expressions the whole value, as JSON, must match. Every accepted difference must
be documented, with `docs` linking to where; for a deliberate difference from the old pipeline, that's the
[file ingestion](file-ingestion.md) docs.

The comparisons are `pupil-noskill`, `pupil-premium`, `further-education`, `file-results`, `rejected-rows` and
`downloads`. The kinds are listed in `DifferenceKinds`.

## Anonymising production files

Most verification happens in the test environment, by replaying production's data files through both pipelines. Test
can't have personal data, so the files are anonymised first with the tool's `anonymise` command. It reads the zips the
old pipeline kept in its production `giapdataprocessing` container (`<folder>/raw/` and `<folder>/processed/`) and
writes anonymised copies to the `files/` folder of the test environment's `verification` container, with the same zip
and data file names.

It's run by a named person, on a DfE-managed device, with their own Azure CLI login. The real files are streamed from
production into the anonymised zips in memory and are never written to disk. A zip isn't committed to the `verification`
container until it's finished, so an interrupted run leaves nothing behind.

### Access

With `--key-vault`, the tool reads three secrets from the production `app` Key Vault (`s189p01-giap-pd-app-kv`), which
needs the Key Vault Secrets User role, activated through PIM. They're only held in memory.

| Secret | |
| --- | --- |
| `Verification--LegacyProcessingStorage` | The read and list SAS for `giapdataprocessing` that `compare` already uses. |
| `Verification--AnonymisedFilesStorage` | A SAS URL for the test environment's `verification` container (`s189t01giaptsingestsa`), with create and write permissions only. Added by hand. |
| `Verification--AnonymisationKey` | 32 random bytes, base64-encoded, e.g. from `openssl rand -base64 32`. Added by hand, and only ever in production. |

The tool refuses to write anywhere but a `verification` container in another storage account that isn't production's.
Without `--key-vault` it reads the three from configuration (e.g. User Secrets), but only if both storage accounts are
Azurite, so a key outside the Key Vault can't be used with real files.

Every fake value is derived from the key, so changing it changes every fake identifier, name and DOB. Regenerate all of
the files with `--overwrite` if it ever changes.

### Steps

1. List the zips to choose from. Only their names, sizes and dates are listed.

   ```sh
   dotnet run --project src/DfE.GIAP.Verification -- list-sources --key-vault s189p01-giap-pd-app-kv
   ```

2. Write the manifest: the zips to verify with, in the order they're to be uploaded, and their ingest file paths.

   ```json
   {
       "files": [
           { "source": "ks2/processed/NPD_KS2_2025_U.zip", "target": "ks2/NPD_KS2_2025_U.zip" },
           { "source": "cla/ks2/processed/GIAP_CLA_KS2_2025.zip", "target": "cla-ks2/GIAP_CLA_KS2_2025.zip" }
       ]
   }
   ```

3. Describe the zips' data files, to classify their columns. This lists each data file's columns, how each is
   classified, and for each identifier column the shapes of its values (e.g. `A999999999999`) and how many start with
   each character. It has no values.

   ```sh
   dotnet run --project src/DfE.GIAP.Verification -- anonymise --manifest <manifest> --list-columns --key-vault s189p01-giap-pd-app-kv
   ```

4. Classify every column in `src/DfE.GIAP.Verification/anonymisation.json` (see below), and choose the reserved ranges
   for PMRs from the shapes.

5. Anonymise them. A zip that's already in the `verification` container is left as it is, unless `--overwrite` is given;
   that's only found out once the zip has been anonymised, so to resume an interrupted run quickly, take the finished zips
   out of the manifest. The manifest is written to `files/manifest.json` for the verification workflow.

   ```sh
   dotnet run --project src/DfE.GIAP.Verification -- anonymise --manifest <manifest> --key-vault s189p01-giap-pd-app-kv
   ```

6. In the test environment's Worker pod, before the files are first replayed, find the fake keys that are already in
   test. Fake ULNs can't have a reserved range (see below), so a few will match the junk already in test. The keys are
   written to `files/collisions.json`, and the verification workflow's reset deletes those pupils and learners from both
   sides before the replay.

   ```sh
   kubectl exec -n <namespace> deploy/<worker deployment> -- dotnet /apps/verify/DfE.GIAP.Verification.dll check-collisions
   ```

### The classification

`anonymisation.json` classifies every column of every folder's data files. A data file with a column that isn't
classified isn't anonymised, and the run stops, naming the column; so personal data in a column nobody has looked at
can't reach test.

```json
{
    "identifiers": {
        "pmr": [{ "source": "[0-9A-F]{18}", "target": "[G-V][0-9A-F]{17}" }]
    },
    "folders": {
        "ks2": {
            "subject": "PupilMatchingRef",
            "sex": "GENDER",
            "transform": { "PupilMatchingRef": "pmr", "UPN": "upn", "SURNAME": "surname", "FORENAMES": "forename", "DOB": "dob" },
            "keep": ["ACADYR", "GENDER", "LA", "ESTAB", "URN"]
        }
    }
}
```

Column names are matched case-insensitively. `subject` is the PMR or ULN column: a pupil's fake names and DOB are chosen
from their real identifier, so they're the same in every file. `sex` is used to choose fake forenames.

| Transform | |
| --- | --- |
| `keep` | Left as it is. |
| `pmr` | Permuted into the reserved range for its format (see below). A PMR in none of the formats stops the run. |
| `upn` | Permuted into another UPN with a valid check letter; a temporary UPN stays temporary. |
| `uln` | Permuted into another valid ULN. |
| `candidate-number` | Permuted into another number of the same length. |
| `guid` | Permuted into another GUID, keeping its case and hyphens. |
| `forename`, `middle-names`, `surname` | Each word is replaced with a name from `Anonymisation/Names` (the English names from the Bogus library), keeping the case and the spaces and hyphens between them. Never the real name. |
| `dob` | Moved to another day in the same month and year, in the same format. |
| `postcode` | Cut to its outward code, e.g. `SW1A 1AA` to `SW1A`; anything that isn't a postcode is blanked. |
| `address`, `blank` | Blanked. |

Empty values and `NULL` are left as they are. A UPN, ULN, candidate number or GUID that isn't valid is replaced with
characters of the same kinds (a digit for a digit, a letter for a letter), and never gets a valid check character, so
a row the pipelines reject for it is still rejected. A DOB that isn't a date is left as it is, for the same reason.
Days 1 to 12 of a DOB stay in 1 to 12, and later days stay after 12, because the old pipeline read slashed dates
month-first: a date it could read stays one it can read.

Only the bytes of the values that are replaced change. The BOM, quoting, line endings, every other column, and
everything else in the zip, such as metadata documents, are copied exactly. As a last check, a row that would still
have a real identifier or name in any of its columns stops the run.

### Fake identifiers

Identifiers are permuted with a keyed format-preserving permutation (a Feistel network with HMAC-SHA256), so two real
identifiers never get the same fake one, and the same identifier gets the same fake one in every file and year.

Fake PMRs are in a reserved range, so they can't be the same as a PMR already in test: each PMR format has a target
format with at least as many values that no real PMR can match, e.g. hex PMRs starting with `G` to `V`. The tool
refuses a classification where a target range is smaller than its format, or overlaps any real format or another
target.

Fake ULNs can't be in a reserved range. A ULN is 9 digits and a check digit, and real ULNs start with every digit
from 1 to 9, so there's no range left with room for them all. They're permuted among all the valid ULNs instead, and
the few that match learners already in test are found by `check-collisions`. UPNs don't need a reserved range: they
aren't a key in either pipeline.
