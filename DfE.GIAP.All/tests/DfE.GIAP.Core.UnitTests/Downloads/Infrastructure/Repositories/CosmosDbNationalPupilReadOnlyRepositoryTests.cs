using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Infrastructure.Repositories;

public sealed class CosmosDbNationalPupilReadOnlyRepositoryTests
{
    private readonly InMemoryLoggerService _mockLogger;

    public CosmosDbNationalPupilReadOnlyRepositoryTests()
    {
        _mockLogger = LoggerServiceTestDoubles.MockLoggerService();
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullLogger()
    {
        // Arrange
        Mock<IMapper<NationalPupilDto, NationalPupil>> mockMapper = MapperTestDoubles.Default<NationalPupilDto, NationalPupil>();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosDbNationalPupilReadOnlyRepository(
            logger: null!,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullQueryHandler()
    {
        // Arrange
        Mock<IMapper<NationalPupilDto, NationalPupil>> mockMapper = MapperTestDoubles.Default<NationalPupilDto, NationalPupil>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosDbNationalPupilReadOnlyRepository(
            logger: _mockLogger,
            cosmosDbQueryHandler: null!,
            dtoToEntityMapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullMapper()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosDbNationalPupilReadOnlyRepository(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: null!));
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_ReturnsEmptyPupilList_When_CosmosException()
    {
        // Arrange
        string[] pupilIds = ["9999999999"];
        Func<IEnumerable<NationalPupilDto>> cosmosExceptionGenerator =
            CosmosExceptionTestDoubles.ThrowsCosmosExceptionDelegate<IEnumerable<NationalPupilDto>>();

        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany(cosmosExceptionGenerator);
        Mock<IMapper<NationalPupilDto, NationalPupil>> mockMapper = MapperTestDoubles.Default<NationalPupilDto, NationalPupil>();

        CosmosDbNationalPupilReadOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<NationalPupil> result = await sut.GetPupilsByIdsAsync(pupilIds);

        // Assert
        Assert.Empty(result);
        Assert.Equal("CosmosException in GetPupilsByIdsAsync", _mockLogger.Logs.Single());
        mockMapper.Verify(m => m.Map(It.IsAny<NationalPupilDto>()), Times.Never);
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_ReturnsEmptyPupilList_When_PupilIdsIsEmptyList()
    {
        // Arrange
        IEnumerable<string> emptyIds = Enumerable.Empty<string>();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();
        Mock<IMapper<NationalPupilDto, NationalPupil>> mockMapper = MapperTestDoubles.Default<NationalPupilDto, NationalPupil>();

        CosmosDbNationalPupilReadOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<NationalPupil> result = await sut.GetPupilsByIdsAsync(emptyIds);

        // Assert
        Assert.Empty(result);
        mockMapper.Verify(m => m.Map(It.IsAny<NationalPupilDto>()), Times.Never);
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_ReturnsEmptyPupilList_When_NoPupilsFoundById()
    {
        // Arrange
        string[] pupilIds = ["9999999999"];
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany<NationalPupilDto>(() => []);

        Mock<IMapper<NationalPupilDto, NationalPupil>> mockMapper = MapperTestDoubles.Default<NationalPupilDto, NationalPupil>();

        CosmosDbNationalPupilReadOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<NationalPupil> result = await sut.GetPupilsByIdsAsync(pupilIds);

        // Assert
        Assert.Empty(result);
        mockMapper.Verify(m => m.Map(It.IsAny<NationalPupilDto>()), Times.Never);
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_MapperIsCalled_When_PupilsFound()
    {
        // Arrange
        List<NationalPupilDto> furtherEducationPupilStub = NationalPupilDtoTestDoubles.Generate();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany(() => furtherEducationPupilStub);
        Mock<IMapper<NationalPupilDto, NationalPupil>> mockMapper = MapperTestDoubles.MockFor<NationalPupilDto, NationalPupil>(It.IsAny<NationalPupil>());

        CosmosDbNationalPupilReadOnlyRepository sut = new(
                    logger: _mockLogger,
                    cosmosDbQueryHandler: mockQueryHandler.Object,
                    dtoToEntityMapper: mockMapper.Object);

        string[] pupilIds = ["9999999999"];

        // Act
        // Force enumeration to trigger mapper calls
        IEnumerable<NationalPupil> response = (await sut.GetPupilsByIdsAsync(pupilIds)).ToList();

        // Assert
        Assert.NotNull(response);
        mockMapper.Verify(
           (mapper) => mapper.Map(It.IsAny<NationalPupilDto>()),
           Times.Exactly(furtherEducationPupilStub.Count));
    }
}
