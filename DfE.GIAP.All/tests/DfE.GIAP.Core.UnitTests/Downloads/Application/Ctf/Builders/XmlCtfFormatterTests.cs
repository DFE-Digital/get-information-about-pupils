using System.Text;
using DfE.GIAP.Core.Downloads.Application.Ctf.Formatters;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Ctf.Builders;

public class XmlCtfFormatterTests
{
    private static string ReadXml(Stream stream)
    {
        stream.Position = 0;
        using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    [Fact]
    public async Task FormatAsync_WritesRootElement()
    {
        // Arrange
        XmlCtfFormatter formatter = new XmlCtfFormatter();
        MemoryStream stream = new MemoryStream();

        CtfHeader header = new CtfHeader();
        List<CtfPupil> pupils = new List<CtfPupil>();

        CtfFile file = new CtfFile(header, pupils);

        // Act
        await formatter.FormatAsync(file, stream);
        string xml = ReadXml(stream);

        // Assert
        Assert.Contains("<CTfile>", xml);
        Assert.Contains("</CTfile>", xml);
    }

    [Fact]
    public async Task FormatAsync_WritesHeaderValues()
    {
        // Arrange
        XmlCtfFormatter formatter = new XmlCtfFormatter();
        MemoryStream stream = new MemoryStream();

        CtfHeader header = new CtfHeader
        {
            DocumentName = "Common Transfer File",
            CtfVersion = "25.0",
            DateTime = new DateTime(2024, 2, 1, 10, 30, 0),
            DocumentQualifier = "partial",
            DataDescriptor = "CTF",
            SupplierId = "GIAP",
            SourceSchool = new CtfSchoolInfo
            {
                LEA = "123",
                Estab = "4567",
                SchoolName = "Source School",
                AcademicYear = "2023"
            },
            DestSchool = new CtfSchoolInfo
            {
                LEA = "999",
                Estab = "8888"
            }
        };

        CtfFile file = new CtfFile(header, new List<CtfPupil>());

        // Act
        await formatter.FormatAsync(file, stream);
        string xml = ReadXml(stream);

        // Assert
        Assert.Contains("<Header>", xml);
        Assert.Contains("<DocumentName>Common Transfer File</DocumentName>", xml);
        Assert.Contains("<CTFversion>25.0</CTFversion>", xml);
        Assert.Contains("<DateTime>2024-02-01T10:30:00</DateTime>", xml);
        Assert.Contains("<DocumentQualifier>partial</DocumentQualifier>", xml);
        Assert.Contains("<DataDescriptor>CTF</DataDescriptor>", xml);
        Assert.Contains("<SupplierID>GIAP</SupplierID>", xml);

        Assert.Contains("<SourceSchool>", xml);
        Assert.Contains("<LEA>123</LEA>", xml);
        Assert.Contains("<Estab>4567</Estab>", xml);
        Assert.Contains("<SchoolName>Source School</SchoolName>", xml);
        Assert.Contains("<AcademicYear>2023</AcademicYear>", xml);

        Assert.Contains("<DestSchool>", xml);
        Assert.Contains("<LEA>999</LEA>", xml);
        Assert.Contains("<Estab>8888</Estab>", xml);
    }

    [Fact]
    public async Task FormatAsync_WritesSinglePupil()
    {
        // Arrange
        XmlCtfFormatter formatter = new XmlCtfFormatter();
        MemoryStream stream = new MemoryStream();

        CtfPupil pupil = new CtfPupil
        {
            UPN = "A123456789",
            Surname = "Doe",
            Forename = "Jane",
            DOB = "2011-05-10",
            Sex = "F",
            Assessments = new List<CtfKeyStageAssessment>()
        };

        CtfFile file = new CtfFile(new CtfHeader(), new List<CtfPupil> { pupil });

        // Act
        await formatter.FormatAsync(file, stream);
        string xml = ReadXml(stream);

        // Assert
        Assert.Contains("<Pupil>", xml);
        Assert.Contains("<UPN>A123456789</UPN>", xml);
        Assert.Contains("<Surname>Doe</Surname>", xml);
        Assert.Contains("<Forename>Jane</Forename>", xml);
        Assert.Contains("<DOB>2011-05-10</DOB>", xml);
        Assert.Contains("<Sex>F</Sex>", xml);
    }

    [Fact]
    public async Task FormatAsync_WritesAssessmentsGroupedByStage()
    {
        // Arrange
        XmlCtfFormatter formatter = new XmlCtfFormatter();
        MemoryStream stream = new MemoryStream();

        List<CtfKeyStageAssessment> assessments = new List<CtfKeyStageAssessment>
        {
            new CtfKeyStageAssessment
            {
                Stage = "KS1",
                Locale = "ENG",
                Year = "2020",
                Subject = "Maths",
                Method = "T",
                Component = "C1",
                ResultStatus = "R",
                ResultQualifier = "Q",
                Result = "W"
            },
            new CtfKeyStageAssessment
            {
                Stage = "KS1",
                Locale = "ENG",
                Year = "2020",
                Subject = "Reading",
                Method = "T",
                Component = "C2",
                ResultStatus = "R",
                ResultQualifier = "Q",
                Result = "W"
            }
        };

        CtfPupil pupil = new CtfPupil
        {
            UPN = "X",
            Surname = "Y",
            Forename = "Z",
            DOB = "2000-01-01",
            Sex = "M",
            Assessments = assessments
        };

        CtfFile file = new CtfFile(new CtfHeader(), new List<CtfPupil> { pupil });

        // Act
        await formatter.FormatAsync(file, stream);
        string xml = ReadXml(stream);

        // Assert
        Assert.Contains("<KeyStage>", xml);
        Assert.Contains("<Stage>KS1</Stage>", xml);
        Assert.Contains("<Subject>Maths</Subject>", xml);
        Assert.Contains("<Subject>Reading</Subject>", xml);
    }

    [Fact]
    public async Task FormatAsync_SkipsNullValues()
    {
        // Arrange
        XmlCtfFormatter formatter = new XmlCtfFormatter();
        MemoryStream stream = new MemoryStream();

        CtfPupil pupil = new CtfPupil
        {
            UPN = string.Empty,
            Surname = "Smith",
            Forename = string.Empty,
            DOB = "2010-01-01",
            Sex = null,
            Assessments = new List<CtfKeyStageAssessment>()
        };

        CtfFile file = new CtfFile(new CtfHeader(), new List<CtfPupil> { pupil });

        // Act
        await formatter.FormatAsync(file, stream);
        string xml = ReadXml(stream);

        // Assert
        Assert.DoesNotContain("<UPN>", xml);
        Assert.Contains("<Surname>Smith</Surname>", xml);
        Assert.DoesNotContain("<Forename>", xml);
        Assert.DoesNotContain("<Sex>", xml);
    }
}
