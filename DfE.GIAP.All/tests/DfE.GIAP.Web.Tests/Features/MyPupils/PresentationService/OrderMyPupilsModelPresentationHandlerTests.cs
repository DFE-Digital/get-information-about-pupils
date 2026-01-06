using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PresentationService;

public sealed class OrderMyPupilsModelPresentationHandlerTests
{
    [Fact]
    public void Handle_SortBy_Empty_Returns_Unsorted_Pupils()
    {
        // Arrange
        MyPupilsPresentationQueryModel presentationQueryModel = MyPupilsPresentationQueryModel.CreateDefault();

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 10);

        OrderMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response =
            sut.Handle(
                pupils,
                presentationQueryModel,
                MyPupilsPupilSelectionState.CreateDefault());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(pupils, response);
        // TODO expand to other properties
    }

    [Fact]
    public void Handle_SortBy_UnknownKey_Throws_ArgumentException()
    {
        // Arrange
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(sortKey: "unknown-sortByKey");


        OrderMyPupilsModelPresentationHandler sut = new();

        // Act Assert
        Action act = () => sut.Handle(It.IsAny<MyPupilsPresentationPupilModels>(), query, It.IsAny<MyPupilsPupilSelectionState>());
        Assert.Throws<ArgumentException>(act);
    }

    
    [Theory]
    [InlineData("forename", "asc")]
    [InlineData("forename", "desc")]
    [InlineData("FORENAME", "asc")]
    [InlineData("foRENamE", "desc")]
    public void Handle_SortBy_Forename_Returns_SortedPupils_By_Forename(string sortKey, string sortDirection)
    {
        // Arrange
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(sortKey, sortDirection);

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 20);

        OrderMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Handle(pupils, query, It.IsAny<MyPupilsPupilSelectionState>());

        // Assert
        IOrderedEnumerable<MyPupilsPresentationPupilModel> expected =
            sortDirection == "asc" ?
                pupils.Values.OrderBy(t => t.Forename) :
                pupils.Values.OrderByDescending(t => t.Forename);

        Assert.Equal(expected, response.Values);
    }

    [Theory]
    [InlineData("surname", "asc")]
    [InlineData("surname", "desc")]
    [InlineData("SURNAME", "asc")]
    [InlineData("suRnAme", "desc")]
    public void Handle_SortBy_Surname_Returns_SortedPupils_By_Surname(string sortKey, string sortDirection)
    {
        // Arrange
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(sortKey, sortDirection);

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 20);

        OrderMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Handle(pupils, query, It.IsAny<MyPupilsPupilSelectionState>());

        IOrderedEnumerable<MyPupilsPresentationPupilModel> expected =
            sortDirection == "asc" ?
                pupils.Values.OrderBy(t => t.Surname) :
                pupils.Values.OrderByDescending(t => t.Surname);

        // Assert
        Assert.Equal(expected, response.Values);
    }

    
    [Theory]
    [InlineData("dob", "asc")]
    [InlineData("dob", "desc")]
    [InlineData("DOB", "asc")]
    [InlineData("dOB", "desc")]
    public void Handle_SortBy_DateOfBirth_Returns_SortedPupils_By_DateOfBirth(string sortKey, string sortDirection)
    {
        // Arrange
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(sortKey, sortDirection);

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 20);

        OrderMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Handle(pupils, query, It.IsAny<MyPupilsPupilSelectionState>());

        // Assert
        IEnumerable<MyPupilsPresentationPupilModel> expected =
            sortDirection == "asc" ?
                pupils.Values.OrderBy(t => t.ParseDateOfBirth()) :
                pupils.Values.OrderByDescending(t => t.ParseDateOfBirth());

        Assert.Equal(expected, response.Values);
    }

    
    [Theory]
    [InlineData("sex", "asc")]
    [InlineData("sex", "desc")]
    [InlineData("SEX", "asc")]
    [InlineData("seX", "desc")]
    public void Handle_SortBy_Sex_Returns_SortedPupils_By_Sex(string sortKey, string sortDirection)
    {
        // Arrange
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(sortKey, sortDirection);

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 20);

        OrderMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Handle(pupils, query, It.IsAny<MyPupilsPupilSelectionState>());

        // Assert
        IEnumerable<MyPupilsPresentationPupilModel> expected =
            sortDirection == "asc" ?
                pupils.Values.OrderBy(t => t.Sex) :
                pupils.Values.OrderByDescending(t => t.Sex);

        Assert.Equal(expected, response.Values);
    }
}
