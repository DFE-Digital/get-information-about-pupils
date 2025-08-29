using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CheckNewsArticleUpdates;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases.CheckNewsArticleUpdate;

public sealed class CheckNewsArticleUpdateUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_NewsArticleRepositoryIsNull()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> userRepo = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CheckNewsArticleUpdatesUseCase(null!, userRepo.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_UserRepositoryIsNull()
    {
        // Arrange
        Mock<INewsArticleReadOnlyRepository> newsRepo = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CheckNewsArticleUpdatesUseCase(newsRepo.Object, null!));
    }

    [Fact]
    public async Task HandleRequestAsync_BubblesException_When_UserRepositoryThrows()
    {
        // Arrange
        Mock<INewsArticleReadOnlyRepository> newsRepo = new();
        Mock<IUserReadOnlyRepository> userRepo = new();
        CheckNewsArticleUpdatesUseCase useCase = new(newsRepo.Object, userRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        CheckNewsArticleUpdatesRequest request = new(userId.Value);

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
        Mock<INewsArticleReadOnlyRepository> newsRepo = new();
        Mock<IUserReadOnlyRepository> userRepo = new();
        CheckNewsArticleUpdatesUseCase useCase = new(newsRepo.Object, userRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        CheckNewsArticleUpdatesRequest request = new(userId.Value);
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
    public async Task HandleRequestAsync_ReturnsTrue_WhenArticlesUpdated()
    {
        // Arrange
        Mock<INewsArticleReadOnlyRepository> newsRepo = new();
        Mock<IUserReadOnlyRepository> userRepo = new();
        CheckNewsArticleUpdatesUseCase useCase = new(newsRepo.Object, userRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        CheckNewsArticleUpdatesRequest request = new(userId.Value);
        User user = UserTestDoubles.Default();

        userRepo.Setup(r => r.GetUserByIdAsync(userId, default))
            .ReturnsAsync(user);

        newsRepo.Setup(r => r.HasAnyNewsArticleBeenModifiedSinceAsync(user.LastLoggedIn))
            .ReturnsAsync(true);

        // Act
        CheckNewsArticleUpdateResponse result = await useCase.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.HasUpdates);
    }

    [Fact]
    public async Task HandleRequestAsync_ReturnsFalse_WhenNoArticlesUpdated()
    {
        // Arrange
        Mock<INewsArticleReadOnlyRepository> newsRepo = new();
        Mock<IUserReadOnlyRepository> userRepo = new();
        CheckNewsArticleUpdatesUseCase useCase = new(newsRepo.Object, userRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        CheckNewsArticleUpdatesRequest request = new(userId.Value);
        User user = UserTestDoubles.Default();

        userRepo.Setup(r => r.GetUserByIdAsync(userId, default))
            .ReturnsAsync(user);

        newsRepo.Setup(r => r.HasAnyNewsArticleBeenModifiedSinceAsync(user.LastLoggedIn))
            .ReturnsAsync(false);

        // Act
        CheckNewsArticleUpdateResponse result = await useCase.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.HasUpdates);
    }
}
