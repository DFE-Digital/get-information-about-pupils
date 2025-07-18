using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repository;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure;
public sealed class MapUserProfileDtoToUserMapperTests
{
    [Fact]
    public void Map_ReturnsUserWithCorrectIdAndUpns_When_ValidDtoProvided()
    {
        // Arrange

        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 2);

        IEnumerable<string> pupilList = ["invalid-uon-1", upns[0].Value];

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

        UserProfileDto dto = new()
        {
            UserId = "user",
            MyPupilList = myPupilList,
            PupilList = pupilList.ToArray()
        };

        MapUserProfileDtoToUserMapper mapper = new();

        // Act
        User result = mapper.Map(dto);

        // Assert
        Assert.Equal(dto.UserId, result.UserId.Value);
        Assert.Equal(2, result.PupilIdentifiers.Count()); // invalid UPNs should be filtered out
        Assert.Contains(result.PupilIdentifiers, upn => upn.Value == upns[0].Value);
        Assert.Contains(result.PupilIdentifiers, upn => upn.Value == upns[1].Value);
    }

    //[Fact]
    //public void Map_ReturnsUserWithEmptyUpns_When_NoValidUpnsProvided()
    //{
    //    // Arrange
    //    UserProfileDto dto = new()
    //    {
    //        UserId = "user-456",
    //        MyPupilList = new List<UserProfileDto> { new() { PupilId = "invalid" } },
    //        PupilList = ["also-invalid"]
    //    };

    //    MapUserProfileDtoToUserMapper mapper = new();

    //    // Act
    //    User result = mapper.Map(dto);

    //    // Assert
    //    Assert.Equal("user-456", result.Id.Value);
    //    Assert.Empty(result.PupilIdentifiers);
    //}

    //[Fact]
    //public void Map_HandlesNullPupilList_Gracefully()
    //{
    //    // Arrange
    //    UserProfileDto dto = new()
    //    {
    //        UserId = "user-789",
    //        MyPupilList = new List<UserProfileDto> { new() { PupilId = "123456789012" } },
    //        PupilList = null
    //    };

    //    MapUserProfileDtoToUserMapper mapper = new();

    //    // Act
    //    User result = mapper.Map(dto);

    //    // Assert
    //    UniquePupilNumber uniquePupilNumber = Assert.Single(result.PupilIdentifiers);
    //    Assert.Equal("123456789012", uniquePupilNumber.Value);
    //}

}
