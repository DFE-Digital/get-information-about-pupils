using System.Text;
using System.Xml;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Formatters;

public class XmlCtfFormatter : ICtfFormatter
{
    public string ContentType => "application/xml";

    public async Task FormatAsync(CtfFile file, Stream output)
    {
        XmlWriterSettings settings = new()
        {
            Async = true,
            Encoding = Encoding.UTF8,
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(output, settings);

        await writer.WriteStartDocumentAsync();
        await writer.WriteStartElementAsync(null, "CTfile", null);

        await WriteHeaderAsync(writer, file.Header);
        await WritePupilsAsync(writer, file.Pupils);

        await writer.WriteEndElementAsync(); // CTfile
        await writer.WriteEndDocumentAsync();
    }

    private static async Task WriteHeaderAsync(XmlWriter writer, CtfHeader header)
    {
        await writer.WriteStartElementAsync(prefix: null, localName: "Header", ns: null);

        await WriteElementIfNotNullAsync(writer, "DocumentName", header.DocumentName);
        await WriteElementIfNotNullAsync(writer, "CTFversion", header.CtfVersion);
        await WriteElementIfNotNullAsync(writer, "DateTime", header.DateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
        await WriteElementIfNotNullAsync(writer, "DocumentQualifier", header.DocumentQualifier);
        await WriteElementIfNotNullAsync(writer, "DataDescriptor", header.DataDescriptor);
        await WriteElementIfNotNullAsync(writer, "SupplierID", header.SupplierId);

        await writer.WriteStartElementAsync(prefix: null, localName: "SourceSchool", ns: null);
        await WriteElementIfNotNullAsync(writer, "LEA", header.SourceSchool.LEA);
        await WriteElementIfNotNullAsync(writer, "Estab", header.SourceSchool.Estab);
        await WriteElementIfNotNullAsync(writer, "SchoolName", header.SourceSchool.SchoolName);
        await WriteElementIfNotNullAsync(writer, "AcademicYear", header.SourceSchool.AcademicYear);
        await writer.WriteEndElementAsync();

        await writer.WriteStartElementAsync(prefix: null, localName: "DestSchool", ns: null);
        await WriteElementIfNotNullAsync(writer, "LEA", header.DestSchool.LEA);
        await WriteElementIfNotNullAsync(writer, "Estab", header.DestSchool.Estab);
        await writer.WriteEndElementAsync();

        await writer.WriteEndElementAsync(); // Header
    }

    private async Task WritePupilsAsync(XmlWriter writer, IEnumerable<CtfPupil> pupils)
    {
        await writer.WriteStartElementAsync(prefix: null, localName: "CTFpupilData", ns: null);

        foreach (CtfPupil pupil in pupils)
        {
            await writer.WriteStartElementAsync(prefix: null, localName: "Pupil", ns: null);

            await WriteElementIfNotNullAsync(writer, "UPN", pupil.UPN);
            await WriteElementIfNotNullAsync(writer, "Surname", pupil.Surname);
            await WriteElementIfNotNullAsync(writer, "Forename", pupil.Forename);
            await WriteElementIfNotNullAsync(writer, "DOB", pupil.DOB);
            await WriteElementIfNotNullAsync(writer, "Sex", pupil.Sex);

            await WriteAssessmentsAsync(writer, pupil.Assessments);

            await writer.WriteEndElementAsync(); // Pupil
        }

        await writer.WriteEndElementAsync(); // CTFpupilData
    }

    private static async Task WriteAssessmentsAsync(XmlWriter writer, IEnumerable<CtfKeyStageAssessment> assessments)
    {
        await writer.WriteStartElementAsync(prefix: null, localName: "StageAssessments", ns: null);

        foreach (IGrouping<string, CtfKeyStageAssessment> group in assessments.GroupBy(a => a.Stage))
        {
            await writer.WriteStartElementAsync(prefix: null, localName: "KeyStage", ns: null);

            await WriteElementIfNotNullAsync(writer, "Stage", group.Key);

            foreach (CtfKeyStageAssessment assessment in group)
            {
                await writer.WriteStartElementAsync(prefix: null, localName: "StageAssessment", ns: null);

                await WriteElementIfNotNullAsync(writer, "Locale", assessment.Locale);
                await WriteElementIfNotNullAsync(writer, "Year", assessment.Year);
                await WriteElementIfNotNullAsync(writer, "Subject", assessment.Subject);
                await WriteElementIfNotNullAsync(writer, "Method", assessment.Method);
                await WriteElementIfNotNullAsync(writer, "Component", assessment.Component);
                await WriteElementIfNotNullAsync(writer, "ResultStatus", assessment.ResultStatus);
                await WriteElementIfNotNullAsync(writer, "ResultQualifier", assessment.ResultQualifier);
                await WriteElementIfNotNullAsync(writer, "Result", assessment.Result);

                await writer.WriteEndElementAsync(); // StageAssessment
            }

            await writer.WriteEndElementAsync(); // KeyStage
        }

        await writer.WriteEndElementAsync(); // StageAssessments
    }


    private static async Task WriteElementIfNotNullAsync(
        XmlWriter writer,
        string name,
        string? value)
    {
        if (value is null)
            return;

        await writer.WriteElementStringAsync(
            prefix: null,
            localName: name,
            ns: null,
            value: value
        );
    }
}
