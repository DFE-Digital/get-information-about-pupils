﻿using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.DataTransferObjects;

namespace DfE.GIAP.Core.Users.Infrastructure.Repositories.Mappers;
public sealed class UserToUserDtoMapper : IMapper<User, UserDto>
{
    public UserDto Map(User input)
    {
        return new UserDto
        {
            id = input.UserId.Value,
            LastLoggedIn = input.LastLoggedIn,
        };
    }
}
