using DfE.GIAP.Core.Models.News;

namespace DfE.GIAP.Service.News;

public interface INewsService
{
    Task<Article> UpdateNewsArticle(UpdateNewsRequestBody requestBody);
    Task<Article> UpdateNewsDocument(UpdateNewsDocumentRequestBody requestBody);
    Task<Article> UpdateNewsProperty(UpdateNewsDocumentRequestBody requestBody);
}
