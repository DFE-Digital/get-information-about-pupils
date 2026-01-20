using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators;

public class PupilDatasetCollection
{
    public List<EYFSPOutput> EYFSP { get; set; } = new();
    public List<KS1Output> KS1 { get; set; } = new();
    public List<KS2Output> KS2 { get; set; } = new();
    public List<KS4Output> KS4 { get; set; } = new();
    public List<PhonicsOutput> Phonics { get; set; } = new();
    public List<MTCOutput> MTC { get; set; } = new();
    public List<CensusAutumnOutput> CensusAutumn { get; set; } = new();
    public List<CensusSpringOutput> CensusSpring { get; set; } = new();
    public List<CensusSummerOutput> CensusSummer { get; set; } = new();
    public List<PupilPremiumOutputRecord> PupilPremium { get; set; } = new();
    public List<FurtherEducationPPOutputRecord> FurtherEducationPP { get; set; } = new();
    public List<FurtherEducationSENOutputRecord> SEN { get; set; } = new();

    public IEnumerable<object> GetRecords(Dataset dataset) =>
            dataset switch
            {
                Dataset.FE_PP => FurtherEducationPP,
                Dataset.SEN => SEN,
                Dataset.PP => PupilPremium,
                Dataset.KS1 => KS1,
                Dataset.KS2 => KS2,
                Dataset.KS4 => KS4,
                Dataset.Census_Autumn => CensusAutumn,
                Dataset.Census_Spring => CensusSpring,
                Dataset.Census_Summer => CensusSummer,
                Dataset.EYFSP => EYFSP,
                Dataset.Phonics => Phonics,
                Dataset.MTC => MTC,
                _ => Enumerable.Empty<object>()
            };
}
