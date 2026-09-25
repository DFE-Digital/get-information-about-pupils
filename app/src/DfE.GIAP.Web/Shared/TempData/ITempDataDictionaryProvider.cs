using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DfE.GIAP.Web.Shared.TempData;

public interface ITempDataDictionaryProvider
{
    ITempDataDictionary GetTempData();
}
