using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write.Mapper;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetMyPupils;
public sealed class MapMyPupilDtoSelectionStateDecoratorToPupilsViewModelTests
{
    [Fact]
    public void Map_Throws_When_Input_Is_Null()
    {
        // Arrange
        MyPupilDtoPupilSelectionStateDecoratorToPupilsViewModelMapper mapper = new();
        Action act = () => mapper.Map(null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_Returns_EmptyList_When_Input_Is_EmptyList()
    {
        // Arrange
        MyPupilDtoPupilSelectionStateDecoratorToPupilsViewModelMapper mapper = new();

        MyPupilsDtoSelectionStateDecorator mappable = new(
            MyPupilDtos.Create(pupils: []),
            MyPupilsPupilSelectionStateTestDoubles.Default());

        // Act
        PupilsViewModel response = mapper.Map(mappable);

        Assert.NotNull(response);
        Assert.Empty(response.Pupils);
        Assert.Equal(0, response.Count);
    }
}
