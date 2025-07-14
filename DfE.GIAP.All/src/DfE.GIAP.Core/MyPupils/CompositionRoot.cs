using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.Rules.Abstraction;
using DfE.GIAP.Core.MyPupils.Domain.PupilIdentifierMask.Rules;
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
        services.AddScoped<IEvaluator<MaskPupilIdentifierRequest, bool>, DoNotMaskIfDefaultedAgeRangeEvaluator>();
        services.AddScoped<IEvaluator<MaskPupilIdentifierRequest, bool>, MaskIfDateOfBirthDoesNotExistEvaluator>();
        services.AddScoped<IEvaluator<MaskPupilIdentifierRequest, bool>, MaskIfPupilAgeIsHigherThanAuthorisedHighestPupilAgeEvaluator>();
        services.AddScoped<IEvaluator<MaskPupilIdentifierRequest, bool>, MaskIfPupilAgeIsLowerThanAuthorisedLowestPupilAgeRange>();
        services.AddScopedEvaluationHandler<MaskPupilIdentifierRequest, bool>();
        return services;
    }
}
