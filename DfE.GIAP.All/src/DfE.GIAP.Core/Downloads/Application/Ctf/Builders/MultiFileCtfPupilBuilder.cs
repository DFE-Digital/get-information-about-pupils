using System.Linq.Expressions;
using System.Reflection;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

public class MultiFileCtfPupilBuilder : ICtfPupilBuilder
{
    private readonly INationalPupilReadOnlyRepository _nationalPupilReadOnlyRepository;
    private readonly IBlobStorageProvider _blobStorageProvider;

    public MultiFileCtfPupilBuilder(
        INationalPupilReadOnlyRepository nationalPupilReadOnlyRepository,
        IBlobStorageProvider blobStorageProvider)
    {
        ArgumentNullException.ThrowIfNull(nationalPupilReadOnlyRepository);
        ArgumentNullException.ThrowIfNull(blobStorageProvider);
        _nationalPupilReadOnlyRepository = nationalPupilReadOnlyRepository;
        _blobStorageProvider = blobStorageProvider;
    }

    public async Task<IEnumerable<CtfPupil>> Build(IEnumerable<string> selectedPupilIds)
    {
        IEnumerable<NationalPupil> pupils =
            await _nationalPupilReadOnlyRepository.GetPupilsByIdsAsync(selectedPupilIds);

        // Keyed by logical file key: EYF, Phonics, KS1, KS2, MTC
        Dictionary<string, StageDefinition> stageDefinitions = await LoadStageDefinitionsAsync();

        List<CtfPupil> ctfPupils = new();
        foreach (NationalPupil pupil in pupils)
        {
            CtfPupil ctfPupil = new()
            {
                UPN = pupil.Upn ?? "",
                Surname = pupil.Surname ?? "",
                Forename = pupil.Forename ?? "",
                DOB = pupil.DOB.ToString("yyyy-MM-dd"),
                Sex = pupil.Sex
            };

            ProcessStage(pupil.HasEYFSPData, pupil.EarlyYearsFoundationStageProfile, "EYF", stageDefinitions, ctfPupil);
            ProcessStage(pupil.HasPhonicsData, pupil.Phonics, "Phonics", stageDefinitions, ctfPupil);
            ProcessStage(pupil.HasKeyStage1Data, pupil.KeyStage1, "KS1", stageDefinitions, ctfPupil);
            ProcessStage(pupil.HasKeyStage2Data, pupil.KeyStage2, "KS2", stageDefinitions, ctfPupil);
            ProcessStage(pupil.HasMtcData, pupil.MTC, "MTC", stageDefinitions, ctfPupil);

            ctfPupils.Add(ctfPupil);
        }

        return ctfPupils;
    }

    private void ProcessStage<T>(
        bool hasData,
        IEnumerable<T>? entries,
        string stageKey, // EYF / Phonics / KS1 / KS2 / MTC (file key)
        Dictionary<string, StageDefinition> stageDefinitions,
        CtfPupil ctfPupil)
    {
        if (!hasData || entries is null)
            return;

        T? latestEntry = entries
            .OrderByDescending(e => ToAcademicYearEnd(GetAcadYr(e!)))
            .FirstOrDefault();

        if (latestEntry is null)
            return;

        int year = ToAcademicYearEnd(GetAcadYr(latestEntry));
        string yearString = year.ToString();

        if (!stageDefinitions.TryGetValue(stageKey, out StageDefinition? stageDef))
            return;

        List<OutcomeDefinition> applicableRules = stageDef.Rules
            .Where(r => r.AppliesFrom <= year && (r.AppliesTo == null || r.AppliesTo >= year))
            .ToList();

        foreach (OutcomeDefinition rule in applicableRules)
        {
            if (string.IsNullOrWhiteSpace(rule.ResultField))
                continue;

            string? result = GetValue(latestEntry, rule.ResultField);
            if (string.IsNullOrWhiteSpace(result))
                continue;

            ctfPupil.Assessments.Add(new CtfKeyStageAssessment
            {
                Stage = stageDef.Stage,
                Locale = "ENG",
                Year = yearString,
                Subject = rule.Subject,
                Method = rule.Method,
                Component = rule.Component,
                ResultStatus = "R",
                ResultQualifier = rule.ResultQualifier,
                Result = result
            });
        }
    }

    private string GetAcadYr(object entry)
    {
        PropertyInfo? prop = entry.GetType().GetProperty("ACADYR")
            ?? entry.GetType().GetProperty("AcademicYear");

        return prop?.GetValue(entry)?.ToString() ?? "";
    }

    private async Task<Dictionary<string, StageDefinition>> LoadStageDefinitionsAsync()
    {
        // Key by logical file key, not by rule.Stage
        Dictionary<string, StageDefinition> result = new(StringComparer.OrdinalIgnoreCase);

        // fileKey -> blob file name
        Dictionary<string, string> stageFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "EYF", "eyf.json" },
            { "FSP", "fsp.json" },
            { "KS1", "ks1.json" },
            { "KS2", "ks2.json" },
            { "KS3", "ks3.json" },
            { "MTC", "mtc.json" },
            { "Phonics", "phonics.json" },
        };

        foreach ((string key, string file) in stageFiles)
        {
            using Stream stream = await _blobStorageProvider
                .DownloadBlobAsStreamAsync("giapdownloads", $"CTF/{file}");

            using StreamReader reader = new(stream);
            string json = await reader.ReadToEndAsync();

            StageDefinition? def = JsonConvert.DeserializeObject<StageDefinition>(json);
            if (def is not null)
                result[key] = def; // key is EYF / Phonics / KS1 / KS2 / MTC
        }

        return result;
    }

    public int ToAcademicYearEnd(string? acadYr)
    {
        if (string.IsNullOrWhiteSpace(acadYr))
            return 0;

        string[] parts = acadYr.Split('/');
        if (parts.Length != 2)
            return 0;

        return int.TryParse(parts[1], out int endYear) ? endYear : 0;
    }

    private static readonly Dictionary<(Type, string), Func<object, string?>> _accessorCache = new();

    private string? GetValue(object entry, string field)
    {
        Type type = entry.GetType();
        (Type type, string field) key = (type, field);

        if (!_accessorCache.TryGetValue(key, out Func<object, string?>? getter))
        {
            PropertyInfo? prop = type.GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop is null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(object), "obj");
            UnaryExpression cast = Expression.Convert(param, type);
            MemberExpression access = Expression.Property(cast, prop);

            Expression body = access.Type == typeof(string)
                ? access
                : Expression.Call(access, "ToString", Type.EmptyTypes);

            getter = Expression.Lambda<Func<object, string?>>(body, param).Compile();
            _accessorCache[key] = getter;
        }

        return getter(entry);
    }
}

public class StageDefinition
{
    // This Stage value comes from the JSON but is not used as the dictionary key anymore
    public string Stage { get; set; } = "";
    public List<OutcomeDefinition> Rules { get; set; } = new();
}

public class OutcomeDefinition
{
    public string Stage { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Method { get; set; } = "";
    public string Component { get; set; } = "";
    public string ComponentDesc { get; set; } = "";
    public string ResultQualifier { get; set; } = "";
    public string ResultField { get; set; } = "";
    public string ResultType { get; set; } = "";
    public string ResultValues { get; set; } = "";
    public string ResultDesc { get; set; } = "";
    public int AppliesFrom { get; set; }
    public int? AppliesTo { get; set; }
}
