namespace DfE.GIAP.Core.Auth.Application.PostTokenHandlers;

public class ClaimsEnrichmentHandler : IPostTokenValidatedHandler
{
    private readonly IClaimsEnricher _enricher;

    public ClaimsEnrichmentHandler(IClaimsEnricher enricher)
    {
        _enricher = enricher;
    }

    public async Task HandleAsync(TokenAuthorisationContext context)
    {
        context.Principal = await _enricher.EnrichAsync(context.Principal);
    }
}
