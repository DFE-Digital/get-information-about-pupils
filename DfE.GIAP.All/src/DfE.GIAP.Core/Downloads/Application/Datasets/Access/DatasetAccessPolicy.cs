using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access;

public static class DatasetAccessPolicy
{
    private static readonly Dictionary<Dataset, IDatasetAccessRule> _rules = new()
    {
        [Dataset.EYFSP] = DatasetAccessRules.EYFSP(),
        [Dataset.KS1] = DatasetAccessRules.KS1(),
        [Dataset.KS2] = DatasetAccessRules.KS2(),
        [Dataset.KS4] = DatasetAccessRules.KS4(),
        [Dataset.Phonics] = DatasetAccessRules.Phonics(),
        [Dataset.MTC] = DatasetAccessRules.Mtc(),
        [Dataset.PP] = DatasetAccessRules.PupilPremium(),
        [Dataset.SEN] = DatasetAccessRules.SpecialEducationNeeds(),
        [Dataset.CensusAutumn] = DatasetAccessRules.CensusAutumn(),
        [Dataset.CensusSpring] = DatasetAccessRules.CensusSpring(),
        [Dataset.CensusSummer] = DatasetAccessRules.CensusSummer()
    };

    public static bool IsAllowed(Dataset dataset, IAuthorisationContext context)
    {
        return _rules.TryGetValue(dataset, out IDatasetAccessRule? rule) && rule.CanDownload(context);
    }
}
