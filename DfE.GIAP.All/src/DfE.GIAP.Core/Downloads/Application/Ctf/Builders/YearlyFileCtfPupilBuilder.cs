using DfE.GIAP.Core.Downloads.Application.Ctf.Models;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

public class YearlyFileCtfPupilBuilder : ICtfPupilBuilder
{
    private readonly INationalPupilReadOnlyRepository _nationalPupilReadOnlyRepository;
    private readonly IDataSchemaProvider _schemaProvider;
    private readonly IPropertyValueAccessor _valueAccessor;

    public YearlyFileCtfPupilBuilder(
        INationalPupilReadOnlyRepository nationalPupilReadOnlyRepository,
        IDataSchemaProvider schemaProvider,
        IPropertyValueAccessor valueAccessor)
    {
        ArgumentNullException.ThrowIfNull(nationalPupilReadOnlyRepository);
        ArgumentNullException.ThrowIfNull(schemaProvider);
        ArgumentNullException.ThrowIfNull(valueAccessor);

        _nationalPupilReadOnlyRepository = nationalPupilReadOnlyRepository;
        _schemaProvider = schemaProvider;
        _valueAccessor = valueAccessor;
    }

    public async Task<IEnumerable<CtfPupil>> BuildAsync(IEnumerable<string> selectedPupilIds)
    {
        IEnumerable<NationalPupil> pupils =
            await _nationalPupilReadOnlyRepository.GetPupilsByIdsAsync(selectedPupilIds);

        List<CtfPupil> results = new();
        foreach (NationalPupil pupil in pupils)
        {
            CtfPupil ctf = CreateBasePupil(pupil);

            await AddLatestAssessment(pupil.HasEYFSPData, pupil.EarlyYearsFoundationStageProfile, e => e.ACADYR, ctf);
            await AddLatestAssessment(pupil.HasPhonicsData, pupil.Phonics, e => e.AcademicYear, ctf);
            await AddLatestAssessment(pupil.HasKeyStage1Data, pupil.KeyStage1, e => e.ACADYR, ctf);
            await AddLatestAssessment(pupil.HasKeyStage2Data, pupil.KeyStage2, e => e.ACADYR, ctf);
            await AddLatestAssessment(pupil.HasMtcData, pupil.MTC, e => e.ACADYR, ctf);

            results.Add(ctf);
        }

        return results;
    }

    private static CtfPupil CreateBasePupil(NationalPupil pupil)
    {
        return new CtfPupil
        {
            UPN = pupil.Upn ?? "",
            Surname = pupil.Surname ?? "",
            Forename = pupil.Forename ?? "",
            DOB = pupil.DOB.ToString("yyyy-MM-dd"),
            Sex = pupil.Sex
        };
    }

    private async Task AddLatestAssessment<TEntry>(
        bool hasData,
        IEnumerable<TEntry>? entries,
        Func<TEntry, string?> yearSelector,
        CtfPupil target)
    {
        if (!hasData || entries is null)
            return;

        TEntry? latest = entries
            .OrderByDescending(e => ToAcademicYearEnd(yearSelector(e)))
            .FirstOrDefault();

        if (latest is null)
            return;

        int academicYear = ToAcademicYearEnd(yearSelector(latest));
        string yearString = academicYear.ToString();

        DataSchemaDefinition? schema =
            await _schemaProvider.GetSchemaByYearAsync(academicYear);

        if (schema is null)
            return;

        target.Assessments.AddRange(
            BuildStageAssessments(latest, schema, "ENG", yearString));
    }

    public static int ToAcademicYearEnd(string? acadYr)
    {
        if (string.IsNullOrWhiteSpace(acadYr))
            return 0;

        string[] parts = acadYr.Split('/');
        if (parts.Length != 2)
            return 0;

        return int.TryParse(parts[1], out int endYear) ? endYear : 0;
    }

    private IEnumerable<CtfKeyStageAssessment> BuildStageAssessments(
        object entry,
        DataSchemaDefinition definition,
        string locale,
        string year)
    {
        foreach (DataSchemaDefinitionRule rule in definition.Rules ?? Enumerable.Empty<DataSchemaDefinitionRule>())
        {
            if (string.IsNullOrWhiteSpace(rule.ResultField))
                continue;

            string normalisedField = NormaliseResultField(rule.ResultField);
            string? result = _valueAccessor.GetValue(entry, normalisedField);

            if (string.IsNullOrWhiteSpace(result))
                continue;

            yield return new CtfKeyStageAssessment
            {
                Stage = rule.Stage,
                Locale = locale,
                Year = year,
                Subject = rule.Subject,
                Method = rule.Method,
                Component = rule.Component,
                ResultStatus = "R",
                ResultQualifier = rule.ResultQualifier,
                Result = result
            };
        }
    }

    private static string NormaliseResultField(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return field;

        if (field.StartsWith('$') && field.Contains('_'))
            return field[(field.IndexOf('_') + 1)..];

        return field;
    }
}

