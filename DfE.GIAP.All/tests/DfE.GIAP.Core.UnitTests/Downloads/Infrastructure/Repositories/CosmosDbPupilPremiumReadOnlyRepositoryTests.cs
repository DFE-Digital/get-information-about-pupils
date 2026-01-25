using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Infrastructure.Repositories;

public sealed class CosmosDbPupilPremiumReadOnlyRepositoryTests
{
    private readonly InMemoryLoggerService _mockLogger;

    public CosmosDbPupilPremiumReadOnlyRepositoryTests()
    {
        _mockLogger = LoggerServiceTestDoubles.MockLoggerService();
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullLogger()
    {
        // Arrange
        Mock<IMapper<PupilPremiumPupilDto, PupilPremiumPupil>> mockMapper = MapperTestDoubles.Default<PupilPremiumPupilDto, PupilPremiumPupil>();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosDbPupilPremiumDownloadDatasetReadOnlyRepository(
            logger: null!,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullQueryHandler()
    {
        // Arrange
        Mock<IMapper<PupilPremiumPupilDto, PupilPremiumPupil>> mockMapper = MapperTestDoubles.Default<PupilPremiumPupilDto, PupilPremiumPupil>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosDbPupilPremiumDownloadDatasetReadOnlyRepository(
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
        Assert.Throws<ArgumentNullException>(() => new CosmosDbPupilPremiumDownloadDatasetReadOnlyRepository(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: null!));
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_ReturnsEmptyPupilList_When_CosmosException()
    {
        // Arrange
        string[] pupilIds = ["9999999999"];
        Func<IEnumerable<PupilPremiumPupilDto>> cosmosExceptionGenerator =
            CosmosExceptionTestDoubles.ThrowsCosmosExceptionDelegate<IEnumerable<PupilPremiumPupilDto>>();

        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany(cosmosExceptionGenerator);
        Mock<IMapper<PupilPremiumPupilDto, PupilPremiumPupil>> mockMapper = MapperTestDoubles.Default<PupilPremiumPupilDto, PupilPremiumPupil>();

        CosmosDbPupilPremiumDownloadDatasetReadOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<PupilPremiumPupil> result = await sut.GetPupilsByIdsAsync(pupilIds);

        // Assert
        Assert.Empty(result);
        Assert.Equal("CosmosException in GetPupilsByIdsAsync", _mockLogger.Logs.Single());
        mockMapper.Verify(m => m.Map(It.IsAny<PupilPremiumPupilDto>()), Times.Never);
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_ReturnsEmptyPupilList_When_PupilIdsIsEmptyList()
    {
        // Arrange
        IEnumerable<string> emptyIds = Enumerable.Empty<string>();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();
        Mock<IMapper<PupilPremiumPupilDto, PupilPremiumPupil>> mockMapper = MapperTestDoubles.Default<PupilPremiumPupilDto, PupilPremiumPupil>();

        CosmosDbPupilPremiumDownloadDatasetReadOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<PupilPremiumPupil> result = await sut.GetPupilsByIdsAsync(emptyIds);

        // Assert
        Assert.Empty(result);
        mockMapper.Verify(m => m.Map(It.IsAny<PupilPremiumPupilDto>()), Times.Never);
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_ReturnsEmptyPupilList_When_NoPupilsFoundById()
    {
        // Arrange
        string[] pupilIds = ["9999999999"];
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany<PupilPremiumPupilDto>(() => []);

        Mock<IMapper<PupilPremiumPupilDto, PupilPremiumPupil>> mockMapper = MapperTestDoubles.Default<PupilPremiumPupilDto, PupilPremiumPupil>();

        CosmosDbPupilPremiumDownloadDatasetReadOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<PupilPremiumPupil> result = await sut.GetPupilsByIdsAsync(pupilIds);

        // Assert
        Assert.Empty(result);
        mockMapper.Verify(m => m.Map(It.IsAny<PupilPremiumPupilDto>()), Times.Never);
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_MapperIsCalled_When_PupilsFound()
    {
        // Arrange
        List<PupilPremiumPupilDto> pupilPremiumPupilStub = PupilPremiumPupilDtoTestDoubles.Generate();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany(() => pupilPremiumPupilStub);
        Mock<IMapper<PupilPremiumPupilDto, PupilPremiumPupil>> mockMapper = MapperTestDoubles.MockFor<PupilPremiumPupilDto, PupilPremiumPupil>(It.IsAny<PupilPremiumPupil>());

        CosmosDbPupilPremiumDownloadDatasetReadOnlyRepository sut = new(
                    logger: _mockLogger,
                    cosmosDbQueryHandler: mockQueryHandler.Object,
                    dtoToEntityMapper: mockMapper.Object);

        string[] pupilIds = ["9999999999"];

        // Act
        // Force enumeration to trigger mapper calls
        IEnumerable<PupilPremiumPupil> response = (await sut.GetPupilsByIdsAsync(pupilIds)).ToList();

        // Assert
        Assert.NotNull(response);
        mockMapper.Verify(
           (mapper) => mapper.Map(It.IsAny<PupilPremiumPupilDto>()),
           Times.Exactly(pupilPremiumPupilStub.Count));
    }

}
