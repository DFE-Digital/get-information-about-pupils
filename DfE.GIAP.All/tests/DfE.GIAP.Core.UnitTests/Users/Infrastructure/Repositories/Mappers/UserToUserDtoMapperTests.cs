using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Mappers;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Users.Infrastructure.Repositories.Mappers;
public sealed class UserToUserDtoMapperTests
{
    [Fact]
    public void Map_WithValidUser_ReturnsMappedUserDto()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();
        User user = UserTestDoubles.WithId(userId);

        // Act
        UserToUserDtoMapper mapper = new();
        UserDto result = mapper.Map(user);

        // Assert
        Assert.Equal(userId.Value, result.id);
    }
}
