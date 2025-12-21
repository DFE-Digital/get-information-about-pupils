using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.Features.MyPupils.Application;
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
/*
    [Fact]
    public void Handle_SortBy_UnknownKey_Throws_ArgumentException()
    {
        // Arrange
        MyPupilsPresentationQueryModel state = MyPupilsPresentationQueyTestDoubles.Create(sortKey: "unknown-sortByKey");


        OrderMyPupilsModelPresentationHandler sut = new();

        // Act Assert
        Action act = () => sut.Handle(It.IsAny<MyPupilsModels>(), state);
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("forename", SortDirection.Ascending)]
    [InlineData("forename", SortDirection.Descending)]
    [InlineData("FORENAME", SortDirection.Ascending)]
    [InlineData("foRENamE", SortDirection.Descending)]
    public void Handle_SortBy_Forename_Returns_SortedPupils_By_Forename(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationQueryModel state = MyPupilsPresentationQueyTestDoubles.Create(sortKey, sortDirection);

        MyPupilsModels pupils = MyPupilModelTestDoubles.Generate(count: 20);


        OrderMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsModels response = sut.Handle(pupils, state);

        // Assert
        IEnumerable<MyPupilsModel> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.Values.OrderBy(t => t.Forename) :
                pupils.Values.OrderByDescending(t => t.Forename);

        Assert.Equal(expected, response.Values);
    }

    [Theory]
    [InlineData("surname", SortDirection.Ascending)]
    [InlineData("surname", SortDirection.Descending)]
    [InlineData("SURNAME", SortDirection.Ascending)]
    [InlineData("suRnAme", SortDirection.Descending)]
    public void Handle_SortBy_Surname_Returns_SortedPupils_By_Surname(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationQueryModel state = MyPupilsPresentationQueyTestDoubles.Create(sortKey, sortDirection);

        MyPupilsModels pupils = MyPupilModelTestDoubles.Generate(count: 20);


        OrderMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsModels response = sut.Handle(pupils, state);

        IEnumerable<MyPupilsModel> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.Values.OrderBy(t => t.Surname) :
                pupils.Values.OrderByDescending(t => t.Surname);

        // Assert
        Assert.Equal(expected, response.Values);
    }

    [Theory]
    [InlineData("dob", SortDirection.Ascending)]
    [InlineData("dob", SortDirection.Descending)]
    [InlineData("DOB", SortDirection.Ascending)]
    [InlineData("dOB", SortDirection.Descending)]
    public void Handle_SortBy_DateOfBirth_Returns_SortedPupils_By_DateOfBirth(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationQueryModel state = MyPupilsPresentationQueyTestDoubles.Create(sortKey, sortDirection);

        MyPupilsModels pupils = MyPupilModelTestDoubles.Generate(count: 20);


        OrderMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsModels response = sut.Handle(pupils, state);

        // Assert
        IEnumerable<MyPupilsModel> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.Values.OrderBy(t => t.ParseDateOfBirth()) :
                pupils.Values.OrderByDescending(t => t.ParseDateOfBirth());

        Assert.Equal(expected, response.Values);
    }

    [Theory]
    [InlineData("sex", SortDirection.Ascending)]
    [InlineData("sex", SortDirection.Descending)]
    [InlineData("SEX", SortDirection.Ascending)]
    [InlineData("seX", SortDirection.Descending)]
    public void Handle_SortBy_Sex_Returns_SortedPupils_By_Sex(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationQueryModel presentationState = MyPupilsPresentationQueyTestDoubles.Create(sortKey, sortDirection);

        MyPupilsModels pupils = MyPupilModelTestDoubles.Generate(count: 20);


        OrderMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsModels response = sut.Handle(pupils, presentationState);

        // Assert
        IEnumerable<MyPupilsModel> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.Values.OrderBy(t => t.Sex) :
                pupils.Values.OrderByDescending(t => t.Sex);

        Assert.Equal(expected, response.Values);
    }*/
}
