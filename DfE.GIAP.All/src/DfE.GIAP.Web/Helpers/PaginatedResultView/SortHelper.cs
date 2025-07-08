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
            if (String.IsNullOrEmpty(sortField))
            {
                throw new ArgumentNullException("sortField must have a value");
            }

            if (sortField.Equals(activeSortField))
            {
                if (String.IsNullOrEmpty(activeSortDirection))
                {
                    throw new ArgumentNullException("activeSortDirection cannot be empty when an active sort is applied");
                }

                if (activeSortDirection.Equals(AzureSearchSortDirections.Ascending))
                {
                    return AriaSortAscending;
                }
                else return AriaSortDescending;                
            }
            else return AriaSortNone;
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
            if (String.IsNullOrEmpty(sortField))
            {
                throw new ArgumentNullException("sortField must have a value");
            }

            if (sortField.Equals(activeSortField))
            {
                if (String.IsNullOrEmpty(activeSortDirection))
                {
                    throw new ArgumentNullException("activeSortDirection cannot be empty when an active sort is applied");
                }

                if (activeSortDirection.Equals(AzureSearchSortDirections.Ascending))
                {
                    return AzureSearchSortDirections.Descending;
                }
                else return AzureSearchSortDirections.Ascending;
            }
            else return AzureSearchSortDirections.Ascending;
        }
    }
}
