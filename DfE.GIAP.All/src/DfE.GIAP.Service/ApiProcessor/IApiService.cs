﻿using DfE.GIAP.Domain.Models.Common;
using System.Net;

namespace DfE.GIAP.Service.ApiProcessor;

public interface IApiService
{
    Task<TApiModel> GetAsync<TApiModel>(Uri url)
        where TApiModel : class;

    Task<List<TApiModel>> GetToListAsync<TApiModel>(Uri url)
        where TApiModel : class;

    Task<HttpStatusCode> PostAsync<TModel>(Uri url, TModel model)
        where TModel : class;

    Task<TResponseModel> PostAsync<TRequestModel, TResponseModel>(Uri url, TRequestModel model, AzureFunctionHeaderDetails headerDetails)
        where TRequestModel : class
        where TResponseModel : class;
}
