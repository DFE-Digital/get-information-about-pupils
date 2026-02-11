using System.Text;
using System.Xml.Linq;
using DfE.GIAP.Core.Downloads.Application.Ctf;

namespace DfE.GIAP.Core.Downloads.Application.FileExports;

public class XmlCtfFormatter : ICtfFormatter
{
    public string ContentType => "application/xml";

    public byte[] Format(CtfFile ctfFile)
    {
        XDocument xml = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("CTfile",
                BuildHeader(ctfFile.Header),
                BuildPupilData(ctfFile.Pupils)
            )
        );

        return Encoding.UTF8.GetBytes(xml.ToString());
    }

    private static XElement BuildHeader(CtfHeader header)
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
