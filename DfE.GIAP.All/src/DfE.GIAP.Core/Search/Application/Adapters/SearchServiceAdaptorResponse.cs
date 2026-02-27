namespace DfE.GIAP.Core.Search.Application.Adapters;
public record SearchServiceAdaptorResponse<TResults, TFacetResults> : ISearchServiceAdaptorResponse<TResults, TFacetResults>
{
    public TResults? Results { get; init; }
    public TFacetResults? FacetResults { get; init; }
}
