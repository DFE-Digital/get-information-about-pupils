using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.Core.UnitTests.User.TestDoubles;
using DfE.GIAP.Core.User.Infrastructure.Repository;

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

        IEnumerable<PupilItemDto> myPupilList = [
            new()
            {
                PupilId = upns[1].Value
            },
            new()
            {
                PupilId = "invalid-upn-2"
            }
        ];

        UserProfileDto dto = UserProfileDtoTestDoubles.WithPupils(userId, pupilList, myPupilList);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(dto.UserId, result.UserId.Value);
        Assert.Equal(2, result.PupilIdentifiers.Count()); // invalid UPNs should be filtered out
        Assert.Contains(result.PupilIdentifiers, upn => upn.Value == upns[0].Value);
        Assert.Contains(result.PupilIdentifiers, upn => upn.Value == upns[1].Value);
    }

    [Fact]
    public void Map_ReturnsUserWithEmptyUpns_When_NoValidUpnsProvided()
    {
        // Arrange
        UserId userId = new("user");

        string[] pupilList = ["invalid-upn-1"];

        IEnumerable<PupilItemDto> myPupilList = [
            new()
            {
                PupilId = "invalid-upn-2"
            }
        ];

        UserProfileDto dto = UserProfileDtoTestDoubles.WithPupils(userId, pupilList, myPupilList);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(userId.Value, result.UserId.Value);
        Assert.Empty(result.PupilIdentifiers);
    }

    [Fact]
    public void Map_HandlesNullPupilList_Gracefully()
    {
        UniquePupilNumber myPupilListUpn = UniquePupilNumberTestDoubles.Generate();

        UserId userId = new("user");

        IEnumerable<PupilItemDto> myPupilList = [
            new()
            {
                PupilId = myPupilListUpn.Value
            }
        ];
        // Arrange
        UserProfileDto dto = UserProfileDtoTestDoubles.WithPupils(userId, null!, myPupilList);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(userId, result.UserId);
        UniquePupilNumber uniquePupilNumber = Assert.Single(result.PupilIdentifiers);
        Assert.Equal(myPupilListUpn.Value, uniquePupilNumber.Value);
    }


    [Fact]
    public void Map_HandlesNullMyPupilList_Gracefully()
    {
        // Arrange
        UniquePupilNumber pupilListUpn = UniquePupilNumberTestDoubles.Generate();

        UserId user = new("user");
        UserProfileDto dto = UserProfileDtoTestDoubles.WithPupils(user, [pupilListUpn.Value], null!);

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        Core.User.Application.Repository.User result = mapper.Map(dto);

        // Assert
        Assert.Equal(user, result.UserId);
        UniquePupilNumber uniquePupilNumber = Assert.Single(result.PupilIdentifiers);
        Assert.Equal(pupilListUpn.Value, uniquePupilNumber.Value);
    }
}
