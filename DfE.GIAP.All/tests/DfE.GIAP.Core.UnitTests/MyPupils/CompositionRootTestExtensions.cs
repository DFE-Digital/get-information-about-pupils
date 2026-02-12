using DfE.GIAP.Core.MyPupils.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.UnitTests.MyPupils;
internal static class CompositionRootTestExtensions
{
    internal static IServiceCollection AddMyPupilsCoreFakeAdaptors(this IServiceCollection services)
    {
        services.AddScoped<IQueryMyPupilsPort>(sp => new Mock<IQueryMyPupilsPort>().Object);
        return services;
    }

}
