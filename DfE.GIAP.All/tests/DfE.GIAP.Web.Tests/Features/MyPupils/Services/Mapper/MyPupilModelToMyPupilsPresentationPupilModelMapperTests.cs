using Bogus;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.Features.MyPupils.Application;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupils;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupils.Mapper;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Services.Mapper;
public sealed class MyPupilModelToMyPupilsPresentationPupilModelMapperTests
{
    [Fact]
    public void Handle_Throws_When_Input_Is_Null()
    {
        // Arrange
        MyPupilModelToMyPupilsPresentationPupilModelMapper sut = new();

        // Act Assert
        Func<MyPupilsPresentationPupilModel> act = () => sut.Map(null);
        Assert.Throws<ArgumentNullException>(act);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Handle_Maps_Pupil(bool isPupilPremium)
    {
        // Arrange
        MyPupilModelToMyPupilsPresentationPupilModelMapper sut = new();

        Faker<MyPupilsModel> generator = MyPupilsModelTestDoubles.CreateGenerator();
        generator.RuleFor(t => t.IsPupilPremium, isPupilPremium);
        MyPupilsModel model = generator.Generate();

        // Act
        MyPupilsPresentationPupilModel response = sut.Map(model);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(response.PupilPremiumLabel, isPupilPremium ? "Yes" : "No");
        Assert.Equal(response.Forename, model.Forename);
        Assert.Equal(response.Surname, model.Surname);
        Assert.Equal(response.UniquePupilNumber, model.UniquePupilNumber);
        Assert.Equal(response.Sex, model.Sex);
        Assert.Equal(response.DateOfBirth, model.DateOfBirth);
        Assert.Equal(response.LocalAuthorityCode, model.LocalAuthorityCode.ToString());
        Assert.False(response.IsSelected);
    }
}
