﻿using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure.Repository;
public sealed class MapUserProfileDtoToUserMapperTests
{
    [Fact]
    public void Map_WithValidDto_ReturnsMappedUser()
    {
        // Arrange
        const int upnCount = 5;
        UserId userId = UserIdTestDoubles.Default();

        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(upnCount);
        UserDto userDto = UserDtoTestDoubles.WithPupils(userId, upns);

        // Act
        MapUserProfileDtoToUserMapper mapper = new();
        User.Application.User result = mapper.Map(userDto);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Equal(upnCount, result.UniquePupilNumbers.Count());
        Assert.Equivalent(result.UniquePupilNumbers, upns);
    }

    [Fact]
    public void Map_WithNullMyPupils_ReturnsUserWithEmptyList()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();
        UserDto userDto = new()
        {
            id = userId.Value,
            MyPupils = null!
        };

        // Act
        MapUserProfileDtoToUserMapper mapper = new();
        User.Application.User result = mapper.Map(userDto);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Empty(result.UniquePupilNumbers);
    }

    [Fact]
    public void Map_WithNullMyPupils_Pupils_ReturnsUserWithEmptyList()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();
        UserDto userDto = new()
        {
            id = userId.Value,
            MyPupils = new()
            {
                Pupils = null!
            }
        };

        // Act
        MapUserProfileDtoToUserMapper mapper = new();
        User.Application.User result = mapper.Map(userDto);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Empty(result.UniquePupilNumbers);
    }

    [Fact]
    public void Map_WithEmptyMyPupils_Pupils_ReturnsUserWithEmptyList()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();
        UserDto userDto = new()
        {
            id = userId.Value,
            MyPupils = new()
            {
                Pupils = []
            }
        };

        // Act
        MapUserProfileDtoToUserMapper mapper = new();
        User.Application.User result = mapper.Map(userDto);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Empty(result.UniquePupilNumbers);
    }
}
