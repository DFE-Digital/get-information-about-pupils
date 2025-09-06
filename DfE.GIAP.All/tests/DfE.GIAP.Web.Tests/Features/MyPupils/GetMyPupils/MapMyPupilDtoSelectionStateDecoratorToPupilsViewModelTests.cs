using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetMyPupils;
public sealed class MapMyPupilDtoSelectionStateDecoratorToPupilsViewModelTests
{
    [Fact]
    public void Map_Throws_When_Input_Is_Null()
    {
        // Arrange
        MyPupilDtoPupilSelectionStateDecoratorToPupilsViewModelMapper sut = new();
        Action act = () => sut.Map(null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_Returns_EmptyList_When_Input_Is_EmptyList()
    {
        // Arrange
        MyPupilsDtoSelectionStateDecorator mappable = new(
            MyPupilDtos.Create(pupils: []),
            MyPupilsPupilSelectionStateTestDoubles.Default());

        MyPupilDtoPupilSelectionStateDecoratorToPupilsViewModelMapper sut = new();

        // Act
        PupilsViewModel response = sut.Map(mappable);

        Assert.NotNull(response);
        Assert.Empty(response.Pupils);
        Assert.Equal(0, response.Count);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Map_Maps_With_MappingApplied_For_PupilPremium(bool isPupilPremium)
    {
        // Arrange
        MyPupilDto createdPupil = MyPupilDtoBuilder.Create()
            .WithPupilPremium(isPupilPremium)
            .Build();

        MyPupilDtos inputPupils = MyPupilDtos.Create([createdPupil]);

        MyPupilDtoPupilSelectionStateDecoratorToPupilsViewModelMapper sut = new();

        // Act
        PupilsViewModel response = sut.Map(
            new MyPupilsDtoSelectionStateDecorator(
                inputPupils,
                MyPupilsPupilSelectionStateTestDoubles.Default()));

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Pupils);
        Assert.NotEmpty(response.Pupils);
        Assert.Equal(1, response.Count);
        PupilViewModel pupilViewModel = Assert.Single(response.Pupils);
        AssertMappedPupil(createdPupil, pupilViewModel, expectedPupilIsSelected: false);
    }

    private static void AssertMappedPupil(
        MyPupilDto input,
        PupilViewModel output,
        bool expectedPupilIsSelected)
    {
        Assert.Equal(input.UniquePupilNumber, output.UniquePupilNumber);
        Assert.Equal(input.Forename, output.Forename);
        Assert.Equal(input.Surname, output.Surname);
        Assert.Equal(input.DateOfBirth, output.DateOfBirth);
        Assert.Equal(input.Sex, output.Sex);
        Assert.Equal(input.LocalAuthorityCode.ToString(), output.LocalAuthorityCode);
        Assert.Equal(input.IsPupilPremium ? "Yes" : "No", output.PupilPremiumLabel);
        Assert.Equal(expectedPupilIsSelected, output.IsSelected);
    }

}
