using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Pupils;

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
    public List<PupilPremiumOutputRecord> PupilPremium { get; set; } = new();
    public List<FurtherEducationPPOutputRecord> FurtherEducationPP { get; set; } = new();
    public List<FurtherEducationSENOutputRecord> SEN { get; set; } = new();
}
