using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKey;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.ViewModels;

namespace DfE.GIAP.Web.Extensions.Startup;

internal static class ContentExtensions
{
    internal static IServiceCollection AddContentPresentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IMapper<GetContentByPageKeyResponse, AccessibilityViewModel>, GetContentByPageKeyResponseToAccessibilityViewModelMapper>();
        services.AddSingleton<IMapper<GetContentByPageKeyResponse, PrivacyViewModel>, GetContentByPageKeyResponseToPrivacyViewModelMapper>();
        services.AddSingleton<IMapper<GetContentByPageKeyResponse, TermsOfUseViewModel>, GetContentByPageKeyResponseToTermsOfUseViewModelMapper>();
        services.AddSingleton<IMapper<GetContentByPageKeyResponse, ConsentViewModel>, GetContentByPageKeyResponseToConsentViewModelMapper>();

        return services;
    }
}
