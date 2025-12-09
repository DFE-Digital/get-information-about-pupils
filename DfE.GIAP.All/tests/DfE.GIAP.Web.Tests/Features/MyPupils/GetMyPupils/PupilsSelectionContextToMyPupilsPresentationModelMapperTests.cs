using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Mapper;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetPupilViewModels;
public sealed class PupilsSelectionContextToMyPupilsPresentationModelMapperTests
{
    [Fact]
    public void Map_Throws_When_Input_Is_Null()
    {
        // Arrange
        MyPupilsModelToMyPupilsPresentationPupilModel sut = new();
        Action act = () => sut.Map(null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_Returns_EmptyList_When_Input_Is_EmptyList()
    {
        // Arrange
        PupilsSelectionContext mappable = new(
            MyPupilsModel.Create(pupils: []),
            MyPupilsPupilSelectionStateTestDoubles.Default());


        MyPupilsModelToMyPupilsPresentationPupilModel sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Map(mappable);

        Assert.NotNull(response);
        Assert.Empty(response.Values);
        Assert.Equal(0, response.Count);
    }

    [Fact]
    public void Map_Maps_With_MappingApplied_For_PupilPremium()
    {
        // Arrange
        MyPupilModel createdPupilWithPupilPremium = MyPupilDtoBuilder.Create()
            .WithPupilPremium(true)
            .Build();

        MyPupilModel createdPupil = MyPupilDtoBuilder.Create()
            .WithPupilPremium(false)
            .Build();

        MyPupilsModel inputPupils = MyPupilsModel.Create([createdPupilWithPupilPremium, createdPupil]);

        MyPupilsModelToMyPupilsPresentationPupilModel sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Map(
            new PupilsSelectionContext(
                inputPupils,
                MyPupilsPupilSelectionStateTestDoubles.Default()));

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Values);
        Assert.NotEmpty(response.Values);
        Assert.Equal(2, response.Count);

        List<MyPupilsPresentationPupilModel> responsePupils = response.Values.ToList();
        AssertMappedPupil(createdPupilWithPupilPremium, responsePupils[0], expectPupilIsSelected: false);
        AssertMappedPupil(createdPupil, responsePupils[1], expectPupilIsSelected: false);
    }

    [Fact]
    public void Map_Maps_With_MappingApplied_For_IsPupilSelected()
    {
        // Arrange
        MyPupilsModel createdPupils = MyPupilDtosTestDoubles.Generate(count: 2);


        MyPupilsModelToMyPupilsPresentationPupilModel sut = new();

        Dictionary<List<string>, bool> selectionStateMapping = new()
        {
            { [createdPupils.Values[0].UniquePupilNumber], true},
            { [createdPupils.Values[1].UniquePupilNumber], false}
        };

        MyPupilsPupilSelectionState selectionState =
            MyPupilsPupilSelectionStateTestDoubles.WithPupilsSelectionState(selectionStateMapping);

        // Act
        MyPupilsPresentationPupilModels response = sut.Map(
            new PupilsSelectionContext(createdPupils, selectionState));

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Values);
        Assert.NotEmpty(response.Values);
        Assert.Equal(2, response.Count);

        List<MyPupilsPresentationPupilModel> responsePupils = response.Values.ToList();
        AssertMappedPupil(createdPupils.Values[0], responsePupils[0], expectPupilIsSelected: true);
        AssertMappedPupil(createdPupils.Values[1], responsePupils[1], expectPupilIsSelected: false);
    }

    private static void AssertMappedPupil(
        MyPupilModel input,
        MyPupilsPresentationPupilModel output,
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
