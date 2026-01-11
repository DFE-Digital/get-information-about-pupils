using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

public class PupilDatasetCollection
{
    //public List<CensusAutumnOutput> CensusAutumn { get; set; } = new();
    //public List<CensusSpringOutput> CensusSpring { get; set; } = new();
    //public List<CensusSummerOutput> CensusSummer { get; set; } = new();
    //public List<EYFSPOutput> EYFSP { get; set; } = new();
    //public List<KS1Output> KS1 { get; set; } = new();
    //public List<KS2Output> KS2 { get; set; } = new();
    //public List<KS4Output> KS4 { get; set; } = new();
    //public List<PhonicsOutput> Phonics { get; set; } = new();
    //public List<MTCOutput> MTC { get; set; } = new();
    public List<PPOutputRecord> PP { get; set; } = new();
    public List<SENOutputRecord> SEN { get; set; } = new();
}



// Application/Models/PupilDatasetContext.cs
public class PupilDatasetContext
{
    public DownloadType SourceType { get; set; }

    // Original pupil model (FurtherEducationPupil or NationalPupil)
    public object RawPupil { get; set; } = default!;

    // Dataset availability flags
    public bool HasCensusAutumnData { get; set; }
    public bool HasCensusSpringData { get; set; }
    public bool HasCensusSummerData { get; set; }
    public bool HasEYFSPData { get; set; }
    public bool HasPhonicsData { get; set; }
    public bool HasKeyStage1Data { get; set; }
    public bool HasKeyStage2Data { get; set; }
    public bool HasKeyStage4Data { get; set; }
    public bool HasMtcData { get; set; }
    public bool HasPupilPremiumData { get; set; }
    public bool HasSpecialEducationalNeedsData { get; set; }
}


// Application/Interfaces/IPupilDatasetAggregator.cs
public interface IPupilDatasetAggregator
{
    Task<PupilDatasetCollection> AggregateAsync(
        DownloadType downloadType,
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets);
}

// Application/Services/PupilDatasetAggregator.cs
// TODO: FE/NPD/PP pupil aggregator maybe?
public class PupilDatasetAggregator : IPupilDatasetAggregator
{
    private readonly IFurtherEducationReadOnlyRepository _feRepo;
    private readonly INationalPupilReadOnlyRepository _npdRepo;

    public PupilDatasetAggregator(
        IFurtherEducationReadOnlyRepository feRepo,
        INationalPupilReadOnlyRepository npdRepo)
    {
        _feRepo = feRepo;
        _npdRepo = npdRepo;
    }

    public async Task<PupilDatasetCollection> AggregateAsync(
        DownloadType downloadType,
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets)
    {
        List<PupilDatasetContext> contexts = await GetPupilContextsAsync(downloadType, pupilIds);

        PupilDatasetCollection collection = new();
        foreach (PupilDatasetContext ctx in contexts)
        {
            foreach (Dataset dataset in selectedDatasets)
            {
                if (!HasDataset(ctx, dataset))
                    continue;

                AddDatasetRecord(collection, ctx, dataset);
            }
        }

        return collection;
    }

    private async Task<List<PupilDatasetContext>> GetPupilContextsAsync(
        DownloadType type,
        IEnumerable<string> pupilIds)
    {
        return type switch
        {
            DownloadType.FurtherEducation or DownloadType.PupilPremium
                => await GetFEPupilContextsAsync(pupilIds),
            DownloadType.NPD
                => await GetNPDPupilContextsAsync(pupilIds),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private async Task<List<PupilDatasetContext>> GetFEPupilContextsAsync(IEnumerable<string> pupilIds)
    {
        IEnumerable<FurtherEducationPupil> pupils = await _feRepo.GetPupilsByIdsAsync(pupilIds);

        return pupils.Select(fe => new PupilDatasetContext
        {
            SourceType = DownloadType.FurtherEducation,
            RawPupil = fe,
            HasPupilPremiumData = fe.HasPupilPremiumData,
            HasSpecialEducationalNeedsData = fe.HasSpecialEducationalNeedsData
        }).ToList();
    }

    private async Task<List<PupilDatasetContext>> GetNPDPupilContextsAsync(IEnumerable<string> pupilIds)
    {
        IEnumerable<NationalPupil> pupils = await _npdRepo.GetPupilsByIdsAsync(pupilIds);

        return pupils.Select(np => new PupilDatasetContext
        {
            SourceType = DownloadType.NPD,
            RawPupil = np,
            HasCensusAutumnData = np.HasCensusAutumnData,
            HasCensusSpringData = np.HasCensusSpringData,
            HasCensusSummerData = np.HasCensusSummerData,
            HasEYFSPData = np.HasEYFSPData,
            HasPhonicsData = np.HasPhonicsData,
            HasKeyStage1Data = np.HasKeyStage1Data,
            HasKeyStage2Data = np.HasKeyStage2Data,
            HasKeyStage4Data = np.HasKeyStage4Data,
            HasMtcData = np.HasMtcData
        }).ToList();
    }

    private static bool HasDataset(PupilDatasetContext ctx, Dataset dataset)
    {
        return dataset switch
        {
            Dataset.Census_Autumn => ctx.HasCensusAutumnData,
            Dataset.Census_Spring => ctx.HasCensusSpringData,
            Dataset.Census_Summer => ctx.HasCensusSummerData,
            Dataset.EYFSP => ctx.HasEYFSPData,
            Dataset.KS1 => ctx.HasKeyStage1Data,
            Dataset.KS2 => ctx.HasKeyStage2Data,
            Dataset.KS4 => ctx.HasKeyStage4Data,
            Dataset.Phonics => ctx.HasPhonicsData,
            Dataset.MTC => ctx.HasMtcData,
            Dataset.PP => ctx.HasPupilPremiumData,
            Dataset.SEN => ctx.HasSpecialEducationalNeedsData,
            _ => false
        };
    }

    private static void AddDatasetRecord(
        PupilDatasetCollection collection,
        PupilDatasetContext ctx,
        Dataset dataset)
    {
        switch (dataset)
        {
            case Dataset.PP:
                AddPupilPremiumRecord(collection, ctx);
                break;
            case Dataset.SEN:
                AddSenRecord(collection, ctx);
                break;

                // TODO: Add handlers for Census, EYFSP, KS1, KS4, Phonics, MTC
        }
    }

    // FURTHER EDUCATION OUTPUT
    private static void AddPupilPremiumRecord(PupilDatasetCollection collection, PupilDatasetContext ctx)
    {
        FurtherEducationPupil fe = (FurtherEducationPupil)ctx.RawPupil;
        PupilPremiumEntry? pp = fe.PupilPremium?.FirstOrDefault();

        collection.PP.Add(new PPOutputRecord
        {
            ULN = fe.UniqueLearnerNumber,
            Forename = fe.Forename,
            Surname = fe.Surname,
            Gender = fe.Gender,
            DOB = fe.DOB.ToShortDateString(),
            ACAD_YEAR = pp?.AcademicYear,
            NCYear = pp?.NationalCurriculumYear,
            Pupil_Premium_FTE = pp?.FullTimeEquivalent,
        });
    }

    private static void AddSenRecord(PupilDatasetCollection collection, PupilDatasetContext ctx)
    {
        FurtherEducationPupil fe = (FurtherEducationPupil)ctx.RawPupil;
        SpecialEducationalNeedsEntry? sen = fe.specialEducationalNeeds?.FirstOrDefault();

        collection.SEN.Add(new SENOutputRecord
        {
            ULN = fe.UniqueLearnerNumber,
            Forename = fe.Forename,
            Surname = fe.Surname,
            Gender = fe.Gender,
            DOB = fe.DOB.ToShortDateString(),
            NCYear = sen?.NationalCurriculumYear,
            Acad_Year = sen?.AcademicYear,
            SEN_Provision = sen?.Provision,
        });
    }
}
