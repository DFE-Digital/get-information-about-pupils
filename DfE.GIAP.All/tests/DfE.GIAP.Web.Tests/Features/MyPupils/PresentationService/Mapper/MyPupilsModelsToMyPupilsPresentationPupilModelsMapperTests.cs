using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Features.MyPupils.Application;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Mapper;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PresentationService.Mapper;
public sealed class MyPupilsModelsToMyPupilsPresentationPupilModelsMapperTests
{

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Func<MyPupilModelsToMyPupilsPresentationPupilModelMapper> construct = () => new(null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Map_Returns_Empty_When_Input_Is_Null()
    {
        // Arrange
        Mock<IMapper<MyPupilsModel, MyPupilsPresentationPupilModel>> mapperMock
            = MapperTestDoubles.MockFor<MyPupilsModel, MyPupilsPresentationPupilModel>();

        MyPupilModelsToMyPupilsPresentationPupilModelMapper mapper = new(mapperMock.Object);

        // Act
        MyPupilsPresentationPupilModels mapped =  mapper.Map(null!);

        // Assert
        Assert.NotNull(mapped);
        Assert.Empty(mapped.Values);
    }

    [Fact]
    public void Map_Returns_Empty_When_Pupils_Is_Empty()
    {
        Mock<IMapper<MyPupilsModel, MyPupilsPresentationPupilModel>> mapperMock
            = MapperTestDoubles.MockFor<MyPupilsModel, MyPupilsPresentationPupilModel>();

        MyPupilModelsToMyPupilsPresentationPupilModelMapper mapper = new(mapperMock.Object);

        // Act
        MyPupilsPresentationPupilModels mapped =
            mapper.Map(
                MyPupilsModels.Create([]));


        // Assert
        Assert.NotNull(mapped);
        Assert.Empty(mapped.Values);

        mapperMock.Verify(mapper => mapper.Map(It.IsAny<MyPupilsModel>()), Times.Never);
    }

    [Fact]
    public void Map_Calls_Mapper_When_Pupils_Exists()
    {
        const int pupilCount = 15;
        Mock<IMapper<MyPupilsModel, MyPupilsPresentationPupilModel>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsModel, MyPupilsPresentationPupilModel>();

        // Return a stub from the mapped mock
        mapperMock
            .Setup((mapper) => mapper.Map(It.IsAny<MyPupilsModel>()))
            .Returns(new MyPupilsPresentationPupilModel());

        MyPupilModelsToMyPupilsPresentationPupilModelMapper mapper = new(mapperMock.Object);

        MyPupilsModels myPupils = MyPupilsModelTestDoubles.Generate(pupilCount);

        // Act
        MyPupilsPresentationPupilModels mapped = mapper.Map(myPupils);


        // Assert
        Assert.NotNull(mapped);
        Assert.Equal(pupilCount, mapped.Values.Count);
        mapperMock.Verify(mapper => mapper.Map(It.IsAny<MyPupilsModel>()), Times.Exactly(pupilCount));
    }
}
