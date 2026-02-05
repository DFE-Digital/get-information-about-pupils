using System.Text;
using System.Xml.Linq;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public class CtfFile
{
    public CtfHeader Header { get; set; } = new();
    public List<CtfPupil> Pupils { get; set; } = new();
}

public class CtfHeader
{
    public string DocumentName { get; set; } = "Common Transfer File";
    public string CtfVersion { get; set; } = "25.0";
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public string DocumentQualifier { get; set; } = "partial";
    public string DataDescriptor { get; set; } = string.Empty;
    public string SupplierId { get; set; } = "GIAP";

    public CtfSchoolInfo SourceSchool { get; set; } = new();
    public CtfSchoolInfo DestSchool { get; set; } = new();
}

public class CtfSchoolInfo
{
    public string LEA { get; set; } = string.Empty;
    public string Estab { get; set; } = string.Empty;
    public string? SchoolName { get; set; }
    public string? AcademicYear { get; set; }
}

public class CtfPupil
{
    public string UPN { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Forename { get; set; } = string.Empty;
    public string DOB { get; set; } = string.Empty; // Parse as: yyyy-MM-dd
    public string? Sex { get; set; }

    public List<CtfKeyStageAssessment> Assessments { get; set; } = new();
}

// Either: EYFSP/Phonics & KS1/KS2/KS2 MTC
public class CtfKeyStageAssessment
{
    public string Stage { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string ResultStatus { get; set; } = string.Empty;
    public string ResultQualifier { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}




// OUTPUT FORMATTER
public interface ICtfFormatter
{
    string ContentType { get; }
    byte[] Format(CtfHeader header, IEnumerable<CtfPupil> pupils);
}

public class XmlCtfFormatter : ICtfFormatter
{
    public string ContentType => "application/xml";

    public byte[] Format(CtfHeader header, IEnumerable<CtfPupil> pupils)
    {
        XDocument xml = new XDocument(
            new XElement("CTfile",
                BuildHeader(header),
                BuildPupilData(pupils)
            )
        );

        return Encoding.UTF8.GetBytes(xml.ToString());
    }

    private XElement BuildHeader(CtfHeader header)
    {
        return new XElement("Header",
            new XElement("DocumentName", header.DocumentName),
            new XElement("CTFversion", header.CtfVersion),
            new XElement("DateTime", header.DateTime.ToString("yyyy-MM-ddTHH:mm:ss")),
            new XElement("DocumentQualifier", header.DocumentQualifier),
            new XElement("DataDescriptor", header.DataDescriptor),
            new XElement("SupplierID", header.SupplierId),
            new XElement("SourceSchool",
                new XElement("LEA", header.SourceSchool.LEA),
                new XElement("Estab", header.SourceSchool.Estab),
                new XElement("SchoolName", header.SourceSchool.SchoolName),
                new XElement("AcademicYear", header.SourceSchool.AcademicYear)
            ),
            new XElement("DestSchool",
                new XElement("LEA", header.DestSchool.LEA),
                new XElement("Estab", header.DestSchool.Estab)
            )
        );
    }

    private XElement BuildPupilData(IEnumerable<CtfPupil> pupils)
    {
        return new XElement("CTFpupilData",
            pupils.Select(BuildPupil)
        );
    }

    private XElement BuildPupil(CtfPupil pupil)
    {
        return new XElement("Pupil",
            new XElement("UPN", pupil.UPN),
            new XElement("Surname", pupil.Surname),
            new XElement("Forename", pupil.Forename),
            new XElement("DOB", pupil.DOB),
            new XElement("Sex", pupil.Sex),
            BuildAssessments(pupil.Assessments)
        );
    }

    private XElement BuildAssessments(IEnumerable<CtfKeyStageAssessment> assessments)
    {
        if (!assessments.Any())
            return new XElement("StageAssessments");

        return new XElement("StageAssessments",
            assessments.GroupBy(a => a.Stage).Select(stageGroup =>
                new XElement("KeyStage",
                    new XElement("Stage", stageGroup.Key),
                    stageGroup.Select(BuildAssessment)
                )
            )
        );
    }

    private XElement BuildAssessment(CtfKeyStageAssessment assessment)
    {
        return new XElement("StageAssessment",
            new XElement("Locale", assessment.Locale),
            new XElement("Year", assessment.Year),
            new XElement("Subject", assessment.Subject),
            new XElement("Method", assessment.Method),
            new XElement("Component", assessment.Component),
            new XElement("ResultStatus", assessment.ResultStatus),
            new XElement("ResultQualifier", assessment.ResultQualifier),
            new XElement("Result", assessment.Result)
        );
    }
}



// HEADER AGGREGATION
public interface ICtfHeaderBuilder
{
    CtfHeader Build(CtfHeaderContext context);
}

public class CtfHeaderBuilder : ICtfHeaderBuilder
{
    public const string DescriptorEstablishment =
        "This attainment data was obtained via the Searchable pupil data option of Get Information About Pupils";

    public const string DescriptorNonEstablishment =
        "This attainment data was obtained via the Get Information About Pupils school site";

    public CtfHeader Build(CtfHeaderContext context)
    {
        DateTime now = DateTime.UtcNow;

        return new CtfHeader
        {
            DocumentName = "Common Transfer File",
            CtfVersion = "25.0", // TODO: Currently coming from config, changes every september(ish)
            DateTime = now,
            DocumentQualifier = "partial",
            DataDescriptor = context.IsEstablishment
                ? DescriptorEstablishment
                : DescriptorNonEstablishment,
            SupplierId = "GIAP",

            SourceSchool = new CtfSchoolInfo
            {
                LEA = "XXX",
                Estab = "XXXX",
                SchoolName = "Get Information About Pupils",
                AcademicYear = CalculateAcademicYear(now)
            },

            DestSchool = new CtfSchoolInfo
            {
                LEA = context.IsEstablishment ? context.DestLEA : "XXX",
                Estab = context.IsEstablishment ? context.DestEstab : "XXXX"
            }
        };
    }

    private string CalculateAcademicYear(DateTime now)
    {
        int year = now.Month >= 9 ? now.Year : now.Year - 1;
        return year.ToString();
    }
}

public class CtfHeaderContext
{
    public bool IsEstablishment { get; set; }

    public string SourceLEA { get; set; } = string.Empty;
    public string SourceEstab { get; set; } = string.Empty;
    public string SourceSchoolName { get; set; } = string.Empty;

    public string DestLEA { get; set; } = string.Empty;
    public string DestEstab { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;
}



// PUPIL AGGREGATIONS
public interface ICtfPupilBuilder
{
    Task<IEnumerable<CtfPupil>> Build(IEnumerable<string> selectedPupilIds);
}

public class CtfPupilBuilder : ICtfPupilBuilder
{
    private readonly INationalPupilReadOnlyRepository _nationalPupilReadOnlyRepository;
    private readonly IBlobStorageProvider _blobStorageProvider;

    public CtfPupilBuilder(
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
        IEnumerable<NationalPupil> pupils = await _nationalPupilReadOnlyRepository
            .GetPupilsByIdsAsync(selectedPupilIds);

        IReadOnlyList<DataMapperDefinition> mapperDefinitions = await LoadMapperDefinitionsAsync();

        List<CtfPupil> ctfPupils = new();
        foreach (NationalPupil pupil in pupils)
        {
            CtfPupil ctfPupil = new()
            {
                UPN = pupil.Upn ?? string.Empty,
                Surname = pupil.Surname ?? string.Empty,
                Forename = pupil.Forename ?? string.Empty,
                DOB = pupil.DOB.ToString("yyyy-MM-dd"),
                Sex = pupil.Sex
            };

            // Add each stage assessment, only if they have data

            ctfPupils.Add(ctfPupil);
        }

        return ctfPupils;

        // Loop through each pupil - DONE
        // map basic pupil details to CTF Pupil - DONE
        // Map each assessment stage, ONLY if they have data
        //      Map EYFPS entity
        //      Map Phonics & KS1 (same time?)
        //      Map KS2
        //      Map MTC

        // MTC binding rules:
        // Sort MTC entries by ACADYR (a specific ACADYR date manipulation is done here)
        // Loops through sorted MTC entries
        //      Tracking the current MTC entries ACADYR year (same date manipulation)
        //      Get DEFINITIONS where Stage (ks2) & Year (entry year) match
        //      Makes entity into dictionary of string/value pair??
        //      Creates stage element


        // 3. Map repository pupils → CTF pupils using definitions
        // TODO: Figure out what mapping is required, and why. Scalable solution?
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

}

public class DataMapperDefinition
{
    public string? Year { get; set; }
    public List<DataMapperDefinitionRule>? Rules { get; set; }
}

public class DataMapperDefinitionRule
{
    public string? Stage { get; set; }
    public string? Subject { get; set; }
    public string? Method { get; set; }
    public string? Component { get; set; }
    public string? ResultQualifier { get; set; }
    public string? ResultField { get; set; }
}
