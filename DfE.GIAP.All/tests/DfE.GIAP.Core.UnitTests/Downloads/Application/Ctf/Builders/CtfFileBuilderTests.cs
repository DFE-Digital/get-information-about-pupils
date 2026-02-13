using DfE.GIAP.Core.Downloads.Application.Ctf.Builders;
using DfE.GIAP.Core.Downloads.Application.Ctf.Context;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Ctf.Builders;

public class CtfFileBuilderTests
{
    [Fact]
    public void Constructor_WithNullHeaderBuilder_Throws()
    {
        ICtfHeaderBuilder headerBuilder = null!;
        ICtfPupilBuilder pupilBuilder = new Mock<ICtfPupilBuilder>().Object;

        Action act = new Action(() => new CtfFileBuilder(headerBuilder, pupilBuilder));

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Constructor_WithNullPupilBuilder_Throws()
    {
        ICtfHeaderBuilder headerBuilder = new Mock<ICtfHeaderBuilder>().Object;
        ICtfPupilBuilder pupilBuilder = null!;

        Action act = new Action(() => new CtfFileBuilder(headerBuilder, pupilBuilder));

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public async Task AggregateFileAsync_BuildsHeaderAndPupils_AndReturnsCtfFile()
    {
        Mock<ICtfHeaderBuilder> headerBuilderMock = new();
        Mock<ICtfPupilBuilder> pupilBuilderMock = new();

        ICtfHeaderContext headerContext = new Mock<ICtfHeaderContext>().Object;
        IEnumerable<string> selectedPupils = new List<string> { "A", "B" };

        CtfHeader expectedHeader = new();
        List<CtfPupil> expectedPupils =
        [
            new CtfPupil(),
            new CtfPupil()
        ];

        headerBuilderMock
            .Setup(h => h.Build(headerContext))
            .Returns(expectedHeader);

        pupilBuilderMock
            .Setup(p => p.BuildAsync(selectedPupils))
            .ReturnsAsync(expectedPupils);

        CtfFileBuilder builder = new(
            headerBuilderMock.Object,
            pupilBuilderMock.Object);

        CtfFile result = await builder.AggregateFileAsync(headerContext, selectedPupils);

        headerBuilderMock.Verify(h => h.Build(headerContext), Times.Once());
        pupilBuilderMock.Verify(p => p.BuildAsync(selectedPupils), Times.Once());

        Assert.Equal(expectedHeader, result.Header);
        Assert.Equal(expectedPupils, result.Pupils);
    }
}
