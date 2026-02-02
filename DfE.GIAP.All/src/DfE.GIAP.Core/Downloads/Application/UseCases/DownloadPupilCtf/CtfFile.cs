using System.Text;
using System.Xml.Linq;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

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
    public DateTime DOB { get; set; }
    public string? Gender { get; set; }
    public string? Sex { get; set; }

    public List<CtfKeyStageAssessment> Assessments { get; set; } = new();
}

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




public interface ICtfBuilder
{
    CtfFile Build(CtfHeader header, IEnumerable<CtfPupil> pupils);
}

public class CtfBuilder : ICtfBuilder
{
    public CtfFile Build(CtfHeader header, IEnumerable<CtfPupil> pupils)
    {
        return new CtfFile
        {
            Header = header,
            Pupils = pupils.ToList()
        };
    }
}


public interface ICtfSerializer
{
    byte[] Serialize(CtfFile file);
    string ContentType { get; }
}

public class XmlCtfSerializer : ICtfSerializer
{
    public string ContentType => "application/xml";
    public byte[] Serialize(CtfFile file)
    {
        XDocument xml = new XDocument(
            new XElement("CTfile",
            BuildHeader(file.Header),
            BuildPupilData(file.Pupils)));

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
            new XElement("DOB", pupil.DOB.ToString("yyyy-MM-dd")),
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


public interface IPupilCtfAggregator
{
    Task<PupilCtfAggregate> AggregateAsync(IEnumerable<string> selectedPupilIds);
}

public class PupilCtfAggregator : IPupilCtfAggregator
{
    //private readonly IPupilRepository _pupilRepository;
    //private readonly IDataDefinitionBlobClient _dataDefinitionClient;

    public PupilCtfAggregator(
        //IPupilRepository pupilRepository,
        //IDataDefinitionBlobClient dataDefinitionClient
        )
    {
        //_pupilRepository = pupilRepository;
        //_dataDefinitionClient = dataDefinitionClient;
    }

    public async Task<PupilCtfAggregate> AggregateAsync(IEnumerable<string> selectedPupilIds)
    {
        //var pupils = await _pupilRepository.GetPupilsByIdentifiersAsync(selectedPupilIds);
        //var definitions = await _dataDefinitionClient.GetDataDefinitionsAsync();

        // Transform repository models → CTF domain models
        //var ctfPupils = pupils.Select(MapToCtfPupil).ToList();

        //var header = BuildHeader(definitions);

        //return new PupilCtfAggregate
        //{
        //    Header = header,
        //    Pupils = ctfPupils
        //};

        throw new NotImplementedException();
    }

    private CtfPupil MapToCtfPupil(Pupil pupil)
    {
        // mapping logic / Inject mappers
        throw new NotImplementedException();
    }

    private CtfHeader BuildHeader(List<DataDefinition> definitions)
    {
        // header construction logic here
        throw new NotImplementedException();
    }
}

public class PupilCtfAggregate
{
    public CtfHeader Header { get; init; } = new();
    public List<CtfPupil> Pupils { get; init; } = new();

    // Optional: if we want to expose helpers later
    public bool HasPupils => Pupils.Count > 0;
}



public interface ICtfFileNameProvider
{
    string GenerateFileName();
}

public class DefaultCtfFileNameProvider : ICtfFileNameProvider
{
    public string GenerateFileName()
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return $"pupil_ctf_{timestamp}.xml";
    }
}
