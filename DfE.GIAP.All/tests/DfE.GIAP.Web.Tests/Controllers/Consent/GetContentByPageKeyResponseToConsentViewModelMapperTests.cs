using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Consent;
public sealed class GetContentByPageKeyResponseToConsentViewModelMapperTests
{
    [Fact]
    public void Mapper_Throws_When_Input_Is_Null()
    {
        // Arrange
        GetContentByPageKeyResponseToConsentViewModelMapper sut = new();

        // Act
        Action act = () => sut.Map(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Mapper_Throws_When_Input_Content_Is_Null()
    {
        // Arrange
        GetContentByPageKeyUseCaseResponse response = new(Content: null);

        GetContentByPageKeyResponseToConsentViewModelMapper sut = new();
        // Act
        Action act = () => sut.Map(response);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Mapper_Maps_ConsentViewModel()
    {
        // Arrange
        Content content = ContentTestDoubles.Default();
        GetContentByPageKeyUseCaseResponse contentResponse = new(content);
        GetContentByPageKeyResponseToConsentViewModelMapper sut = new();

        // Act
        ConsentViewModel response = sut.Map(contentResponse);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(response.Response, content);
    }
}
