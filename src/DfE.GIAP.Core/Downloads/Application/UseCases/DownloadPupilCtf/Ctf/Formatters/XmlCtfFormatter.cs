using System.Globalization;
using System.Text;
using System.Xml;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Formatters;

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

            await WriteFsmHistoryAsync(writer, pupil.FsmHistory);
            await WriteNawDetailsAsync(writer, pupil.NawDetails);
            await WriteAssessmentsAsync(writer, pupil.Assessments);

            await writer.WriteEndElementAsync(); // Pupil
        }

        await writer.WriteEndElementAsync(); // CTFpupilData
    }

    private static async Task WriteFsmHistoryAsync(XmlWriter writer, List<FsmInstanceEntry>? history)
    {
        if (history is not { Count: > 0 })
            return;

        await writer.WriteStartElementAsync(null, "FSMhistory", null);
        foreach (FsmInstanceEntry instance in history)
        {
            await writer.WriteStartElementAsync(null, "FSMinstance", null);
            await WriteElementIfNotNullAsync(writer, "FSMstartDate", FormatDate(instance.FSMstartDate));
            await WriteElementIfNotNullAsync(writer, "FSMEligibilityVerificationDate", FormatDate(instance.FSMEligibilityVerificationDate));
            await WriteElementIfNotNullAsync(writer, "FSMendDate", FormatDate(instance.FSMendDate));
            await WriteElementIfNotNullAsync(writer, "UKcountry", instance.UKcountry);
            await WriteElementIfNotNullAsync(writer, "FSMCategory", instance.FSMCategory);
            await writer.WriteEndElementAsync(); // FSMinstance
        }
        await writer.WriteEndElementAsync(); // FSMhistory
    }

    private static async Task WriteNawDetailsAsync(XmlWriter writer, NawDetailsEntry? details)
    {
        if (details is null)
            return;

        await writer.WriteStartElementAsync(null, "NAWdetails", null);
        await WriteElementIfNotNullAsync(writer, "SpeakWelsh", details.SpeakWelsh);
        await WriteElementIfNotNullAsync(writer, "HomeWelsh", details.HomeWelsh);
        await WriteElementIfNotNullAsync(writer, "NationalIdentity", details.NationalIdentity);
        await WriteElementIfNotNullAsync(writer, "EthnicitySource", details.EthnicitySource);
        await WriteElementIfNotNullAsync(writer, "WelshSource", details.WelshSource);
        await WriteElementIfNotNullAsync(writer, "EALAcquisition", details.EALAcquisition);
        await WriteElementIfNotNullAsync(writer, "LanguageSource", details.LanguageSource);
        await WriteElementIfNotNullAsync(writer, "SENCurriculumandTeachingMethods", details.SENCurriculumandTeachingMethods);
        await WriteElementIfNotNullAsync(writer, "SENGroupingandSupport", details.SENGroupingandSupport);
        await WriteElementIfNotNullAsync(writer, "SENSpecialisedResources", details.SENSpecialisedResources);
        await WriteElementIfNotNullAsync(writer, "SENAdviceandAssessment", details.SENAdviceandAssessment);
        await WriteElementIfNotNullAsync(writer, "DateEntry", FormatDate(details.DateEntry));
        await WriteElementIfNotNullAsync(writer, "FSMTransitionalProtection", FormatBoolean(details.FSMTransitionalProtection));
        await WriteElementIfNotNullAsync(writer, "ALNDecisionOutcome", details.ALNDecisionOutcome);
        await WriteElementIfNotNullAsync(writer, "ALNDecisionBody", details.ALNDecisionBody);
        await WriteElementIfNotNullAsync(writer, "ALNSupportPlan", details.ALNSupportPlan);
        await WriteCodesAsync(writer, "ALNAreasOfNeed", "ALNAreaOfNeed", details.ALNAreasOfNeed);
        await WriteCodesAsync(writer, "ALNneeds", "ALNType", details.ALNneeds);
        await WriteElementIfNotNullAsync(writer, "ALNLocalHealthBoardProvision", FormatBoolean(details.ALNLocalHealthBoardProvision));
        await WriteElementIfNotNullAsync(writer, "IDPReviewDate", FormatDate(details.IDPReviewDate));
        await writer.WriteEndElementAsync(); // NAWdetails
    }

    private static async Task WriteCodesAsync(XmlWriter writer, string container, string element, List<string>? codes)
    {
        if (codes is not { Count: > 0 })
            return;

        await writer.WriteStartElementAsync(null, container, null);
        foreach (string code in codes)
            await WriteElementIfNotNullAsync(writer, element, code);
        await writer.WriteEndElementAsync();
    }

    private static string? FormatDate(DateTime? date) =>
        date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string? FormatBoolean(bool? value) =>
        value.HasValue ? XmlConvert.ToString(value.Value) : null;

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
