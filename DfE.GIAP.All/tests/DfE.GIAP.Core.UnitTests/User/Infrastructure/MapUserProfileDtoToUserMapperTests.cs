using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
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
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 2);

        string[] pupilList = ["invalid-upn-1", upns[0].Value];

        IEnumerable<MyPupilItemDto> myPupilList = [
            new()
            {
                UPN = upns[1].Value
            },
            new()
            {
                UPN = "invalid-upn-2"
            }
        ];

        UserDto dto = UserDtoTestDoubles.WithPupils(userId, pupilList, myPupilList);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.UserReadRepository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(dto.id, result.UserId.Value);
        Assert.Equal(2, result.PupilIds.Select(t => t.UniquePupilNumber).Count()); // invalid UPNs should be filtered out
        Assert.Contains(result.PupilIds.Select(t => t.UniquePupilNumber), upn => upn.Value == upns[0].Value);
        Assert.Contains(result.PupilIds.Select(t => t.UniquePupilNumber), upn => upn.Value == upns[1].Value);
    }

    [Fact]
    public void Map_ReturnsUserWithEmptyUpns_When_NoValidUpnsProvided()
    {
        // Arrange
        UserId userId = new("user");

        string[] pupilList = ["invalid-upn-1"];

        IEnumerable<MyPupilItemDto> myPupilList = [
            new()
            {
                UPN = "invalid-upn-2"
            }
        ];

        UserDto dto = UserDtoTestDoubles.WithPupils(userId, pupilList, myPupilList);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.UserReadRepository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(userId.Value, result.UserId.Value);
        Assert.Empty(result.PupilIds.Select(t => t.UniquePupilNumber));
    }

    [Fact]
    public void Map_HandlesNullPupilList_Gracefully()
    {
        UniquePupilNumber myPupilListUpn = UniquePupilNumberTestDoubles.Generate();

        UserId userId = new("user");

        IEnumerable<MyPupilItemDto> myPupilList = [
            new()
            {
                UPN = myPupilListUpn.Value
            }
        ];
        // Arrange
        UserDto dto = UserDtoTestDoubles.WithPupils(userId, null!, myPupilList);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.UserReadRepository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(userId, result.UserId);
        UniquePupilNumber uniquePupilNumber = Assert.Single(result.PupilIds.Select(t => t.UniquePupilNumber));
        Assert.Equal(myPupilListUpn.Value, uniquePupilNumber.Value);
    }


    [Fact]
    public void Map_HandlesNullMyPupilList_Gracefully()
    {
        // Arrange
        UniquePupilNumber pupilListUpn = UniquePupilNumberTestDoubles.Generate();

        UserId user = new("user");
        UserDto dto = UserDtoTestDoubles.WithPupils(user, [pupilListUpn.Value], null!);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.UserReadRepository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(user, result.UserId);
        UniquePupilNumber uniquePupilNumber = Assert.Single(result.PupilIds.Select(t => t.UniquePupilNumber));
        Assert.Equal(pupilListUpn.Value, uniquePupilNumber.Value);
    }
}
