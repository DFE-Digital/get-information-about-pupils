using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;

public record UpdateNewsArticleRequest(NewsArticle NewsArticle) : IUseCaseRequest;
