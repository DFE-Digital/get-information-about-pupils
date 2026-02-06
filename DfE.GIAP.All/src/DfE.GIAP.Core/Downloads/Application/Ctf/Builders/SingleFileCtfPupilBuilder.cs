using System.Linq.Expressions;
using System.Reflection;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

public class SingleFileCtfPupilBuilder : ICtfPupilBuilder
{
    private readonly INationalPupilReadOnlyRepository _nationalPupilReadOnlyRepository;
    private readonly IBlobStorageProvider _blobStorageProvider;

    public SingleFileCtfPupilBuilder(
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
        IEnumerable<NationalPupil> pupils = await _nationalPupilReadOnlyRepository.GetPupilsByIdsAsync(selectedPupilIds);
        IReadOnlyList<DataMapperDefinition> mapperDefinitions = await LoadMapperDefinitionsAsync();
        DataMapperDefinition latestDefinition = mapperDefinitions
            .OrderByDescending(d => int.Parse(d.Year!))
            .First();

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

            if (pupil.HasEYFSPData)
            {
                EarlyYearsFoundationStageProfileEntry? latestEntry = pupil.EarlyYearsFoundationStageProfile!
                    .OrderByDescending(e => ToAcademicYearEnd(e.ACADYR))
                    .FirstOrDefault();

                if (latestEntry is not null)
                {
                    string year = ToAcademicYearEnd(latestEntry.ACADYR).ToString();
                    ctfPupil.Assessments.AddRange(
                        BuildStageAssessments(latestEntry, latestDefinition, "ENG", year));
                }
            }

            if (pupil.HasPhonicsData)
            {
                PhonicsEntry? latestEntry = pupil.Phonics!
                    .OrderByDescending(e => ToAcademicYearEnd(e.AcademicYear))
                    .FirstOrDefault();

                if (latestEntry is not null)
                {
                    string year = ToAcademicYearEnd(latestEntry.AcademicYear).ToString();
                    ctfPupil.Assessments.AddRange(
                        BuildStageAssessments(latestEntry, latestDefinition, "ENG", year));
                }

            }

            if (pupil.HasKeyStage1Data)
            {
                KeyStage1Entry? latestEntry = pupil.KeyStage1!
                    .OrderByDescending(e => ToAcademicYearEnd(e.ACADYR))
                    .FirstOrDefault();

                if (latestEntry is not null)
                {
                    string year = ToAcademicYearEnd(latestEntry.ACADYR).ToString();
                    ctfPupil.Assessments.AddRange(
                        BuildStageAssessments(latestEntry, latestDefinition, "ENG", year));
                }
            }

            if (pupil.HasKeyStage2Data)
            {
                KeyStage2Entry? latestEntry = pupil.KeyStage2!
                    .OrderByDescending(e => ToAcademicYearEnd(e.ACADYR))
                    .FirstOrDefault();

                if (latestEntry is not null)
                {
                    string year = ToAcademicYearEnd(latestEntry.ACADYR).ToString();
                    ctfPupil.Assessments.AddRange(
                        BuildStageAssessments(latestEntry, latestDefinition, "ENG", year));
                }
            }

            if (pupil.HasMtcData)
            {
                MtcEntry? latestEntry = pupil.MTC!
                    .OrderByDescending(e => ToAcademicYearEnd(e.ACADYR))
                    .FirstOrDefault();

                if (latestEntry is not null)
                {
                    string year = ToAcademicYearEnd(latestEntry.ACADYR).ToString();
                    ctfPupil.Assessments.AddRange(
                        BuildStageAssessments(latestEntry, latestDefinition, "ENG", year));
                }
            }

            ctfPupils.Add(ctfPupil);
        }

        return ctfPupils;
    }



    private async Task<IReadOnlyList<DataMapperDefinition>> LoadMapperDefinitionsAsync()
    {
        IEnumerable<BlobItemMetadata> blobs =
            await _blobStorageProvider.ListBlobsWithMetadataAsync("giapdownloads", "CTF");

        IEnumerable<Task<DataMapperDefinition>> tasks = blobs.Select(async blob =>
        {
            using Stream stream = await _blobStorageProvider
                .DownloadBlobAsStreamAsync("giapdownloads", blob.Name!);

            using StreamReader reader = new(stream);
            string json = await reader.ReadToEndAsync();

            return JsonConvert.DeserializeObject<DataMapperDefinition>(json) ?? new();
        });

        return await Task.WhenAll(tasks);
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


    private IEnumerable<CtfKeyStageAssessment> BuildStageAssessments(
        object entry,
        DataMapperDefinition definition,
        string locale,
        string year)
    {
        foreach (DataMapperDefinitionRule rule in definition.Rules ?? Enumerable.Empty<DataMapperDefinitionRule>())
        {
            if (string.IsNullOrWhiteSpace(rule.ResultField))
                continue;

            string? result = GetValue(entry, rule.ResultField);
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


    private static readonly Dictionary<(Type, string), Func<object, string?>> _accessorCache = new();
    private string? GetValue(object entry, string? field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return null;

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
