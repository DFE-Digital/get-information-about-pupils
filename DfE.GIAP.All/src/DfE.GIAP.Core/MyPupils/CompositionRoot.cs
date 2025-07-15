using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.MyPupils;
public static class CompositionRoot
{
    public static IServiceCollection AddMyPupilsDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMaskPupilIdentifierEvaluationHandlers();
        return services;
    }

    private static IServiceCollection AddMaskPupilIdentifierEvaluationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IEvaluator<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier>, MaskIfAuthorisationContextHasDefaultedAgeRangeEvaluator>();
        services.AddScoped<IEvaluator<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier>, MaskIfDateOfBirthDoesNotExistEvaluator>();
        services.AddScoped<IEvaluator<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier>, MaskIfPupilAgeIsHigherThanAuthorisedHighestPupilAgeEvaluator>();
        services.AddScoped<IEvaluator<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier>, MaskIfPupilAgeIsLowerThanAuthorisedLowestPupilAgeRange>();
        // This must be the last registered handler
        services.AddScoped<IEvaluator<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier>, DoNotMaskDefaultEvaluator>();
        services.AddScopedEvaluationHandler<MaskPupilIdentifierRequest, ShouldMaskPupilIdentifier>();
        return services;
    }
}
