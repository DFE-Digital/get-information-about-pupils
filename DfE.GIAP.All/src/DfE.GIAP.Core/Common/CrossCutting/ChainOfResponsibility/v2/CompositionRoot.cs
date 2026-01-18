using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2;
public static class CompositionRoot
{

    public static IServiceCollection ComposeStatefulHandlers(this IServiceCollection services)
    {
        // TODO params IEvaluationHandler[] handlers? or callback to IChainedBuilder?
        return services;
    }

    public static IServiceCollection ComposeStatelessHandlers(this IServiceCollection services)
    {
        // TODO params IEvaluationHandler[] handlers? or callback to IChainedBuilder?
        return services;
    }
}
