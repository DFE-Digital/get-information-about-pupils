using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Domain.Models.Common;

namespace DfE.GIAP.Service.Content;

public interface IContentService
{
    Task<CommonResponseBody> GetContent(DocumentType documentType);

    Task<CommonResponseBody> SetDocumentToPublished(CommonRequestBody requestBody, AzureFunctionHeaderDetails headerDetails);

    Task<CommonResponseBody> AddOrUpdateDocument(CommonRequestBody requestBody, AzureFunctionHeaderDetails azureFunctionHeaderDetails);
}
