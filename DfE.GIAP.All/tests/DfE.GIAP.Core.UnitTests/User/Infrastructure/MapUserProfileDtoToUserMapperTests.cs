using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using DfE.GIAP.SharedTests.TestDoubles.Users;

namespace DfE.GIAP.Core.UnitTests.User.Infrastructure;
public sealed class MapUserProfileDtoToUserMapperTests
{
    [Fact]
    public void Map_ReturnsUserWithCorrectIdAndUpns_When_ValidDtoProvided()
    {
        // Arrange
        UserId userId = new("user");
        UniquePupilNumber validUpn = UniquePupilNumberTestDoubles.Generate();

        IEnumerable<MyPupilItemDto> myPupilList = [
            new()
            {
                UPN = validUpn.Value
            },
            new()
            {
                UPN = "invalid-upn-2"
            }
        ];

        UserDto dto = UserDtoTestDoubles.WithPupils(userId, myPupilList);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.UserReadRepository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(dto.id, result.UserId.Value);
        UniquePupilNumber uniquePupilNumber = Assert.Single(result.UniquePupilNumbers); // invalid UPNs should be filtered out
        Assert.Equal(validUpn, uniquePupilNumber);
    }

    [Fact]
    public void Map_ReturnsUserWithEmptyUpns_When_NoValidUpnsProvided()
    {
        // Arrange
        UserId userId = new("user");

        IEnumerable<MyPupilItemDto> myPupilList = [
            new()
            {
                UPN = "invalid-upn-2"
            }
        ];

        UserDto dto = UserDtoTestDoubles.WithPupils(userId, myPupilList);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.UserReadRepository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(userId.Value, result.UserId.Value);
        Assert.Empty(result.UniquePupilNumbers);
    }

    [Fact]
    public void Map_HandlesNullPupilList_Gracefully()
    {
        UniquePupilNumber myPupilListUpn = UniquePupilNumberTestDoubles.Generate();

        UserId userId = new("user");

        IEnumerable<MyPupilItemDto> myPupilList = [
            new()
            {
                UPN = myPupilListUpn.Value,
            },
            new()
            {
                UPN = null!
            }
        ];
        // Arrange
        UserDto dto = UserDtoTestDoubles.WithPupils(userId, myPupilList);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.UserReadRepository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(userId, result.UserId);
        UniquePupilNumber uniquePupilNumber = Assert.Single(result.UniquePupilNumbers);
        Assert.Equal(myPupilListUpn.Value, uniquePupilNumber.Value);
    }
}
