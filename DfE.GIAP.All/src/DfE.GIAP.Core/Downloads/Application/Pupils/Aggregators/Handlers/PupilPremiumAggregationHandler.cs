using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers;

public class PupilPremiumAggregationHandler : IPupilDatasetAggregationHandler
{
    public bool CanHandle(DownloadType downloadType)
        => downloadType == DownloadType.PupilPremium;

    private readonly IPupilPremiumReadOnlyRepository _ppReadRepository;

    public PupilPremiumAggregationHandler(IPupilPremiumReadOnlyRepository pupilPremiumReadRepository)
    {
        ArgumentNullException.ThrowIfNull(pupilPremiumReadRepository);
        _ppReadRepository = pupilPremiumReadRepository;
    }

    public async Task<PupilDatasetCollection> AggregateAsync(
            IEnumerable<string> pupilIds,
            IEnumerable<Dataset> selectedDatasets,
            CancellationToken cancellationToken = default)
    {
        IEnumerable<PupilPremiumPupil> pupils = await _ppReadRepository.GetPupilsByIdsAsync(pupilIds);
        PupilDatasetCollection collection = new();

        foreach (PupilPremiumPupil pupil in pupils)
        {
            if (selectedDatasets.Contains(Dataset.PP) && pupil.HasPupilPremiumData)
                AddPupilPremiumRecord(collection, pupil);
        }

        return collection;
    }


    private static void AddPupilPremiumRecord(PupilDatasetCollection collection, PupilPremiumPupil pp)
    {
        PupilPremiumEntry? ppEntry = pp.PupilPremium?.FirstOrDefault();
        collection.PupilPremium.Add(new PupilPremiumOutputRecord
        {
            UniquePupilNumber = pp.UniquePupilNumber,
            Surname = pp.Surname,
            Forename = pp.Forename,
            Sex = pp.Sex,
            DOB = pp.DOB.ToShortDateString(),
            NCYear = ppEntry?.NCYear,
            DeprivationPupilPremium = ppEntry?.DeprivationPupilPremium,
            ServiceChildPremium = ppEntry?.ServiceChildPremium,
            AdoptedfromCarePremium = ppEntry?.AdoptedfromCarePremium,
            LookedAfterPremium = ppEntry?.LookedAfterPremium,
            PupilPremiumFTE = ppEntry?.PupilPremiumFTE,
            PupilPremiumCashAmount = ppEntry?.PupilPremiumCashAmount,
            PupilPremiumFYStartDate = ppEntry?.PupilPremiumFYStartDate,
            PupilPremiumFYEndDate = ppEntry?.PupilPremiumFYEndDate,
            LastFSM = ppEntry?.LastFSM
        });
    }
}
