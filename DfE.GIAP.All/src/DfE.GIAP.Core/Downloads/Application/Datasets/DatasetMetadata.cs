using DfE.GIAP.Core.Downloads.Application.Datasets.Availability;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Pupils;

namespace DfE.GIAP.Core.Downloads.Application.Datasets;

public class DatasetMetadata
{
    private readonly Dictionary<Dataset, DatasetMetadataRecord> _records;
    public IReadOnlyCollection<Dataset> SupportedDatasets { get; }

    private DatasetMetadata(
        IReadOnlyCollection<Dataset> supported,
        Dictionary<Dataset, DatasetMetadataRecord> records)
    {
        SupportedDatasets = supported;
        _records = records;
    }

    public static DatasetMetadata For(DownloadType type)
    {
        IReadOnlyCollection<Dataset> supported = AvailableDatasetsByDownloadType.GetSupportedDatasets(type);

        Dictionary<Dataset, DatasetMetadataRecord> records = AllDatasetMetadata
            .Where(kvp => supported.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return new DatasetMetadata(supported, records);
    }

    public IEnumerable<object> GetRecords(Dataset dataset, PupilDatasetCollection collection)
        => _records[dataset].Accessor(collection);

    public string GetFileName(Dataset dataset, FileFormat format)
    {
        string ext = format == FileFormat.Csv ? "csv" : "txt";
        return $"{_records[dataset].BaseFileName}.{ext}";
    }

    // -----------------------------------------
    // MASTER METADATA TABLE (single source of truth)
    // -----------------------------------------
    private static readonly Dictionary<Dataset, DatasetMetadataRecord> AllDatasetMetadata =
        new()
        {
            {
                Dataset.PP,
                new DatasetMetadataRecord
                {
                    Dataset = Dataset.PP,
                    Accessor = c => c.PP,
                    BaseFileName = "pp_results"
                }
            },
            {
                Dataset.SEN,
                new DatasetMetadataRecord
                {
                    Dataset = Dataset.SEN,
                    Accessor = c => c.SEN,
                    BaseFileName = "sen_results"
                }
            },
            //{
            //    Dataset.KS1,
            //    new DatasetMetadataRecord
            //    {
            //        Dataset = Dataset.KS1,
            //        Accessor = c => c.KS1,
            //        BaseFileName = "ks1_results"
            //    }
            //},
            //{
            //    Dataset.KS2,
            //    new DatasetMetadataRecord
            //    {
            //        Dataset = Dataset.KS2,
            //        Accessor = c => c.KS2,
            //        BaseFileName = "ks2_results"
            //    }
            //},
            //{
            //    Dataset.KS4,
            //    new DatasetMetadataRecord
            //    {
            //        Dataset = Dataset.KS4,
            //        Accessor = c => c.KS4,
            //        BaseFileName = "ks4_results"
            //    }
            //},
            //{
            //    Dataset.Census_Autumn,
            //    new DatasetMetadataRecord
            //    {
            //        Dataset = Dataset.Census_Autumn,
            //        Accessor = c => c.CensusAutumn,
            //        BaseFileName = "census_autumn_results"
            //    }
            //},
            //{
            //    Dataset.Census_Spring,
            //    new DatasetMetadataRecord
            //    {
            //        Dataset = Dataset.Census_Spring,
            //        Accessor = c => c.CensusSpring,
            //        BaseFileName = "census_spring_results"
            //    }
            //},
            //{
            //    Dataset.Census_Summer,
            //    new DatasetMetadataRecord
            //    {
            //        Dataset = Dataset.Census_Summer,
            //        Accessor = c => c.CensusSummer,
            //        BaseFileName = "census_summer_results"
            //    }
            //},
            //{
            //    Dataset.EYFSP,
            //    new DatasetMetadataRecord
            //    {
            //        Dataset = Dataset.EYFSP,
            //        Accessor = c => c.EYFSP,
            //        BaseFileName = "eyfsp_results"
            //    }
            //},
            //{
            //    Dataset.Phonics,
            //    new DatasetMetadataRecord
            //    {
            //        Dataset = Dataset.Phonics,
            //        Accessor = c => c.Phonics,
            //        BaseFileName = "phonics_results"
            //    }
            //},
            //{
            //    Dataset.MTC,
            //    new DatasetMetadataRecord
            //    {
            //        Dataset = Dataset.MTC,
            //        Accessor = c => c.MTC,
            //        BaseFileName = "mtc_results"
            //    }
            //}
        };
}


public class DatasetMetadataRecord
{
    public Dataset Dataset { get; init; }
    public Func<PupilDatasetCollection, IEnumerable<object>> Accessor { get; init; } = default!;
    public string BaseFileName { get; init; } = string.Empty;
}
