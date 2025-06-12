using DfE.GIAP.Core.Content.Infrastructure.Repositories;
using DfE.GIAP.Core.Content.Infrastructure.Repositories.Mapper;
using DfE.GIAP.Core.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Content.Tests.Mapper;
public sealed class ContentDTOToContentMapperTests
{

    [Fact]
    public void ContentDTOToContentMapper_Map_Returns_EmptyContent_When_NullInput()
    {
        // Arrange
        ContentDTOToContentMapper mapper = new();

        // Act
        Core.Content.Application.Model.Content response = mapper.Map(null);

        // Assert
        Core.Content.Application.Model.Content expectedResponse = Core.Content.Application.Model.Content.Empty();
        Assert.NotNull(response);
        Assert.Equal(expectedResponse, response);
    }

    [Fact]
    public void ContentDTOToContentMapper_Map_Returns_MappedContent_When_DTOPassed()
    {
        // Arrange
        ContentDTOToContentMapper mapper = new();
        ContentDTO dto = ContentDTOTestDoubles.Generate(1).Single();

        // Act
        Core.Content.Application.Model.Content response = mapper.Map(dto);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(dto.Title, response.Title);
        Assert.Equal(dto.Body, response.Body);
    }
}
