namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

/// <summary>
/// The kind of data in an ingested data file, from its name.
/// </summary>
public enum IngestDataset
{
    CensusAutumn,
    CensusSpring,
    CensusSummer,
    Eyfsp,
    Ks1,
    Ks2,
    Ks4,
    Mtc,
    Phonics,
    PupilPremium,
    FurtherEducationPupilPremium,
    FurtherEducationSpecialEducationalNeeds
}
