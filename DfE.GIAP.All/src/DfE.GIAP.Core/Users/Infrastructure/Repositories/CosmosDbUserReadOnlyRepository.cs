﻿using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
using Microsoft.Azure.Cosmos;
using User = DfE.GIAP.Core.Users.Application.User;

namespace DfE.GIAP.Core.Users.Infrastructure.Repositories;

/// <summary>
/// Provides a read-only repository for accessing user data stored in a Cosmos DB container.
/// </summary>
internal sealed class CosmosDbUserReadOnlyRepository : IUserReadOnlyRepository
{
    private const string ContainerName = "users";
    private readonly ILoggerService _loggerService;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<UserDto, User> _userMapper;

    public CosmosDbUserReadOnlyRepository(
        ILoggerService loggerService,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<UserDto, User> userMapper)
    {
        ArgumentNullException.ThrowIfNull(loggerService);
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        ArgumentNullException.ThrowIfNull(userMapper);
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
        _loggerService = loggerService;
        _userMapper = userMapper;
    }


    /// <summary>
    /// Retrieves a user by their unique identifier asynchronously.
    /// </summary>
    /// <remarks>This method queries the underlying data store to retrieve the user associated with the
    /// provided identifier. If no user is found, an exception is thrown. Ensure that the <paramref name="id"/>
    /// parameter is valid and not null.</remarks>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <param name="ctx">An optional <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="User"/> object representing the user with the specified identifier.</returns>
    public async Task<User> GetUserByIdAsync(
        UserId id,
        CancellationToken ctx = default)
    {
        try
        {
            UserDto userDto =
                await _cosmosDbQueryHandler.ReadItemByIdAsync<UserDto>(
                    id: id.Value,
                    containerKey: ContainerName,
                    partitionKeyValue: id.Value,
                    ctx);

            ArgumentNullException.ThrowIfNull(userDto);
            User user = _userMapper.Map(userDto);

            return user;
        }
        catch (CosmosException ex)
        {
            _loggerService.LogTrace(
                level: LogLevel.Critical,
                message: $"CosmosException in {nameof(GetUserByIdAsync)}.",
                exception: ex,
                category: "Users",
                source: nameof(GetUserByIdAsync));
            throw;
        }
    }


    /// <summary>
    /// Retrieves a user by their unique identifier if the user exists in the database.
    /// </summary>
    /// <remarks>This method queries the database for a user with the specified identifier. If the user is not
    /// found, it returns <see langword="null"/> without throwing an exception. If an unexpected error occurs during the
    /// operation, the exception is logged and rethrown.</remarks>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <param name="ctx">An optional <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>The <see cref="User"/> object if a user with the specified identifier exists; otherwise, <see langword="null"/>.</returns>
    public async Task<User?> GetUserByIdIfExistsAsync(
        UserId id,
        CancellationToken ctx = default)
    {
        try
        {
            UserDto? userDto =
                await _cosmosDbQueryHandler.ReadItemByIdAsync<UserDto>(
                    id: id.Value,
                    containerKey: ContainerName,
                    partitionKeyValue: id.Value,
                    ctx);

            return userDto is not null ? _userMapper.Map(userDto) : null;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _loggerService.LogTrace(
                level: LogLevel.Information,
                message: $"User with ID '{id.Value}' not found (404) in {nameof(GetUserByIdIfExistsAsync)}.",
                exception: ex,
                category: "Users",
                source: nameof(GetUserByIdIfExistsAsync));
            return null;
        }
        catch (Exception ex)
        {
            _loggerService.LogTrace(
                level: LogLevel.Critical,
                message: $"Exception in {nameof(GetUserByIdIfExistsAsync)}.",
                exception: ex,
                category: "Users",
                source: nameof(GetUserByIdIfExistsAsync));
            throw;
        }
    }
}
