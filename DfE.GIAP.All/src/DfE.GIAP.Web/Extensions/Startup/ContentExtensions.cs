﻿using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.ViewModels;

namespace DfE.GIAP.Web.Extensions.Startup;

internal static class ContentExtensions
{
    internal static IServiceCollection AddContentPresentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IMapper<GetContentByPageKeyUseCaseResponse, PrivacyViewModel>, GetContentByPageKeyResponseToPrivacyViewModelMapper>();
        services.AddSingleton<IMapper<GetContentByPageKeyUseCaseResponse, TermsOfUseViewModel>, GetContentByPageKeyResponseToTermsOfUseViewModelMapper>();

        return services;
    }
}
