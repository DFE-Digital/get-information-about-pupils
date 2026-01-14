using DfE.GIAP.Core.Downloads.Application.Datasets;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
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
        PupilPremiumEntry? pp = fe.PupilPremium?.FirstOrDefault();
        collection.PP.Add(new PPOutputRecord
        {
            ULN = fe.UniqueLearnerNumber,
            Forename = fe.Forename,
            Surname = fe.Surname,
            Gender = fe.Gender,
            DOB = fe.DOB.ToShortDateString(),
            ACAD_YEAR = pp?.AcademicYear,
            NCYear = pp?.NationalCurriculumYear,
            Pupil_Premium_FTE = pp?.FullTimeEquivalent,
        });
    }

    private static void AddSenRecord(PupilDatasetCollection collection, FurtherEducationPupil fe)
    {
        SpecialEducationalNeedsEntry? sen = fe.specialEducationalNeeds?.FirstOrDefault();
        collection.SEN.Add(new SENOutputRecord
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

public class NationalPupilDatabaseAggregationHandler : IPupilDatasetAggregationHandler
{
    public bool CanHandle(DownloadType downloadType)
        => downloadType == DownloadType.NPD;

    public Task<PupilDatasetCollection> AggregateAsync(
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();
}

public class PupilPremiumAggregationHandler : IPupilDatasetAggregationHandler
{
    public bool CanHandle(DownloadType downloadType)
        => downloadType == DownloadType.PupilPremium;

    public Task<PupilDatasetCollection> AggregateAsync(
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
