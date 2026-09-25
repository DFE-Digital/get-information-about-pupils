namespace DfE.GIAP.Web.Shared.Serializer;

public static class CompositionRoot
{
    public static IServiceCollection AddJsonSerializer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IJsonSerializer, NewtonsoftJsonSerializer>();
        return services;
    }
}
