using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DfE.GIAP.Web.Shared.TempData;

public sealed class TempDataDictionaryProvider : ITempDataDictionaryProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    public TempDataDictionaryProvider(
        IHttpContextAccessor httpContextAccessor,
        ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
    }
    public ITempDataDictionary GetTempData()
    {
        HttpContext context = _httpContextAccessor.HttpContext;
        ArgumentNullException.ThrowIfNull(context);
        return _tempDataDictionaryFactory.GetTempData(context);
    }
}
