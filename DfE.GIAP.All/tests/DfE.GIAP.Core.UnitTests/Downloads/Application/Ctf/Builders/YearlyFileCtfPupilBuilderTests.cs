using DfE.GIAP.Core.Downloads.Application.Ctf.Builders;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Ctf.Builders;

public class YearlyFileCtfPupilBuilderTests
{
    private static NationalPupil CreateBasePupil()
    {
        return new NationalPupil
        {
            Upn = "A123",
            Surname = "Smith",
            Forename = "John",
            DOB = new DateTime(2010, 1, 1),
            Sex = "M"
        };
    }

    [Fact]
    public async Task BuildAsync_MapsBaseFieldsCorrectly()
    {
        // Arrange
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IDataSchemaProvider> schema = new();
        Mock<IPropertyValueAccessor> accessor = new();

        NationalPupil pupil = CreateBasePupil();
        List<NationalPupil> pupils = [pupil];

        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(pupils);

        schema.Setup(s => s.GetSchemaByYearAsync(It.IsAny<int>()))
            .ReturnsAsync((DataSchemaDefinition)null!);

        YearlyFileCtfPupilBuilder builder = new(repo.Object, schema.Object, accessor.Object);

        // Act
        IEnumerable<CtfPupil> result = await builder.BuildAsync(new List<string> { "A123" });

        // Assert
        CtfPupil output = Assert.Single(result);
        Assert.Equal(pupil.Upn, output.UPN);
        Assert.Equal(pupil.Surname, output.Surname);
        Assert.Equal(pupil.Forename, output.Forename);
        Assert.Equal(pupil.DOB.ToString("yyyy-MM-dd"), output.DOB);
        Assert.Equal(pupil.Sex, output.Sex);
    }

    [Fact]
    public async Task BuildAsync_UsesLatestPhonicsEntry()
    {
        // Arrange
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IDataSchemaProvider> schema = new();
        Mock<IPropertyValueAccessor> accessor = new();

        PhonicsEntry older = new() { AcademicYear = "2018/2019" };
        PhonicsEntry newer = new() { AcademicYear = "2019/2020" };

        NationalPupil pupil = CreateBasePupil();
        pupil.Phonics = [older, newer];

        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<NationalPupil> { pupil });

        DataSchemaDefinition schemaDef = new()
        {
            Year = "2020",
            Rules =
            [
                new DataSchemaDefinitionRule
                {
                    Stage = "KS1",
                    ResultField = "Score",
                    Subject = "Reading",
                    Method = "T",
                    Component = "C1",
                    ResultQualifier = "Q"
                }
            ]
        };

        schema.Setup(s => s.GetSchemaByYearAsync(2020))
            .ReturnsAsync(schemaDef);

        accessor.Setup(a => a.GetValue(newer, "Score"))
            .Returns("15");

        YearlyFileCtfPupilBuilder builder = new(repo.Object, schema.Object, accessor.Object);

        // Act
        IEnumerable<CtfPupil> result = await builder.BuildAsync(new List<string> { "A123" });

        // Assert
        CtfPupil output = Assert.Single(result);
        Assert.Single(output.Assessments);
        Assert.Equal("15", output.Assessments[0].Result);
    }

    [Fact]
    public async Task BuildAsync_SkipsAssessment_WhenSchemaMissing()
    {
        // Arrange
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IDataSchemaProvider> schema = new();
        Mock<IPropertyValueAccessor> accessor = new();

        KeyStage1Entry ks1 = new() { ACADYR = "2020/2021" };

        NationalPupil pupil = CreateBasePupil();
        pupil.KeyStage1 = [ks1];

        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<NationalPupil> { pupil });

        schema.Setup(s => s.GetSchemaByYearAsync(2021))
            .ReturnsAsync((DataSchemaDefinition)null!);

        YearlyFileCtfPupilBuilder builder = new(repo.Object, schema.Object, accessor.Object);

        // Act
        IEnumerable<CtfPupil> result = await builder.BuildAsync(new List<string> { "A123" });

        // Assert
        CtfPupil output = Assert.Single(result);
        Assert.Empty(output.Assessments);
    }

    [Fact]
    public async Task BuildAsync_UsesValueAccessor_ForRuleFields()
    {
        // Arrange
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IDataSchemaProvider> schema = new();
        Mock<IPropertyValueAccessor> accessor = new();

        KeyStage2Entry ks2 = new() { ACADYR = "2021/2022" };

        NationalPupil pupil = CreateBasePupil();
        pupil.KeyStage2 = [ks2];

        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<NationalPupil> { pupil });

        DataSchemaDefinition schemaDef = new()
        {
            Year = "2022",
            Rules = new List<DataSchemaDefinitionRule>
            {
                new DataSchemaDefinitionRule
                {
                    Stage = "KS2",
                    ResultField = "ScaledScore",
                    Subject = "Math",
                    Method = "T",
                    Component = "C1",
                    ResultQualifier = "Q"
                }
            }
        };

        schema.Setup(s => s.GetSchemaByYearAsync(2022))
            .ReturnsAsync(schemaDef);

        accessor.Setup(a => a.GetValue(ks2, "ScaledScore"))
            .Returns("110");

        YearlyFileCtfPupilBuilder builder =
            new YearlyFileCtfPupilBuilder(repo.Object, schema.Object, accessor.Object);

        // Act
        IEnumerable<CtfPupil> result = await builder.BuildAsync(new List<string> { "A123" });

        // Assert
        CtfPupil output = Assert.Single(result);
        Assert.Single(output.Assessments);
        Assert.Equal("110", output.Assessments[0].Result);
    }

    [Fact]
    public void ToAcademicYearEnd_ParsesCorrectly()
    {
        // Act
        int result = YearlyFileCtfPupilBuilder.ToAcademicYearEnd("2020/2021");

        // Assert
        Assert.Equal(2021, result);
    }

    [Fact]
    public void ToAcademicYearEnd_ReturnsZero_ForInvalidFormat()
    {
        // Act
        int result = YearlyFileCtfPupilBuilder.ToAcademicYearEnd("invalid");

        // Assert
        Assert.Equal(0, result);
    }
}
