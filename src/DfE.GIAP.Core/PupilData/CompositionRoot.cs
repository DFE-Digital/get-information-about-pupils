using DfE.GIAP.Core.Ingest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.PupilData;

public static class CompositionRoot
{
    public static IServiceCollection AddPupilDataDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<NpdDataWriter>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IIngestDatasetProcessor, NpdDatasetProcessor>());
        services.TryAddTransient<PupilPremiumDataWriter>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IIngestDatasetProcessor, PupilPremiumDatasetProcessor>());
        services.TryAddTransient<FurtherEducationDataWriter>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IIngestDatasetProcessor, FurtherEducationDatasetProcessor>());

        return services;
    }
}
