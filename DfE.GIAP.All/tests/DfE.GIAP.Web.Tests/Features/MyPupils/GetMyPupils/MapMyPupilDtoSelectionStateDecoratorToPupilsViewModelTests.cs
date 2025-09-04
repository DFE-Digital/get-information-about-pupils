using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetMyPupils;
public sealed class MapMyPupilDtoSelectionStateDecoratorToPupilsViewModelTests
{
    [Fact]
    public void Map_Throws_When_Input_Is_Null()
    {
        // Arrange
        MapMyPupilDtoSelectionStateDecoratorToPupilsViewModel mapper = new();
        Action act = () => mapper.Map(null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}
