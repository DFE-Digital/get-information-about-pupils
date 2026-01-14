using DfE.GIAP.Core.Downloads.Application.Datasets;
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

        DatasetMetadata metadata = DatasetMetadata.For(DownloadType.FurtherEducation);
        IEnumerable<Dataset> datasetsToProcess = selectedDatasets.Intersect(metadata.SupportedDatasets);

        IEnumerable<FurtherEducationPupil> pupils = await _feReadRepository.GetPupilsByIdsAsync(pupilIds);
        PupilDatasetCollection collection = new();

        foreach (FurtherEducationPupil fe in pupils)
        {
            foreach (Dataset dataset in datasetsToProcess)
            {
                switch (dataset)
                {
                    case Dataset.PP when fe.HasPupilPremiumData:
                        AddPupilPremiumRecord(collection, fe);
                        break;
                    case Dataset.SEN when fe.HasSpecialEducationalNeedsData:
                        AddSenRecord(collection, fe);
                        break;
                }
            }
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
            Gender = fe.Gender,
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
            Gender = fe.Gender,
            DOB = fe.DOB.ToShortDateString(),
            NCYear = sen?.NationalCurriculumYear,
            Acad_Year = sen?.AcademicYear,
            SEN_Provision = sen?.Provision,
        });
    }
}
