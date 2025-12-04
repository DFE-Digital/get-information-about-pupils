using DfE.GIAP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Helpers.PaginatedResultView
{
    public static class SortHelper
    {
        public const string AriaSortNone = "none";
        public const string AriaSortAscending = "ascending";
        public const string AriaSortDescending = "descending";

        /// <summary>
        /// Determines what the aria sort should be for a column depending on which column is currently active.
        /// </summary>
        /// <param name="sortField">the column name</param>
        /// <param name="activeSortField">which column is active</param>
        /// <param name="activeSortDirection">which direction it is actively sorting.</param>
        /// <returns></returns>
        public static string DetermineAriaSort(string sortField, string activeSortField, string activeSortDirection)
        {
            ArgumentException.ThrowIfNullOrEmpty(sortField);

            if (!sortField.Equals(activeSortField, StringComparison.Ordinal))
            {
                return AriaSortNone;
            }

            ArgumentException.ThrowIfNullOrEmpty(activeSortDirection);

            return activeSortDirection.Equals(AzureSearchSortDirections.Ascending) ?
                AriaSortAscending :
                    AriaSortDescending;
        }

        /// <summary>
        /// Determines the sort direction that will be applied if the user clicks the column header.
        /// </summary>
        /// <param name="sortField">the column name</param>
        /// <param name="activeSortField">which column is active</param>
        /// <param name="activeSortDirection">which direction it is actively sorting. </param>
        /// <returns></returns>
        public static string DetermineSortDirection(string sortField, string activeSortField, string activeSortDirection)
        {
            ArgumentException.ThrowIfNullOrEmpty(sortField);

            if (!sortField.Equals(activeSortField))
            {
                return AzureSearchSortDirections.Ascending;
            }
            ArgumentException.ThrowIfNullOrEmpty(activeSortDirection);

            return activeSortDirection.Equals(AzureSearchSortDirections.Ascending) ?
                AzureSearchSortDirections.Descending :
                    AzureSearchSortDirections.Ascending;
        }
    }
}
