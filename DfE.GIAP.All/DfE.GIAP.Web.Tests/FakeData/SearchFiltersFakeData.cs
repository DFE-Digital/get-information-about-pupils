﻿using DfE.GIAP.Core.Models.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DfE.GIAP.Web.Tests.FakeData
{
    public class SearchFiltersFakeData
    {
        public SearchFilters GetSearchFilters()
        {
            return new SearchFilters()
            {
                CurrentFiltersApplied = new List<CurrentFilterDetail>()
                {
                    new CurrentFilterDetail()
                    {
                        FilterType = FilterType.Dob,
                        FilterName = "1/1/2015"
                    }
                },
                CurrentFiltersAppliedString = "[{\"FilterName\":\"1/1/2015\",\"FilterType\":3}]",
                CustomFilterText = new CustomFilterText()
                {
                    DobDay = 1,
                    DobMonth = 1,
                    DobYear = 2015
                }
            };
        }
    }
}
