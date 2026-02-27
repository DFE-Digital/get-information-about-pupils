namespace DfE.GIAP.Core.Search.Application.Adapters;

public interface ISearchServiceAdaptorResponse<TResults, TFacetResults>
{
    public TResults? Results { get; init; }
    public TFacetResults? FacetResults { get; init; }
}
