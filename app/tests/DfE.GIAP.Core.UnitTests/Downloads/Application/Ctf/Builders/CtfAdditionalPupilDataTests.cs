using System.Globalization;
using System.Xml.Linq;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Builders;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Formatters;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Models;
using DfE.GIAP.Core.Downloads.Infrastructure.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Ctf.Builders;

public class CtfAdditionalPupilDataTests
{
    // Documents the assumed Cosmos contract and exercises deserialization, both mappings
    // and XML serialization together, so a missing assignment cannot silently drop data.
    private const string PupilJson = """
        {
          "UPN": "A123", "Surname": "Smith", "Forename": "Jane",
          "DOB": "2013-09-19", "Sex": "F",
          "NAWdetails": {
            "SpeakWelsh": "2", "HomeWelsh": "2", "NationalIdentity": "WAL",
            "EthnicitySource": "C", "WelshSource": "P", "EALAcquisition": "C",
            "LanguageSource": "C",
            "SENCurriculumandTeachingMethods": "CT1",
            "SENGroupingandSupport": "GS1", "SENSpecialisedResources": "SR1",
            "SENAdviceandAssessment": "AA1", "DateEntry": "2020-10-13",
            "FSMTransitionalProtection": false,
            "ALNDecisionOutcome": "IDP", "ALNDecisionBody": "SCH", "ALNSupportPlan": "C",
            "ALNAreasOfNeed": ["COIN", "BESD"], "ALNneeds": ["SLCD", "BESD"],
            "ALNLocalHealthBoardProvision": true, "IDPReviewDate": "2026-10-01"
          },
          "FSMhistory": [
            {
              "FSMstartDate": "2020-10-24", "FSMEligibilityVerificationDate": "2026-06-01",
              "FSMendDate": "2026-07-31", "UKcountry": "ENG", "FSMCategory": "TFSM"
            },
            { "FSMstartDate": "2026-09-01", "UKcountry": "WLS" }
          ],
          "MTC": [{ "ACADYR": "2025/2026", "FormMark": "20" }]
        }
        """;

    [Fact]
    public async Task Download_PreservesAllAdditionalData_InCtfOrder_AlongsideAssessments()
    {
        XElement pupil = await ExportPupilAsync(PupilJson);

        Assert.Equal(new[] { "UPN", "Surname", "Forename", "DOB", "Sex", "FSMhistory", "NAWdetails", "StageAssessments" },
            pupil.Elements().Select(e => e.Name.LocalName));
        XElement expectedFsm = XElement.Parse("""
            <FSMhistory>
              <FSMinstance>
                <FSMstartDate>2020-10-24</FSMstartDate>
                <FSMEligibilityVerificationDate>2026-06-01</FSMEligibilityVerificationDate>
                <FSMendDate>2026-07-31</FSMendDate>
                <UKcountry>ENG</UKcountry>
                <FSMCategory>TFSM</FSMCategory>
              </FSMinstance>
              <FSMinstance>
                <FSMstartDate>2026-09-01</FSMstartDate>
                <UKcountry>WLS</UKcountry>
              </FSMinstance>
            </FSMhistory>
            """);
        XElement expectedNaw = XElement.Parse("""
            <NAWdetails>
              <SpeakWelsh>2</SpeakWelsh>
              <HomeWelsh>2</HomeWelsh>
              <NationalIdentity>WAL</NationalIdentity>
              <EthnicitySource>C</EthnicitySource>
              <WelshSource>P</WelshSource>
              <EALAcquisition>C</EALAcquisition>
              <LanguageSource>C</LanguageSource>
              <SENCurriculumandTeachingMethods>CT1</SENCurriculumandTeachingMethods>
              <SENGroupingandSupport>GS1</SENGroupingandSupport>
              <SENSpecialisedResources>SR1</SENSpecialisedResources>
              <SENAdviceandAssessment>AA1</SENAdviceandAssessment>
              <DateEntry>2020-10-13</DateEntry>
              <FSMTransitionalProtection>false</FSMTransitionalProtection>
              <ALNDecisionOutcome>IDP</ALNDecisionOutcome>
              <ALNDecisionBody>SCH</ALNDecisionBody>
              <ALNSupportPlan>C</ALNSupportPlan>
              <ALNAreasOfNeed>
                <ALNAreaOfNeed>COIN</ALNAreaOfNeed>
                <ALNAreaOfNeed>BESD</ALNAreaOfNeed>
              </ALNAreasOfNeed>
              <ALNneeds><ALNType>SLCD</ALNType><ALNType>BESD</ALNType></ALNneeds>
              <ALNLocalHealthBoardProvision>true</ALNLocalHealthBoardProvision>
              <IDPReviewDate>2026-10-01</IDPReviewDate>
            </NAWdetails>
            """);
        Assert.True(XNode.DeepEquals(expectedFsm, pupil.Element("FSMhistory")));
        Assert.True(XNode.DeepEquals(expectedNaw, pupil.Element("NAWdetails")));
        XElement assessment = Assert.Single(pupil.Descendants("StageAssessment"));
        Assert.Equal("20", assessment.Element("Result")?.Value);
        Assert.Equal("2026", assessment.Element("Year")?.Value);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"NAWdetails\":null,\"FSMhistory\":null}")]
    [InlineData("{\"FSMhistory\":[]}")]
    public async Task Download_OmitsAbsentSections(string json)
    {
        XElement pupil = await ExportPupilAsync(json);
        Assert.Null(pupil.Element("NAWdetails"));
        Assert.Null(pupil.Element("FSMhistory"));
    }

    [Fact]
    public async Task Download_OmitsEmptyOptionalAlnNeeds()
    {
        JObject source = JObject.Parse(PupilJson);
        source["NAWdetails"]!["ALNneeds"] = new JArray();

        XElement pupil = await ExportPupilAsync(source.ToString());

        Assert.Null(pupil.Element("NAWdetails")?.Element("ALNneeds"));
        Assert.NotNull(pupil.Element("NAWdetails")?.Element("ALNAreasOfNeed"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Download_PreservesOptionalBoolean_AndOmitsMissingOptionalFields(bool? provision)
    {
        JObject source = JObject.Parse(PupilJson);
        JObject details = (JObject)source["NAWdetails"]!;
        string[] optionalFields = ["LanguageSource", "SENCurriculumandTeachingMethods",
            "SENGroupingandSupport", "SENSpecialisedResources", "SENAdviceandAssessment",
            "DateEntry", "ALNneeds", "IDPReviewDate"];
        foreach (string field in optionalFields)
            details.Remove(field);
        details["ALNLocalHealthBoardProvision"] = provision.HasValue ? new JValue(provision.Value) : JValue.CreateNull();

        XElement pupil = await ExportPupilAsync(source.ToString());
        XElement naw = pupil.Element("NAWdetails")!;
        foreach (string field in optionalFields)
            Assert.Null(naw.Element(field));
        Assert.Equal(provision.HasValue ? (provision.Value ? "true" : "false") : null,
            naw.Element("ALNLocalHealthBoardProvision")?.Value);
        Assert.Equal("false", naw.Element("FSMTransitionalProtection")?.Value);
    }

    [Fact]
    public async Task Download_FormatsDatesUsingGregorianCalendar()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
            XElement pupil = await ExportPupilAsync(PupilJson);
            Assert.Equal("2026-10-01", pupil.Element("NAWdetails")?.Element("IDPReviewDate")?.Value);
            Assert.Equal("2020-10-24", pupil.Descendants("FSMstartDate").First().Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    private static async Task<XElement> ExportPupilAsync(string json)
    {
        NationalPupilDto dto = JsonConvert.DeserializeObject<NationalPupilDto>(json)!;
        NationalPupil pupil = new NationalPupilDtoToEntityMapper().Map(dto);
        Mock<INationalPupilReadOnlyRepository> repository = new();
        repository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { pupil });
        Mock<IDataSchemaProvider> schemas = new();
        schemas.Setup(s => s.GetSchemaByYearAsync(2026)).ReturnsAsync(new DataSchemaDefinition
        {
            Year = "2026",
            Rules = [new() { Stage = "KS2", Subject = "MAT", Method = "TT", Component = "MTC", ResultQualifier = "MT", ResultField = "FormMark" }]
        });
        YearlyFileCtfPupilBuilder builder = new(repository.Object, schemas.Object, new CachePropertyValueAccessor());
        IEnumerable<CtfPupil> pupils = await builder.BuildAsync(["A123"]);
        using MemoryStream stream = new();
        await new XmlCtfFormatter().FormatAsync(new CtfFile(new CtfHeader { CtfVersion = "26.0" }, pupils), stream);
        stream.Position = 0;
        XDocument document = XDocument.Load(stream);
        return Assert.Single(document.Descendants("Pupil"));
    }
}
