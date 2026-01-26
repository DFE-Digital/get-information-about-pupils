using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Mappers;
using DfE.GIAP.SharedTests.TestDoubles;
using User = DfE.GIAP.Core.Users.Application.Models.User;

namespace DfE.GIAP.Core.UnitTests.Users.Infrastructure.Repositories.Mappers;
public sealed class UserDtoToUserMapperTests
{
    [Fact]
    public void Map_WithValidDto_ReturnsMappedUser()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();

        UserDto userDto = UserDtoTestDoubles.WithId(userId);

        // Act
        UserDtoToUserMapper mapper = new();
        User result = mapper.Map(userDto);

        // Assert
        Assert.Equal(userId, result.UserId);
    }
}
