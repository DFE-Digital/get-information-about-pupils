using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
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

    [Fact]
    public void Map_Maps_With_MappingApplied_For_PupilPremium()
    {
        // Arrange
        MyPupilDto createdPupilWithPupilPremium = MyPupilDtoBuilder.Create()
            .WithPupilPremium(true)
            .Build();

        MyPupilDto createdPupil = MyPupilDtoBuilder.Create()
            .WithPupilPremium(false)
            .Build();

        MyPupilDtos inputPupils = MyPupilDtos.Create([createdPupilWithPupilPremium, createdPupil]);

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
        Assert.Equal(2, response.Count);

        List<PupilViewModel> responsePupils = response.Pupils.ToList();
        AssertMappedPupil(createdPupilWithPupilPremium, responsePupils[0], expectPupilIsSelected: false);
        AssertMappedPupil(createdPupil, responsePupils[1], expectPupilIsSelected: false);
    }

    [Fact]
    public void Map_Maps_With_MappingApplied_For_IsPupilSelected()
    {
        // Arrange
        MyPupilDtos createdPupils = MyPupilDtosTestDoubles.Generate(count: 2);

        MyPupilDtoPupilSelectionStateDecoratorToPupilsViewModelMapper sut = new();

        Dictionary<IEnumerable<string>, bool> selectionStateMapping = new()
        {
            {[createdPupils.Values[0].UniquePupilNumber], true},
            {[createdPupils.Values[1].UniquePupilNumber], false}
        };

        MyPupilsPupilSelectionState selectionState =
            MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(selectionStateMapping);

        // Act
        PupilsViewModel response = sut.Map(
            new MyPupilsDtoSelectionStateDecorator(createdPupils, selectionState));

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Pupils);
        Assert.NotEmpty(response.Pupils);
        Assert.Equal(2, response.Count);

        List<PupilViewModel> responsePupils = response.Pupils.ToList();
        AssertMappedPupil(createdPupils.Values[0], responsePupils[0], expectPupilIsSelected: true);
        AssertMappedPupil(createdPupils.Values[1], responsePupils[1], expectPupilIsSelected: false);
    }

    private static void AssertMappedPupil(
        MyPupilDto input,
        PupilViewModel output,
        bool expectPupilIsSelected)
    {
        Assert.Equal(input.UniquePupilNumber, output.UniquePupilNumber);
        Assert.Equal(input.Forename, output.Forename);
        Assert.Equal(input.Surname, output.Surname);
        Assert.Equal(input.DateOfBirth, output.DateOfBirth);
        Assert.Equal(input.Sex, output.Sex);
        Assert.Equal(input.LocalAuthorityCode.ToString(), output.LocalAuthorityCode);
        Assert.Equal(input.IsPupilPremium ? "Yes" : "No", output.PupilPremiumLabel);
        Assert.Equal(expectPupilIsSelected, output.IsSelected);
    }

}
