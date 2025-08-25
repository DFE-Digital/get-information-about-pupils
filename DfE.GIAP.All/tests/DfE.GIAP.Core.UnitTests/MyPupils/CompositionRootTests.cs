using Azure.Search.Documents;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Search.Provider;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.User.Application.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using DfE.GIAP.SharedTests;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CompositionRoot = DfE.GIAP.Core.MyPupils.CompositionRoot;

namespace DfE.GIAP.Core.UnitTests.MyPupils;
public sealed class CompositionRootTests
{
    [Fact]
    public void ThrowsArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection? serviceCollection = null;
        Action register = () => CompositionRoot.AddMyPupilsDependencies(serviceCollection!);
        Assert.Throws<ArgumentNullException>(register);
    }

    [Fact]
    public void Registers_CompositionRoot_CanResolve_Services()
    {
        // Arrange
        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddSharedTestDependencies()
                .AddMyPupilsDependencies();

        // Act
        IServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider);

        Assert.NotNull(provider.GetService<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>());
        Assert.NotNull(provider.GetService<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>());

        Assert.NotNull(provider.GetService<IAggregatePupilsForMyPupilsApplicationService>());
        Assert.NotNull(provider.GetService<IMapper<DecoratedSearchIndexDto, Pupil>>());
        Assert.NotNull(provider.GetService<IMapper<Pupil, PupilDto>>());

        Assert.NotNull(provider.GetService<IUserReadOnlyRepository>());
        Assert.NotNull(provider.GetService<IMapper<UserDto, User.Application.User>>());

        Assert.NotNull(provider.GetService<IUserWriteRepository>());

        Assert.NotNull(provider.GetService<ISearchClientProvider>());
        Assert.NotNull(provider.GetService<IEnumerable<SearchClient>>());
        Assert.NotNull(provider.GetService<IOptions<SearchIndexOptions>>());
    }
}
