using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.UseCases.GetMultipleContentByKeys;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DfE.GIAP.Web.Controllers.Extensions;

internal static class ContentResultItemExtensions
{
    internal static Content FilterByContentKeyOrEmpty(this IEnumerable<ContentResultItem> items, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        IEnumerable<ContentResultItem> filteredItems = items.Where(t => t.Key.Equals(key));
        if (!filteredItems.Any())
        {
            return Content.Empty();
        }
        return filteredItems.First().Content;
    }
}
