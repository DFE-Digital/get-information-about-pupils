namespace DfE.GIAP.Web.Shared.TempData;

public static class CompositionRoot
{
    public static IServiceCollection AddTempData(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<ITempDataDictionaryProvider, TempDataDictionaryProvider>();
        return services;
    }
}
