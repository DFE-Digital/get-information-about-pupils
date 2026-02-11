using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators;

public class PupilDatasetCollection
{
    public List<EYFSPOutputRecord> EYFSP { get; set; } = new();
    public List<KS1OutputRecord> KS1 { get; set; } = new();
    public List<KS2OutputRecord> KS2 { get; set; } = new();
    public List<KS4OutputRecord> KS4 { get; set; } = new();
    public List<PhonicsOutputRecord> Phonics { get; set; } = new();
    public List<MTCOutputRecord> MTC { get; set; } = new();
    public List<CensusAutumnOutputRecord> CensusAutumn { get; set; } = new();
    public List<CensusSpringOutputRecord> CensusSpring { get; set; } = new();
    public List<CensusSummerOutputRecord> CensusSummer { get; set; } = new();
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
