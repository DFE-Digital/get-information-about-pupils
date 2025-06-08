﻿using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.News;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class NewsViewModel
    {
        public List<NewsArticle> Articles { get; set; }

        public CommonResponseBody NewsPublication { get; set; }

        public CommonResponseBody NewsMaintenance { get; set; }

        public static explicit operator NewsViewModel(List<NewsArticle> articles)
        {
            return new NewsViewModel { Articles = articles };
        }
    }
}
