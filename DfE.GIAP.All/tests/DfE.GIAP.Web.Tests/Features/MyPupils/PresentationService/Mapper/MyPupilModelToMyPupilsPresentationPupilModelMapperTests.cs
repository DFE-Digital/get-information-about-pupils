using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Mapper;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PresentationService.Mapper;
public sealed class MyPupilModelToMyPupilsPresentationPupilModelMapperTests
{

    [Fact]
    public void Handle_Throws_When_Input_Is_Null()
    {
        // Arrange
        MyPupilModelToMyPupilsPresentationPupilModelMapper mapper = new();

        // Act Assert
        Func<MyPupilsPresentationPupilModel> act = () => mapper.Map(null);
        Assert.Throws<ArgumentNullException>(act);
    }

    /*    [Fact]
        public void Map_Throws_When_Input_Is_Null()
        {
            // Arrange
            MyPupilModelsToMyPupilsPresentationPupilModelMapper sut = new();
            Action act = () => sut.Map(null);

            // Act Assert
            Assert.Throws<ArgumentNullException>(act);
        }*/

    /*    [Fact]
        public void Map_Returns_EmptyList_When_Input_Is_EmptyList()
        {
            // Arrange
            MyPupilsModels pupils = MyPupilsModels.Create(pupils: []);

            MyPupilModelsToMyPupilsPresentationPupilModelMapper sut = new();

            // Act
            MyPupilsPresentationPupilModels response = sut.Map(pupils);

            Assert.NotNull(response);
            Assert.Empty(response.Values);
            Assert.Equal(0, response.Count);
        }

        [Fact]
        public void Map_Maps_With_MappingApplied_For_PupilPremium()
        {
            // Arrange
            MyPupilsModel createdPupilWithPupilPremium = MyPupilDtoBuilder.Create()
                .WithPupilPremium(true)
                .Build();

            MyPupilsModel createdPupil = MyPupilDtoBuilder.Create()
                .WithPupilPremium(false)
                .Build();

            MyPupilsModels inputPupils = MyPupilsModels.Create([createdPupilWithPupilPremium, createdPupil]);

            MyPupilModelsToMyPupilsPresentationPupilModelMapper sut = new();

            // Act
            MyPupilsPresentationPupilModels response = sut.Map(inputPupils);

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
            MyPupilsModels createdPupils = MyPupilModelTestDoubles.Generate(count: 2);


            MyPupilModelToMyPupilsPresentationPupilModelMapper sut = new();

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
            MyPupilsModel input,
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
    */
}
