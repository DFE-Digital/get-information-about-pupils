using System.Net;

namespace DfE.GIAP.Web.Services.ApiProcessor;

public interface IApiService
{
    Task<TApiModel> GetAsync<TApiModel>(Uri url)
        where TApiModel : class;

    Task<List<TApiModel>> GetToListAsync<TApiModel>(Uri url)
        where TApiModel : class;

    Task<HttpStatusCode> PostAsync<TModel>(Uri url, TModel model)
        where TModel : class;
}
