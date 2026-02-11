using DfE.GIAP.Core.Downloads.Application.Ctf.Builders;
using DfE.GIAP.Core.Downloads.Application.Ctf.Context;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;
using DfE.GIAP.Core.Downloads.Application.Ctf.Versioning;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Ctf.Builders;

public class CtfHeaderBuilderTests
{
    [Fact]
    public void Constructor_WithNullVersionProvider_Throws()
    {
        ICtfVersionProvider provider = null!;

        Action act = new Action(() => new CtfHeaderBuilder(provider));

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Build_CallsVersionProvider()
    {
        Mock<ICtfVersionProvider> versionProviderMock = new();
        versionProviderMock.Setup(v => v.GetVersion()).Returns("1.0");

        CtfHeaderBuilder builder = new(versionProviderMock.Object);

        Mock<ICtfHeaderContext> contextMock = new();
        contextMock.Setup(c => c.IsEstablishment).Returns(true);

        CtfHeader header = builder.Build(contextMock.Object);

        versionProviderMock.Verify(v => v.GetVersion(), Times.Once());
    }

    [Fact]
    public void Build_WhenEstablishment_UsesEstablishmentDescriptor()
    {
        Mock<ICtfVersionProvider> versionProviderMock = new();
        versionProviderMock.Setup(v => v.GetVersion()).Returns(It.IsAny<string>);

        CtfHeaderBuilder builder = new(versionProviderMock.Object);

        Mock<ICtfHeaderContext> contextMock = new();
        contextMock.Setup(c => c.IsEstablishment).Returns(true);

        CtfHeader header = builder.Build(contextMock.Object);

        Assert.Equal(CtfHeaderBuilder.DescriptorEstablishment, header.DataDescriptor);
    }

    [Fact]
    public void Build_WhenNotEstablishment_UsesNonEstablishmentDescriptor()
    {
        Mock<ICtfVersionProvider> versionProviderMock = new Mock<ICtfVersionProvider>();
        versionProviderMock.Setup(v => v.GetVersion()).Returns(It.IsAny<string>);

        CtfHeaderBuilder builder = new(versionProviderMock.Object);

        Mock<ICtfHeaderContext> contextMock = new();
        contextMock.Setup(c => c.IsEstablishment).Returns(false);

        CtfHeader header = builder.Build(contextMock.Object);

        Assert.Equal(CtfHeaderBuilder.DescriptorNonEstablishment, header.DataDescriptor);
    }

    [Fact]
    public void Build_PopulatesStaticFieldsCorrectly()
    {
        Mock<ICtfVersionProvider> versionProviderMock = new Mock<ICtfVersionProvider>();
        versionProviderMock.Setup(v => v.GetVersion()).Returns("1.0");

        CtfHeaderBuilder builder = new CtfHeaderBuilder(versionProviderMock.Object);

        Mock<ICtfHeaderContext> contextMock = new Mock<ICtfHeaderContext>();
        contextMock.Setup(c => c.IsEstablishment).Returns(true);
        contextMock.Setup(c => c.LocalAuthorityNumber).Returns("123");
        contextMock.Setup(c => c.EstablishedNumber).Returns("123");

        CtfHeader header = builder.Build(contextMock.Object);

        Assert.Equal("Common Transfer File", header.DocumentName);
        Assert.Equal("1.0", header.CtfVersion);
        Assert.Equal("partial", header.DocumentQualifier);
        Assert.Equal("GIAP", header.SupplierId);

        Assert.Equal("XXX", header.SourceSchool.LEA);
        Assert.Equal("XXXX", header.SourceSchool.Estab);
        Assert.Equal("Get Information About Pupils", header.SourceSchool.SchoolName);

        Assert.Equal("123", header.DestSchool.LEA);
        Assert.Equal("123", header.DestSchool.Estab);
    }

    [Fact]
    public void Build_CalculatesAcademicYear_AfterSeptember()
    {
        Mock<ICtfVersionProvider> versionProviderMock = new Mock<ICtfVersionProvider>();
        versionProviderMock.Setup(v => v.GetVersion()).Returns(It.IsAny<string>);

        CtfHeaderBuilder builder = new(versionProviderMock.Object);

        Mock<ICtfHeaderContext> contextMock = new();
        contextMock.Setup(c => c.IsEstablishment).Returns(true);

        CtfHeader header = builder.Build(contextMock.Object);

        DateTime now = header.DateTime;

        string expectedYear = now.Month >= 9
            ? now.Year.ToString()
            : (now.Year - 1).ToString();

        Assert.Equal(expectedYear, header.SourceSchool.AcademicYear);
    }
}
