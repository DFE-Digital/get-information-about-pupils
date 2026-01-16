using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers;

public class FurtherEducationAggregationHandler : IPupilDatasetAggregationHandler
{
    public bool CanHandle(DownloadType downloadType)
        => downloadType == DownloadType.FurtherEducation;

    private readonly IFurtherEducationReadOnlyRepository _feReadRepository;

    public FurtherEducationAggregationHandler(IFurtherEducationReadOnlyRepository feReadRepository)
    {
        ArgumentNullException.ThrowIfNull(feReadRepository);
        _feReadRepository = feReadRepository;
    }


    public async Task<PupilDatasetCollection> AggregateAsync(
            IEnumerable<string> pupilIds,
            IEnumerable<Dataset> selectedDatasets,
            CancellationToken cancellationToken = default)
    {
        PupilDatasetCollection collection = new();
        IEnumerable<FurtherEducationPupil> pupils = await _feReadRepository.GetPupilsByIdsAsync(pupilIds);

        foreach (FurtherEducationPupil pupil in pupils)
        {
            if (selectedDatasets.Contains(Dataset.FE_PP) && pupil.HasPupilPremiumData)
                AddPupilPremiumRecord(collection, pupil);
            if (selectedDatasets.Contains(Dataset.SEN) && pupil.HasSpecialEducationalNeedsData)
                AddSenRecord(collection, pupil);
        }

        return collection;
    }

    private static void AddPupilPremiumRecord(PupilDatasetCollection collection, FurtherEducationPupil fe)
    {
        FurtherEducationPupilPremiumEntry? ppEntry = fe.PupilPremium?.FirstOrDefault();
        collection.FurtherEducationPP.Add(new FurtherEducationPPOutputRecord
        {
            ULN = fe.UniqueLearnerNumber,
            Forename = fe.Forename,
            Surname = fe.Surname,
            Sex = fe.Sex,
            DOB = fe.DOB.ToShortDateString(),
            ACAD_YEAR = ppEntry?.AcademicYear,
            NCYear = ppEntry?.NationalCurriculumYear,
            Pupil_Premium_FTE = ppEntry?.FullTimeEquivalent,
        });
    }

    private static void AddSenRecord(PupilDatasetCollection collection, FurtherEducationPupil fe)
    {
        SpecialEducationalNeedsEntry? sen = fe.specialEducationalNeeds?.FirstOrDefault();
        collection.SEN.Add(new FurtherEducationSENOutputRecord
        {
            ULN = fe.UniqueLearnerNumber,
            Forename = fe.Forename,
            Surname = fe.Surname,
            Sex = fe.Sex,
            DOB = fe.DOB.ToShortDateString(),
            NCYear = sen?.NationalCurriculumYear,
            ACAD_YEAR = sen?.AcademicYear,
            SEN_PROVISION = sen?.Provision,
        });
    }
}
