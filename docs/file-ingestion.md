# File ingestion

> [!WARNING]
> The pipeline documented here isn't live in production yet. Until the switch-over, data files are still processed
> by the old Data Factory and Databricks pipeline.

The file ingestion job processes the zips of data files uploaded to the `ingest` blob container. It runs in the Worker
(`src/DfE.GIAP.Worker`) as Hangfire jobs; the code is in `src/DfE.GIAP.Core/Ingest` and `src/DfE.GIAP.Core/Jobs`.

## Container layout

Each dataset has a folder, matching the old pipeline's: `census`, `eyfsp`, `ks1`, `ks2`, `ks4`, `mtc`, `phonics`,
`pupil-premium`, `fe_pp` and `fe_sen`. Each folder has:

| Folder | Contents |
| --- | --- |
| `<dataset>/upload/` | Zips waiting to be processed |
| `<dataset>/processed/` | Zips that were processed successfully |
| `<dataset>/error/` | Zips that failed, and duplicate uploads |

A blob is never overwritten when it's moved. If the name is already taken, a UTC timestamp is added, e.g.
`NPD_KS2_2025_U.20261120T101500Z.zip`.

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
    anyfailed -- No --> succeeded[Move the zip to processed/ and mark the file Succeeded]
    succeeded --> done([Job succeeds])

    zipfail --> error[Move the zip to error/ and mark the file Failed, with the reason]
    error --> fail
```

Each data file is committed separately. A zip whose data files partly fail still keeps the data that was written.

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
