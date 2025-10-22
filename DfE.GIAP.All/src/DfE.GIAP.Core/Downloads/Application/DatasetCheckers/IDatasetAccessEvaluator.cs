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
            Datasets.EYFSP => IsAgeRangeValid(authorisationContext, 2, 10, 3, 25),
            Datasets.KS1 => IsAgeRangeValid(authorisationContext, 2, 13, 6, 25),
            Datasets.KS2 => IsAgeRangeValid(authorisationContext, 2, 15, 6, 25),
            Datasets.KS4 => IsAgeRangeValid(authorisationContext, 2, 17, 12, 25),
            Datasets.Phonics => IsAgeRangeValid(authorisationContext, 2, 10, 3, 25),
            Datasets.MTC => IsAgeRangeValid(authorisationContext, 2, 14, 4, 25),

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
