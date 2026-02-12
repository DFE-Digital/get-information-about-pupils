using System.Collections;
using DfE.GIAP.Web.Features.Search.LegacyModels;

namespace DfE.GIAP.Web.Tests.TestDoubles;

public class  DobSearchFilterTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new SearchFilters()
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
            }
        };
        yield return new object[]
        {
            new SearchFilters()
            {
                CurrentFiltersApplied = new List<CurrentFilterDetail>()
                {
                    new CurrentFilterDetail()
                    {
                        FilterType = FilterType.Dob,
                        FilterName = "1/2015"
                    }
                },
                CurrentFiltersAppliedString = "[{\"FilterName\":\"1/2015\",\"FilterType\":3}]",
                CustomFilterText = new CustomFilterText()
                {
                    DobMonth = 1,
                    DobYear = 2015
                }
            }
        };
        yield return new object[]
        {
            new SearchFilters()
            {
                CurrentFiltersApplied = new List<CurrentFilterDetail>()
                {
                    new CurrentFilterDetail()
                    {
                        FilterType = FilterType.Dob,
                        FilterName = "2015"
                    }
                },
                CurrentFiltersAppliedString = "[{\"FilterName\":\"2015\",\"FilterType\":3}]",
                CustomFilterText = new CustomFilterText()
                {
                    DobYear = 2015
                }
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
