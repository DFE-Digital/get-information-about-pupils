using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.ViewModels;

namespace DfE.GIAP.Web.Extensions.Startup;

internal static class ContentExtensions
{
    internal static IServiceCollection AddContentPresentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IMapper<GetContentByPageKeyUseCaseRequest, AccessibilityViewModel>>();
        return services;
    }
}
