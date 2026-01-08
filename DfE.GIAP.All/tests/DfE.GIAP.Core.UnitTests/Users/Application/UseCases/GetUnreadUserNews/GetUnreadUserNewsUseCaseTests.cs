using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Users.Application.UseCases.GetUnreadUserNews;
public sealed class GetUnreadUserNewsUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_NewsArticleRepositoryIsNull()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> userRepo = UserReadOnlyRepositoryTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetUnreadUserNewsUseCase(
                newsArticleReadRepository: null!,
                userReadOnlyRepository: userRepo.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_UserRepositoryIsNull()
    {
        // Arrange
        Mock<INewsArticleReadOnlyRepository> newsRepo = NewsArticleReadOnlyRepositoryTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetUnreadUserNewsUseCase(
                newsArticleReadRepository: newsRepo.Object,
                userReadOnlyRepository: null!));
    }

    [Fact]
    public async Task HandleRequestAsync_BubblesException_When_UserRepositoryThrows()
    {
        // Arrange
        Mock<INewsArticleReadOnlyRepository> newsRepo = NewsArticleReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserReadOnlyRepository> userRepo = UserReadOnlyRepositoryTestDoubles.Default();
        GetUnreadUserNewsUseCase useCase = new(
            newsArticleReadRepository: newsRepo.Object,
            userReadOnlyRepository: userRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        GetUnreadUserNewsRequest request = new(userId.Value);

        userRepo.Setup(r => r.GetUserByIdAsync(userId, default))
            .ThrowsAsync(new Exception("User repository exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            useCase.HandleRequestAsync(request));
    }

    [Fact]
    public async Task HandleRequestAsync_BubblesException_When_NewsArticleRepositoryThrows()
    {
        // Arrange
        Mock<INewsArticleReadOnlyRepository> newsRepo = NewsArticleReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserReadOnlyRepository> userRepo = UserReadOnlyRepositoryTestDoubles.Default();
        GetUnreadUserNewsUseCase useCase = new(
            newsArticleReadRepository: newsRepo.Object,
            userReadOnlyRepository: userRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        GetUnreadUserNewsRequest request = new(userId.Value);
        User user = UserTestDoubles.Default();

        userRepo.Setup(r => r.GetUserByIdAsync(userId, default))
            .ReturnsAsync(user);

        newsRepo.Setup(r => r.HasAnyNewsArticleBeenModifiedSinceAsync(user.LastLoggedIn))
            .ThrowsAsync(new Exception("News repository exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            useCase.HandleRequestAsync(request));
    }

    [Fact]
    public async Task HandleRequestAsync_ReturnsTrue_WhenUserHasUnreadNews()
    {
        // Arrange
        Mock<INewsArticleReadOnlyRepository> newsRepo = NewsArticleReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserReadOnlyRepository> userRepo = UserReadOnlyRepositoryTestDoubles.Default();
        GetUnreadUserNewsUseCase useCase = new(
            newsArticleReadRepository: newsRepo.Object,
            userReadOnlyRepository: userRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        GetUnreadUserNewsRequest request = new(userId.Value);
        User user = UserTestDoubles.Default();

        userRepo.Setup(r => r.GetUserByIdAsync(userId, default))
            .ReturnsAsync(user);

        newsRepo.Setup(r => r.HasAnyNewsArticleBeenModifiedSinceAsync(user.LastLoggedIn))
            .ReturnsAsync(true);

        // Act
        GetUnreadUserNewsResponse result = await useCase.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.HasUpdates);
    }

    [Fact]
    public async Task HandleRequestAsync_ReturnsFalse_WhenUserDoesNotHaveUnreadNews()
    {
        // Arrange
        Mock<INewsArticleReadOnlyRepository> newsRepo = NewsArticleReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserReadOnlyRepository> userRepo = UserReadOnlyRepositoryTestDoubles.Default();
        GetUnreadUserNewsUseCase useCase = new(
            newsArticleReadRepository: newsRepo.Object,
            userReadOnlyRepository: userRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        GetUnreadUserNewsRequest request = new(userId.Value);
        User user = UserTestDoubles.Default();

        userRepo.Setup(r => r.GetUserByIdAsync(userId, default))
            .ReturnsAsync(user);

        newsRepo.Setup(r => r.HasAnyNewsArticleBeenModifiedSinceAsync(user.LastLoggedIn))
            .ReturnsAsync(false);

        // Act
        GetUnreadUserNewsResponse result = await useCase.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.HasUpdates);
    }
}
