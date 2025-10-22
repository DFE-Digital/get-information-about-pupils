using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;

namespace DfE.GIAP.Core.Downloads.Application.DatasetCheckers;

public interface IDatasetAccessEvaluator
{
    bool CanDownload(IAuthorisationContext authorisationContext, Datasets dataset);
}

public class DatasetAccessEvaluator : IDatasetAccessEvaluator
{
    public bool CanDownload(IAuthorisationContext authorisationContext, Datasets dataset)
    {
        // Admins & DfE users can download everything
        if (authorisationContext.Role == "GIAPAdmin" || authorisationContext.IsDfeUser)
            return true;

        // Age-based access rules
        return dataset switch
        {
            Datasets.EYFSP => IsAgeRangeValid(authorisationContext: authorisationContext, minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25),
            Datasets.KS1 => IsAgeRangeValid(authorisationContext: authorisationContext, minLow: 2, maxLow: 13, minHigh: 6, maxHigh: 25),
            Datasets.KS2 => IsAgeRangeValid(authorisationContext: authorisationContext, minLow: 2, maxLow: 15, minHigh: 6, maxHigh: 25),
            Datasets.KS4 => IsAgeRangeValid(authorisationContext: authorisationContext, minLow: 2, maxLow: 17, minHigh: 12, maxHigh: 25),
            Datasets.Phonics => IsAgeRangeValid(authorisationContext: authorisationContext, minLow: 2, maxLow: 10, minHigh: 3, maxHigh: 25),
            Datasets.MTC => IsAgeRangeValid(authorisationContext: authorisationContext, minLow: 2, maxLow: 14, minHigh: 4, maxHigh: 25),

            // ULN-based access rules
            Datasets.PP => authorisationContext.StatutoryAgeHigh >= 14,
            Datasets.SEN => authorisationContext.StatutoryAgeHigh >= 14,

            // Datasets with no restrictions until further notice
            Datasets.CensusAutumn => true,
            Datasets.CensusSpring => true,
            Datasets.CensusSummer => true,

            // Default: not downloadable
            _ => false
        };
    }

    private static bool IsAgeRangeValid(IAuthorisationContext authorisationContext, int minLow, int maxLow, int minHigh, int maxHigh)
    {
        return authorisationContext.StatutoryAgeLow >= minLow &&
               authorisationContext.StatutoryAgeLow <= maxLow &&
               authorisationContext.StatutoryAgeHigh >= minHigh &&
               authorisationContext.StatutoryAgeHigh <= maxHigh &&
               authorisationContext.StatutoryAgeLow < authorisationContext.StatutoryAgeHigh;
    }
}
