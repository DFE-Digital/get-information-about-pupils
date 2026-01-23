using System.Linq.Expressions;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PresentationService.PresentationHandlers;

public sealed class OrderMyPupilsModelPresentationHandlerTests
{
    [Fact]
    public async Task Handle_Input_Is_Null_Returns_Failure()
    {
        // Arrange
        OrderMyPupilsModelPresentationHandler sut = new();

        // Act
        HandlerResult<MyPupilsPresentationPupilModels> response = await sut.HandleAsync(null!, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HandlerResultStatus.Failed, response.Status);
    }

    [Fact]
    public async Task Handle_SortBy_Empty_Returns_Unsorted_Pupils()
    {
        // Arrange
        MyPupilsPresentationQueryModel presentationQueryModel = MyPupilsPresentationQueryModel.CreateDefault();

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 10);

        OrderMyPupilsModelPresentationHandler sut = new();

        MyPupilsPresentationHandlerRequest request = new(
            pupils,
            presentationQueryModel,
            It.IsAny<MyPupilsPupilSelectionState>());

        // Act
        HandlerResult<MyPupilsPresentationPupilModels> response = await sut.HandleAsync(request, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.Equal(pupils, response.Result);
        // TODO expand to other properties
    }

    [Fact]
    public async Task Handle_SortBy_UnknownKey_Throws_ArgumentException()
    {
        // Arrange
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(sortKey: "unknown-sortByKey");

        OrderMyPupilsModelPresentationHandler sut = new();

        MyPupilsPresentationHandlerRequest request = new(
            MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 10),
            query,
            It.IsAny<MyPupilsPupilSelectionState>());

        // Act Assert
        HandlerResult<MyPupilsPresentationPupilModels> response =
            await sut.HandleAsync(request, It.IsAny<CancellationToken>()).AsTask();

        Assert.Equal(HandlerResultStatus.Failed, response.Status);
        Assert.Null(response.Result);
        ArgumentException ex = Assert.IsType<ArgumentException>(response.Exception);
        Assert.Contains("Unable to find sortable expression", ex.Message);
    }


    [Theory]
    [InlineData("forename", "asc")]
    [InlineData("forename", "desc")]
    [InlineData("FORENAME", "asc")]
    [InlineData("foRENamE", "desc")]
    public async Task Handle_SortBy_Forename_Returns_SortedPupils_By_Forename(string sortKey, string sortDirection)
    {
        // Arrange
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(sortKey, sortDirection);

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 20);

        OrderMyPupilsModelPresentationHandler sut = new();

        MyPupilsPresentationHandlerRequest request = new(
            pupils,
            query,
            It.IsAny<MyPupilsPupilSelectionState>());

        // Act
        HandlerResult<MyPupilsPresentationPupilModels> response = await sut.HandleAsync(request, It.IsAny<CancellationToken>());

        // Assert
        IOrderedEnumerable<MyPupilsPresentationPupilModel> expected =
            sortDirection == "asc" ?
                pupils.Values.OrderBy(t => t.Forename) :
                pupils.Values.OrderByDescending(t => t.Forename);

        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.Equal(expected, response.Result.Values);
    }

    [Theory]
    [InlineData("surname", "asc")]
    [InlineData("surname", "desc")]
    [InlineData("SURNAME", "asc")]
    [InlineData("suRnAme", "desc")]
    public async Task Handle_SortBy_Surname_Returns_SortedPupils_By_Surname(string sortKey, string sortDirection)
    {
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(sortKey, sortDirection);

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 20);

        OrderMyPupilsModelPresentationHandler sut = new();

        MyPupilsPresentationHandlerRequest request = new(
            pupils,
            query,
            It.IsAny<MyPupilsPupilSelectionState>());

        // Act
        HandlerResult<MyPupilsPresentationPupilModels> response = await sut.HandleAsync(request, It.IsAny<CancellationToken>());

        // Assert
        IOrderedEnumerable<MyPupilsPresentationPupilModel> expected =
            sortDirection == "asc" ?
                pupils.Values.OrderBy(t => t.Surname) :
                pupils.Values.OrderByDescending(t => t.Surname);

        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.Equal(expected, response.Result.Values);
    }


    [Theory]
    [InlineData("dob", "asc")]
    [InlineData("dob", "desc")]
    [InlineData("DOB", "asc")]
    [InlineData("dOB", "desc")]
    public async Task Handle_SortBy_DateOfBirth_Returns_SortedPupils_By_DateOfBirth(string sortKey, string sortDirection)
    {
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(sortKey, sortDirection);

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 20);

        OrderMyPupilsModelPresentationHandler sut = new();

        MyPupilsPresentationHandlerRequest request = new(
            pupils,
            query,
            It.IsAny<MyPupilsPupilSelectionState>());

        // Act
        HandlerResult<MyPupilsPresentationPupilModels> response = await sut.HandleAsync(request, It.IsAny<CancellationToken>());

        // Assert
        IOrderedEnumerable<MyPupilsPresentationPupilModel> expected =
            sortDirection == "asc" ?
                pupils.Values.OrderBy(t => t.ParseDateOfBirth()) :
                pupils.Values.OrderByDescending(t => t.ParseDateOfBirth());

        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.Equal(expected, response.Result.Values);
    }


    [Theory]
    [InlineData("sex", "asc")]
    [InlineData("sex", "desc")]
    [InlineData("SEX", "asc")]
    [InlineData("seX", "desc")]
    public async Task Handle_SortBy_Sex_Returns_SortedPupils_By_Sex(string sortKey, string sortDirection)
    {
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(sortKey, sortDirection);

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 20);

        OrderMyPupilsModelPresentationHandler sut = new();

        MyPupilsPresentationHandlerRequest request = new(
            pupils,
            query,
            It.IsAny<MyPupilsPupilSelectionState>());

        // Act
        HandlerResult<MyPupilsPresentationPupilModels> response = await sut.HandleAsync(request, It.IsAny<CancellationToken>());

        // Assert
        IOrderedEnumerable<MyPupilsPresentationPupilModel> expected =
            sortDirection == "asc" ?
                pupils.Values.OrderBy(t => t.Sex) :
                pupils.Values.OrderByDescending(t => t.Sex);

        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.Equal(expected, response.Result.Values);
    }
}
