# File ingestion

> [!WARNING]
> The pipeline documented here isn't live in production yet. Until the switch-over, data files are still processed
> by the old Data Factory and Databricks pipeline.

The file ingestion job processes the zips of data files uploaded to the `ingest` blob container. It runs in the Worker
(`src/DfE.GIAP.Worker`) as Hangfire jobs; the code is in `src/DfE.GIAP.Core/Ingest` and `src/DfE.GIAP.Core/Jobs`.

## Container layout

Each dataset has a folder, matching the old pipeline's: `census`, `eyfsp`, `ks1`, `ks2`, `ks4`, `mtc`, `phonics`,
`pupil-premium`, `fe_pp`, `fe_sen`, `cla-ks2` and `cla-ks4` (the old pipeline's `cla/ks2` and `cla/ks4`). Each
folder has:

| Folder | Contents |
| --- | --- |
| `<dataset>/upload/` | Zips waiting to be processed |
| `<dataset>/processed/` | Zips that were processed successfully |
| `<dataset>/error/` | Zips that failed, and duplicate uploads |

When a zip is moved, the UTC time it was moved is added to its name, e.g. `NPD_KS2_2025_U.20261120T101500Z.zip`, so
every upload of a zip is kept. A blob is never overwritten.

A zip is identified by its folder and name, e.g. `ks2/NPD_KS2_2025_U.zip`. This is the key of `ingest_files`.

## Finding uploads

`PollIngestUploadsJob` runs every minute. It lists every `<dataset>/upload/` folder and decides what to do with each
zip, oldest first:

```mermaid
flowchart TD
    start([PollIngestUploadsJob, every minute]) --> list[List the zips in every dataset's upload/ folder, oldest first]
    list --> each{{For each zip}}
    each --> seen{Path already in ingest_files?}

    seen -- No --> insert[Insert it as Pending]
    insert --> enqueue[Enqueue ProcessIngestFileJob on the ingest queue]

    seen -- Yes --> status{Its status}
    status -- Pending or Processing --> skip[Skip: it's already queued]
    status -- Succeeded --> duplicate[Duplicate: move the new upload to error/ without processing it and log a warning]
    status -- Failed --> moved{Was the failed zip moved to error/?}
    moved -- "Yes: this is a new upload" --> reset[Set it back to Pending, pointing at the new upload]
    reset --> enqueue
    moved -- "No: it failed before it could be moved" --> skip2[Skip: replay it to process it again]
```

## Processing a zip

`ProcessIngestFileJob` runs on the `ingest` queue, which has a single worker. A distributed lock means only one zip
is processed at a time, even with more than one Worker replica. Hangfire doesn't retry the job automatically.

```mermaid
flowchart TD
    start([ProcessIngestFileJob]) --> mark[Mark the file Processing and increment Attempts]
    mark --> download[Download the zip from wherever it is now to a temp file]
    download -- fails --> infra[Mark the file Failed and leave the zip where it is]
    infra --> fail([Job fails in Hangfire])

    download --> prefix{Zip name starts with the folder's prefix, e.g. NPD_KS2_?}
    prefix -- No --> zipfail[Zip failed]
    prefix -- Yes --> valid{Valid zip with at least one data file?}
    valid -- No --> zipfail

    valid -- Yes --> entries{{"For each data file in the zip (metadata documents and hidden files are skipped)"}}
    entries --> run[Start an ingest_runs row]
    run --> parse{Parse the dataset, academic year and maturity from the data file's name}
    parse -- Invalid name --> runfail[Run failed]
    parse --> folder{Is the dataset one of the folder's datasets?}
    folder -- No --> runfail
    folder -- Yes --> registered{Is a processor registered for the dataset?}
    registered -- No --> runfail
    registered -- Yes --> process[IIngestDatasetProcessor writes and commits the data file]
    process -- Throws --> runfail
    process --> rejected[Record the rejected rows in ingest_rejected_rows and the counts on the run]
    rejected --> runok[Run succeeded]

    runok --> next{More data files?}
    runfail --> next
    next -- Yes --> entries
    next -- No --> anyfailed{Did any run fail?}
    anyfailed -- Yes --> zipfail
    anyfailed -- No --> publish[Publish the zip's metadata documents to the metadata container]
    publish -- fails --> zipfail
    publish --> succeeded[Move the zip to processed/ and mark the file Succeeded]
    succeeded --> done([Job succeeds])

    zipfail --> error[Move the zip to error/ and mark the file Failed, with the reason]
    error --> fail
```

Each data file is committed separately. A zip whose data files partly fail still keeps the data that was written.

## Metadata documents

Zips usually contain documents alongside the data files that describe the dataset, such as its columns and the codes
used in them. Any `.xlsx`, `.doc` or `.docx` file in a zip is a metadata document; it isn't processed as a data file.

Users read these documents on the Glossary page (`GlossaryController`), which lists them newest first and lets users
download them. It currently lists the documents the old pipeline publishes to `AllUsers/Metadata/` in the
`giapdownloads` container of the old downloads storage account. When the new pipeline goes live, the Glossary page
will need to read from the new pipeline's `metadata` container instead.

Once every data file in a zip has succeeded, `IngestMetadataPublisher` copies the zip's metadata documents to the
`metadata` container, in the same storage account as the `ingest` container. They're named as they are in the zip,
without any folders. A document with the same name as one that's already there replaces it, as it did in the old
pipeline. Hidden files such as macOS's `__MACOSX/` and `._` files are skipped.

- A zip with a failed data file doesn't publish its documents.
- If publishing a document fails, the zip fails and is moved to `error/`. The data files have already been committed.
- Replaying a zip publishes its documents again.

## Prepared downloads

Some datasets are also split into a CSV file for each LA, school or trust, which users download from the Prepared
Downloads page (`PreparedDownloadsController`). The page maps the user's organisation to a folder
(`BlobStoragePathContext`), lists the files in it and shows each one's last modified time as its date. It currently
lists the files the old pipeline writes to the `giapdownloads` container of the old downloads storage account. When the
new pipeline goes live, the page will need to read from the new pipeline's `downloads` container instead, and the
files already in `giapdownloads` will need to be copied across so users keep their history.

`PreparedDownloadWriter` writes the files to the `downloads` container, in the same storage account as the `ingest`
container, with the old pipeline's layout:

| Folder | Contents |
| --- | --- |
| `LA/<LA code>/` | Files for an LA |
| `School/<URN>/` | Files for a school |
| `MAT/<group UID>/` | Files for a multi-academy trust |
| `SAT/<group UID>/` | Files for a single-academy trust |

A file replaces any file with the same name, so replaying a data file writes the same files again. Files with other
names, such as earlier years, are kept, so each folder is a history of the files the organisation has been sent. A
file is never deleted, even if a later data file has no rows for its organisation.

The rows are streamed into a temporary table and grouped into files in Postgres, so a data file is never held in
memory. A trust's rows are found by joining each row's URN to the GIAS data in `establishment_groups` and
`establishment_group_links`, as it is when the data file is processed: a trust is an open group (`GroupStatusCode`
`OPEN`) of type 6 (MAT) or 10 (SAT), and its rows are those of every establishment linked to it. An establishment
linked to more than one trust is in each trust's file.

Files are UTF-8 without a BOM, with `\n` line endings. Values are written as they are in the data file (trimmed), and
only quoted if they contain a comma, quote or line break; an empty value is empty. A file's rows are in the order
they're in the data file. A row with no LA, URN or other key isn't written to that kind of file, and a warning with
the number of rows is logged.

These are the deliberate differences from the old pipeline, which wrote the files with Spark:

- Rows are in data file order; Spark wrote them in whatever order its partitions came back in.
- Values aren't converted to the types Spark inferred. E.g. a value with leading zeros keeps them, and a date that
  isn't reformatted is written as it is in the data file.
- Rows with no key are left out, rather than written to a `__HIVE_DEFAULT_PARTITION__` folder.

## NPD datasets

`NpdDatasetProcessor` (in `src/DfE.GIAP.Core/PupilData`) processes the NPD data files: census (autumn, spring and
summer), EYFSP, phonics, KS1, KS2, KS4 and MTC. It replaces the old pipeline's `data_update_pipeline` notebook, and
writes to `npd_pupil_records` and `npd_pupils` through `NpdDataWriter`.

### Reading the file

Data files are always read as tab-separated UTF-8, whatever their extension, with `"` quoting. Each dataset's
identifying columns (PupilMatchingRef, LA, Estab, URN, UPN, surname, forename, middle names (census only), sex, DOB and
academic year) are listed in `NpdDatasetDefinition`, ported from the old pipeline's `dictionary_mapping_to_master`.
Column names are matched case-insensitively.

Every column in a row is stored in the record's `data`, not just the identifying ones:

- Values are trimmed and stored as strings exactly as they are in the file. An empty value is `null`; anything else,
  including the text `NULL`, is kept.
- Dots in column names are replaced with underscores, as the old pipeline did.
- `DOB`, and `EntryDate` if the file has it, are written as `yyyy-MM-ddT00:00:00.000Z`. An `EntryDate` that isn't a
  valid date is written as `null`.
- A row with no academic year keeps `null` in `data`, but its record is stored under the file's academic year.

A pupil with more than one row in a file (e.g. a dual-registered pupil in the census) gets a single record whose `data`
is an array of their rows in file order. The record's identifying fields come from their first row.

### Dates

Dates must be `yyyy-MM-dd` (optionally with a time) or day-first `dd/MM/yyyy` (optionally with a time). The old
pipeline parsed slashed NPD dates month-first, so `01/02/2015` was 2 January; it's now 1 February.

### Rejected rows and failed files

| Problem | Outcome |
| --- | --- |
| The file is empty, or is missing one of the dataset's identifying columns | The data file fails |
| The file has two columns with the same name | The data file fails |
| A row has no PupilMatchingRef | The row is rejected |
| A row's DOB isn't a valid date | The row is rejected |
| A row's academic year isn't the one in the file's name | The row is rejected |
| Every row is rejected, or there are no rows | The data file fails, with the first rejected row's reasons |
| The stored data for the dataset and academic year is from a more mature file | The data file fails |

Rejected rows are recorded in `ingest_rejected_rows`. There's no limit on how many rows can be rejected.

### Writing the records

A data file replaces all the records for its dataset and academic year:

- Files are ranked by maturity: unamended (`U`, or no maturity in the name), then amended (`A`), then final (`F`). A file
  is written if it's at least as mature as the data stored for its dataset and academic year.
- A pupil that has a record for the dataset and academic year but isn't in the file loses that record, and a pupil
  left with no records is removed.
- The identifying fields on `npd_pupils` come from a census record if the pupil has one, then from the latest academic
  year, then from the record that was loaded first.

The rows are streamed into a temporary table and combined in Postgres, so a whole census file is never held in memory.
Writing the same file again leaves the same data, so replaying a file is safe.

## Pupil premium

`PupilPremiumDatasetProcessor` (in `src/DfE.GIAP.Core/PupilData`) processes pupil premium data files. It replaces the
old pipeline's `processPupilPremiumFile` notebook, and writes to `pupil_premium_pupils` through
`PupilPremiumDataWriter`. It then writes the [prepared downloads](#pupil-premium-prepared-downloads), replacing the
old pipeline's `giapPupilPremiumSplit` pipeline and `create_partitions` notebook.

A pupil premium file's name has the academic year, like a census file's: `NPD_pupil-premium_FFD_CLA_2425_U.txt` is
for 2024/2025. Its maturity must be `U` (or none) or `F`.

### Reading the file

Files are read as for [NPD datasets](#reading-the-file): tab-separated UTF-8, with every column stored as a trimmed
string in the pupil's `data`, dots in column names replaced with underscores, and `DOB` parsed day-first and written
as `yyyy-MM-ddT00:00:00.000Z`. The file must have the old pipeline's columns: `PupilMatchingRef`, `LA`, `Estab`,
`URN`, `UPN`, `Surname`, `Forename`, `Middlenames`, `Sex`, `DOB`, `Source` and `Adopted`. In `data`, `Adopted` is
renamed to `AdoptedfromCarePremium`, as the old pipeline did. The file must also have the columns the
[prepared downloads](#pupil-premium-prepared-downloads) need.

A pupil with more than one row gets a single record whose `data` is an array of their rows in file order. The
record's identifying fields come from their first row, and its `source_flag` is the rows' `Source`, or `Both` if they
have more than one.

### Rejected rows and failed files

| Problem | Outcome |
| --- | --- |
| The file's maturity is `A` | The data file fails |
| The file is empty, is missing one of the columns above or one the prepared downloads need, or has two columns with the same name | The data file fails, and nothing is written |
| A row has no PupilMatchingRef | The row is rejected |
| A row's DOB isn't a valid date | The row is rejected |
| A row's `Adopted` is `1` | The row is rejected; pupils adopted from care aren't loaded |
| Every row is rejected, or there are no rows | The data file fails, with the first rejected row's reasons |
| The stored data is from a more recent file | The data file fails |

The old pipeline dropped adopted pupils' rows without recording them, and also dropped rows with an empty `Adopted`.
They're now recorded as rejected rows, and rows with an empty `Adopted` are kept.

### Writing the records

Only the latest pupil premium file is kept: a data file replaces every pupil in `pupil_premium_pupils`, and a pupil
that isn't in the file is removed. If a zip has more than one pupil premium data file, the last one wins.

A file is only written if it's at least as recent as the stored data: a later academic year, or the same year and at
least as mature (`U`, then `F`). The stored data is from the last pupil premium data file that succeeded, according to
`ingest_runs`. The old pipeline had no such check, so uploading a historical file would have replaced the current
data.

### Pupil premium prepared downloads

Once the pupils have been written, `PupilPremiumPreparedDownloads` writes the
[prepared downloads](#prepared-downloads). A data file that fails before then, e.g. because the stored data is from a
more recent file, writes none. If writing them fails, the data file fails; the pupils have already been written, and
replaying the zip writes the prepared downloads again.

Every row in the data file is used, as in the old pipeline, including the rows that are rejected from
`pupil_premium_pupils`, such as pupils adopted from care. A row's `Source` decides which files it's in:

| File | Rows | Columns |
| --- | --- | --- |
| `LA/<LA>/pp<year>_<LA>_<n>.csv` | `Source` `C` | `LA`, `Estab`, then the pupil columns |
| `School/<URN>/pp<year>_<LAEstab>_<n>.csv` | `Source` `C` | The pupil columns |
| `MAT/<group UID>/pp<year>_<group UID>_<n>.csv` | `Source` `C`, at schools in the MAT | `GroupUID`, `LA`, `Estab`, then the pupil columns |
| `SAT/<group UID>/pp<year>_<group UID>_<n>.csv` | `Source` `C`, at schools in the SAT | `GroupUID`, `LA`, `Estab`, then the pupil columns |
| `LA/<LA>/pp<year>_<LA>_AP_<n>.csv` | `Source` `A` | `LA`, `AlternativeProvision` (always `ALT`), then the pupil columns |

`<year>` is the academic year as in the data file's name, e.g. `2425`, and `<n>` is `1` for a `U` file and `2` for an
`F` file. A school's file is named by the `LAEstab` of its first row; if that's empty, the school's file isn't written.

The pupil columns are `UPN`, `Surname`, `Forename`, `Sex`, `DOB`, `NCYear`, `Deprivation Pupil Premium`,
`Service Child Premium`, `Adopted from Care Premium`, `Looked After Premium`, `PupilPremiumFTE`,
`PupilPremiumCashAmount`, `PupilPremiumFYStartDate`, `PupilPremiumFYEndDate` and `LastFSM`. They come from the data
file's columns of the same name, without the spaces, except:

- `Adopted from Care Premium` is the data file's `Adopted`.
- `Service Child Premium` is `CENSUSSERVICEEVER6`. In a school's file, it's `1` if `MODSERVICE` is `1` and
  `CENSUSSERVICEEVER6` is `0`.
- `DOB`, `PupilPremiumFYStartDate` and `PupilPremiumFYEndDate` are written as `yyyy-MM-dd`, read as for
  [NPD datasets](#dates). A value that isn't a valid date is empty.

The data file must have every one of these columns (and `LAEstab`, `MODSERVICE` and `CENSUSSERVICEEVER6`). This is
checked before anything is written, so a file the prepared downloads can't be written from doesn't replace the
pupils.

As in the old pipeline, files from earlier data files are kept, so an LA's folder has a `_1` and a `_2` file for each
year. The old pipeline's `RemoveOldPupilPremiumFiles` notebook, which deleted every pupil premium file, hasn't been
replaced.

Apart from the [differences for every prepared download](#prepared-downloads), these are the deliberate differences
from the old pipeline:

- School files no longer have the artefacts of being rewritten by pandas, e.g. `1.0` for `1` in a column with an empty
  value.
- Dates are read as for [NPD datasets](#dates), so slashed dates are read day-first.

## Further education

`FurtherEducationDatasetProcessor` (in `src/DfE.GIAP.Core/PupilData`) processes the FE pupil premium and FE SEN data
files. It replaces the old pipeline's `fe_data_update_pipeline` notebook, and writes to
`further_education_learner_records` and `further_education_learners` through `FurtherEducationDataWriter`.

An FE file's name has the calendar year the academic year ends in, like an NPD file's: `FE_SEN_2026.txt` is for
2025/2026. Its maturity is ignored; as in the old pipeline, FE files have no maturity rule.

### Reading the file

Files are read as for [NPD datasets](#reading-the-file): tab-separated UTF-8, with every column stored as a trimmed
string in the record's `data`, dots in column names replaced with underscores, and `DOB` parsed day-first and written
as `yyyy-MM-ddT00:00:00.000Z`. The file must have the old pipeline's columns: `ULN`, `Surname`, `Forename`, `Sex`,
`DOB` and `ACAD_YEAR`. (The old pipeline's FE pupil premium columns had `Sex` misspelt as `Sec`.)

A learner with more than one row gets a single record whose `data` is an array of their rows in file order. The
record's identifying fields come from their first row.

FE files write `ACAD_YEAR` as e.g. `2025/26`; `2025/2026` is also accepted. Records are stored under the file's
academic year in full, e.g. `2025/2026`, but `data` keeps `ACAD_YEAR` as it is in the file. A row with no academic year
keeps `null` in `data`, but its record is stored under the file's academic year.

### Rejected rows and failed files

| Problem | Outcome |
| --- | --- |
| The file is empty, is missing one of the columns above, or has two columns with the same name | The data file fails |
| A row has no ULN | The row is rejected |
| A row's DOB isn't a valid date | The row is rejected |
| A row's `ACAD_YEAR` isn't the one in the file's name, as `2025/26` or `2025/2026` | The row is rejected |
| Every row is rejected, or there are no rows | The data file fails, with the first rejected row's reasons |

The ULN's format isn't checked, as in the old pipeline. The old pipeline stored each row under its own `ACAD_YEAR`, so
a file could cover more than one academic year; every row must now be for the file's academic year.

### Writing the records

- An FE pupil premium file replaces **all** FE data, SEN included, as the old pipeline emptied the container before
  each one. Afterwards, `further_education_learners` has exactly the learners in the file. Any SEN files need to be
  uploaded again after it.
- An FE SEN file replaces the SEN records for its academic year. A learner that has a SEN record for the year but
  isn't in the file loses it; the old pipeline merged SEN files into the stored data, so they kept it.
- A learner left with no records is removed.
- The identifying fields on `further_education_learners` come from a pupil premium record if the learner has one, then
  from the latest academic year, then from the record that was loaded first. The old pipeline meant to prefer pupil
  premium records, but its check never matched.

There's no check against the stored data: any FE file is written, even one for an earlier academic year. Writing the
same file again leaves the same data, so replaying a file is safe.

## Children looked after

`ChildrenLookedAfterDatasetProcessor` (in `src/DfE.GIAP.Core/PupilData`) processes the KS2 and KS4 children looked
after (CLA) data files. It replaces the old pipeline's `giapKS2CLASplit` and `giapKS4CLASplit` pipelines and
`create_partitions_cla` notebook. Nothing is written to Postgres; the files are only split into
[prepared downloads](#prepared-downloads) for LAs.

Zips are uploaded to `cla-ks2/upload/` or `cla-ks4/upload/`, and their names must start with `GIAP_CLA_KS2_` or
`GIAP_CLA_KS4_`. A CLA data file's name must contain `CLA` and `KS2` or `KS4`, and the calendar year the academic year
ends in, e.g. `GIAP_CLA_KS4_2025.txt` is for 2024/2025. Its maturity is ignored.

Files are read as for [NPD datasets](#reading-the-file): tab-separated UTF-8, with values trimmed. The file must have
the old pipeline's columns: `CLA_LA`, `CLA_CHILD_LA_CODE`, `CLA_SEX`, `CLA_DOB`, `CLA_AGE_AUG`, `CLA_UPN`,
`CLA_ETHNIC` and `CLA_12_MONTHS`. The old pipeline used the KS4 columns for KS2 files too.

Each LA's rows are written to `LA/<CLA_LA>/<ks2 or ks4>_<year>_cla.csv`, e.g. `LA/201/ks4_2025_cla.csv`, with every
column in the data file, in the order they're in the file. A row with no `CLA_LA` isn't written, and isn't counted in
the run's records written.

| Problem | Outcome |
| --- | --- |
| The file is empty, is missing one of the columns above, or has two columns with the same name | The data file fails |
| There are no rows | The data file fails |

The [differences for every prepared download](#prepared-downloads) apply; in particular, `CLA_DOB` is written as it
is in the data file, rather than in whatever form Spark inferred for it.

## Replaying files

Processing a file again reprocesses every data file in it from wherever the zip is now. A zip that succeeds is moved
to `processed/` (or stays there), and one that fails is moved to `error/`. Dataset processors must be safe to run
again on the same file.

- **One file**: requeue its `ProcessIngestFileJob` from the Hangfire dashboard's failed or succeeded jobs.
- **Every failed file**: trigger the `requeue-failed-ingest-files` recurring job from the dashboard. It's never
  scheduled.
- **Fixing a failed upload**: upload a corrected zip with the same name to `upload/`.
- **Backfill**: upload the historical zips to `upload/`.

## What's recorded

| Table | One row per |
| --- | --- |
| `ingest_files` | Uploaded zip path: where the zip is now, its status, the reason it failed and the number of attempts |
| `ingest_runs` | Data file per attempt: dataset, academic year, maturity, record counts, start/end times, status and error |
| `ingest_rejected_rows` | Row a dataset processor rejected: its row number, values and the reasons |
