using DfE.GIAP.Common.Enums;
using DfE.GIAP.Web.Helpers.PaginatedResultView;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers
{
    public class SortHelperTests
    {
        [Theory]
        [InlineData("Forename", "Forename", AzureSearchSortDirections.Ascending, SortHelper.AriaSortAscending)]
        [InlineData("Forename", "Forename", AzureSearchSortDirections.Descending, SortHelper.AriaSortDescending)]
        [InlineData("Surname", "Forename", AzureSearchSortDirections.Descending, SortHelper.AriaSortNone)]
        public void DetermineAriaSort_works_correctly(string sortField, string activeSortField, string activeSortDirection, string expectedResult)
        {
            Assert.Equal(expectedResult, SortHelper.DetermineAriaSort(sortField, activeSortField, activeSortDirection));
        }

        [Fact]
        public void DetermineAriaSort_throws_exception_if_activeSortDirection_is_empty()
        {
            Assert.Throws<ArgumentNullException>(() => SortHelper.DetermineAriaSort("Forename", "Forename", null));
        }

        [Fact]
        public void DetermineAriaSort_throws_exception_if_sortField_is_empty()
        {
            Assert.Throws<ArgumentNullException>(() => SortHelper.DetermineAriaSort("", "Forename", null));
        }

        [Theory]
        [InlineData("Forename", "Forename", AzureSearchSortDirections.Ascending, AzureSearchSortDirections.Descending)]
        [InlineData("Forename", "Forename", AzureSearchSortDirections.Descending, AzureSearchSortDirections.Ascending)]
        [InlineData("Surname", "Forename", AzureSearchSortDirections.Descending, AzureSearchSortDirections.Ascending)]
        public void DetermineSortDirection_works_correctly(string sortField, string activeSortField, string activeSortDirection, string expectedResult)
        {
            Assert.Equal(expectedResult, SortHelper.DetermineSortDirection(sortField, activeSortField, activeSortDirection));
        }

        [Fact]
        public void DetermineSortDirection_throws_exception_if_activeSortDirection_is_empty()
        {
            Assert.Throws<ArgumentNullException>(() => SortHelper.DetermineSortDirection("Forename", "Forename", null));
        }

        [Fact]
        public void DetermineSortDirection_throws_exception_if_sortField_is_empty()
        {
            Assert.Throws<ArgumentNullException>(() => SortHelper.DetermineSortDirection("", "Forename", null));
        }
    }
}
