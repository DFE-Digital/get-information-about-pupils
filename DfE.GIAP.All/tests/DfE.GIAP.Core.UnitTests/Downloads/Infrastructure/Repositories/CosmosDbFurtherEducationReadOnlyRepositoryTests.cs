﻿using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Infrastructure.Repositories;

public sealed class CosmosDbFurtherEducationReadOnlyRepositoryTests
{
    private readonly InMemoryLoggerService _mockLogger;

    public CosmosDbFurtherEducationReadOnlyRepositoryTests()
    {
        _mockLogger = LoggerServiceTestDoubles.MockLoggerService();
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullLogger()
    {
        // Arrange
        Mock<IMapper<FurtherEducationPupilDto, FurtherEducationPupil>> mockMapper = MapperTestDoubles.Default<FurtherEducationPupilDto, FurtherEducationPupil>();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosDbFurtherEducationReadOnlyRepository(
            logger: null!,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullQueryHandler()
    {
        // Arrange
        Mock<IMapper<FurtherEducationPupilDto, FurtherEducationPupil>> mockMapper = MapperTestDoubles.Default<FurtherEducationPupilDto, FurtherEducationPupil>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosDbFurtherEducationReadOnlyRepository(
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
        Assert.Throws<ArgumentNullException>(() => new CosmosDbFurtherEducationReadOnlyRepository(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: null!));
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_ReturnsEmptyPupilList_When_CosmosException()
    {
        // Arrange
        string[] pupilIds = ["9999999999"];
        Func<IEnumerable<FurtherEducationPupilDto>> cosmosExceptionGenerator =
            CosmosExceptionTestDoubles.ThrowsCosmosExceptionDelegate<IEnumerable<FurtherEducationPupilDto>>();

        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany(cosmosExceptionGenerator);
        Mock<IMapper<FurtherEducationPupilDto, FurtherEducationPupil>> mockMapper = MapperTestDoubles.Default<FurtherEducationPupilDto, FurtherEducationPupil>();

        CosmosDbFurtherEducationReadOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<FurtherEducationPupil> result = await sut.GetPupilsByIdsAsync(pupilIds);

        // Assert
        Assert.Empty(result);
        Assert.Equal("CosmosException in GetPupilsByIdsAsync", _mockLogger.Logs.Single());
        mockMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupilDto>()), Times.Never);
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_ReturnsEmptyPupilList_When_PupilIdsIsEmptyList()
    {
        // Arrange
        IEnumerable<string> emptyIds = Enumerable.Empty<string>();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();
        Mock<IMapper<FurtherEducationPupilDto, FurtherEducationPupil>> mockMapper = MapperTestDoubles.Default<FurtherEducationPupilDto, FurtherEducationPupil>();

        CosmosDbFurtherEducationReadOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<FurtherEducationPupil> result = await sut.GetPupilsByIdsAsync(emptyIds);

        // Assert
        Assert.Empty(result);
        mockMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupilDto>()), Times.Never);
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_ReturnsEmptyPupilList_When_NoPupilsFoundById()
    {
        // Arrange
        string[] pupilIds = ["9999999999"];
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany<FurtherEducationPupilDto>(() => []);

        Mock<IMapper<FurtherEducationPupilDto, FurtherEducationPupil>> mockMapper = MapperTestDoubles.Default<FurtherEducationPupilDto, FurtherEducationPupil>();

        CosmosDbFurtherEducationReadOnlyRepository sut = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockQueryHandler.Object,
            dtoToEntityMapper: mockMapper.Object);

        // Act
        IEnumerable<FurtherEducationPupil> result = await sut.GetPupilsByIdsAsync(pupilIds);

        // Assert
        Assert.Empty(result);
        mockMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupilDto>()), Times.Never);
    }

    [Fact]
    public async Task GetPupilsByIdsAsync_MapperIsCalled_When_PupilsFound()
    {
        // Arrange
        List<FurtherEducationPupilDto> furtherEducationPupilStub = FurtherEducationPupilDtoTestDoubles.Generate();
        Mock<ICosmosDbQueryHandler> mockQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany(() => furtherEducationPupilStub);
        Mock<IMapper<FurtherEducationPupilDto, FurtherEducationPupil>> mockMapper = MapperTestDoubles.MockFor<FurtherEducationPupilDto, FurtherEducationPupil>(It.IsAny<FurtherEducationPupil>());

        CosmosDbFurtherEducationReadOnlyRepository sut = new(
                    logger: _mockLogger,
                    cosmosDbQueryHandler: mockQueryHandler.Object,
                    dtoToEntityMapper: mockMapper.Object);

        string[] pupilIds = ["9999999999"];

        // Act
        // Force enumeration to trigger mapper calls
        IEnumerable<FurtherEducationPupil> response = (await sut.GetPupilsByIdsAsync(pupilIds)).ToList();

        // Assert
        Assert.NotNull(response);
        mockMapper.Verify(
           (mapper) => mapper.Map(It.IsAny<FurtherEducationPupilDto>()),
           Times.Exactly(furtherEducationPupilStub.Count));
    }
}
