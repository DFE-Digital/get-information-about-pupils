using System.Net;
using System.Threading.Tasks;
using DfE.GIAP.Core.Models.News;

namespace DfE.GIAP.Service.News;

public interface INewsService
{
    Task<Article> UpdateNewsArticle(UpdateNewsRequestBody requestBody);
    Task<Article> UpdateNewsDocument(UpdateNewsDocumentRequestBody requestBody);
    Task<HttpStatusCode> DeleteNewsArticle(string newsId);
    Task<Article> UpdateNewsProperty(UpdateNewsDocumentRequestBody requestBody);
}
