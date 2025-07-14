using System;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
public static class CompositionRoot
{
    public static IServiceCollection AddScopedEvaluationHandler<TEvaluationRequest, TEvaluationResponse>(this IServiceCollection services)
    {
        services.AddScoped<IChainEvaluationHandler<TEvaluationRequest, TEvaluationResponse>>(provider =>
        {
            List<IEvaluator<TEvaluationRequest, TEvaluationResponse>> evaluators = provider.GetServices<IEvaluator<TEvaluationRequest, TEvaluationResponse>>().ToList();

            if (evaluators.Count == 0)
            {
                throw new InvalidOperationException($"No Evaluators of type {typeof(IEvaluator<TEvaluationRequest, TEvaluationResponse>)} registered for EvaluationHandler {typeof(IChainEvaluationHandler<TEvaluationRequest, TEvaluationResponse>)}.");
            }

            return evaluators.ChainHandlers();
        });
        return services;
    }

    private static ChainEvaluationHandler<TEvaluationRequest, TEvaluationResponse> ChainHandlers<TEvaluationRequest, TEvaluationResponse>(
        this List<IEvaluator<TEvaluationRequest, TEvaluationResponse>> evaluators)
    {
        ChainEvaluationHandler<TEvaluationRequest, TEvaluationResponse> headOutputHandler = new(evaluators[0]);
        ChainEvaluationHandler<TEvaluationRequest, TEvaluationResponse> current = headOutputHandler;

        // Chain the remaining handlers
        foreach (IEvaluator<TEvaluationRequest, TEvaluationResponse> evaluator in evaluators.Skip(1))
        {
            ChainEvaluationHandler<TEvaluationRequest, TEvaluationResponse> nextHandler = new(evaluator);
            current.ChainNextHandler(nextHandler);
            current = nextHandler;
        }


        return headOutputHandler;
    }
}
