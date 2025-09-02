using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel.Factory;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetMyPupils;
public sealed class PupilsViewModelFactoryTests
{
    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Func<PupilViewModelFactory> construct = () => new(null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void CreateViewModel_Throws_When_MyPupilDtos_Is_Null()
    {
        // Arrange
        Mock<IMapper<MyPupilDto, PupilViewModel>> mockMapper = MapperTestDoubles.Default<MyPupilDto, PupilViewModel>();

        PupilViewModelFactory sut = new(mockMapper.Object);

        Action act = () => sut.CreateViewModel(null, new());

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void CreateViewModel_Throws_When_SelectionState_Is_Null()
    {
        // Arrange
        Mock<IMapper<MyPupilDto, PupilViewModel>> mockMapper = MapperTestDoubles.Default<MyPupilDto, PupilViewModel>();

        PupilViewModelFactory sut = new(mockMapper.Object);

        Action act = () => sut.CreateViewModel(MyPupilDtosTestDoubles.Generate(1),  null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}
