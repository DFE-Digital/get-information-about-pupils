using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Common.CrossCutting.Patterns.Behavioural.ChainOfResponsibility;

/// <summary>
/// The composition root provides a unified location in the application where the composition
/// of the object graphs for the degree-status inference dependencies happen, using the IOC container.
/// </summary>
public static class CompositionRoot
{
    /// <summary>
    /// Extension method which provides all the pre-registrations required to invoke the degree-status inference logic.
    /// </summary>
    /// <param name="services">
    /// The originating application services on which to register the degree-status inference dependencies.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// The exception thrown if no valid <see cref="IServiceCollection"/> is provisioned.
    /// </exception>
    public static void RegisterDegreeStatusInferenceServices(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services),
                "A service collection is required to configure the 'evaluator' service dependencies.");
        }

        // Register evaluators.  
        //services.AddScoped<EvaluatorA>();  
        //services.AddScoped<EvaluatorB>();  
        //services.AddScoped<EvaluatorC>();  

        // Register the scoped evaluation handlers.  
        //services.AddScopedEvaluationHandler<EvaluatorA>();  
        //services.AddScopedEvaluationHandler<EvaluatorB>();  
        //services.AddScopedEvaluationHandler<EvaluatorC>();  
    }
}

/// <summary>
/// Extension methods which provide utility methods for the service provider.
/// </summary>
internal static class ServiceProviderExtensions
{
    /// <summary>
    /// Utility method which allows us to cleanly register a scoped evaluation handler to the service provider.
    /// </summary>
    /// <typeparam name="TEvaluationHandler">
    /// The type of evaluation handler to be assigned to the <see cref="ChainEvaluationHandler{TEvaluationRequest, TEvaluationResponse}"/>.
    /// </typeparam>
    /// <param name="services">
    /// The service collection instance to which the scoped evaluation handler is to be registered.
    /// </param>
    public static void AddScopedEvaluationHandler<TEvaluationHandler, TEvaluationRequest, TEvaluationResponse>(
        this IServiceCollection services)
            where TEvaluationHandler : IEvaluator<TEvaluationRequest, TEvaluationResponse>
            where TEvaluationRequest : class
            where TEvaluationResponse : class =>
                services.AddScoped<IChainEvaluationHandler<TEvaluationRequest, TEvaluationResponse>>(provider =>
                    provider.AddEvaluationHandler<TEvaluationHandler, TEvaluationRequest, TEvaluationResponse>());

    /// <summary>
    /// Utility method which allows us to derive an <see cref="ChainEvaluationHandler{TEvaluationRequest, TEvaluationResponse}"/> instance.
    /// </summary>
    /// <typeparam name="TEvaluationHandler">
    /// The type of evaluation handler to be assigned to the <see cref="ChainEvaluationHandler{TEvaluationRequest, TEvaluationResponse}"/>.
    /// </typeparam>
    /// <param name="serviceProvider">
    /// The service provider instance from which to extract the required TEvaluationHandler service.
    /// </param>
    /// <returns>
    /// A configured instance of the <see cref="ChainEvaluationHandler{TEvaluationRequest, TEvaluationResponse}"/>.
    /// </returns>
    public static ChainEvaluationHandler<TEvaluationRequest, TEvaluationResponse> AddEvaluationHandler<
        TEvaluationHandler, TEvaluationRequest, TEvaluationResponse>(this IServiceProvider serviceProvider)
            where TEvaluationHandler : IEvaluator<TEvaluationRequest, TEvaluationResponse>
            where TEvaluationRequest : class
            where TEvaluationResponse : class =>
                new(serviceProvider.CreateScope()
                    .ServiceProvider.GetRequiredService<TEvaluationHandler>());
}
