using System.Globalization;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.Handlers;

/// <summary>
/// Handles the Date of Birth (DOB) filter logic for text-based learner search.
/// Converts user-entered DOB components into structured filter values.
/// </summary>
public class DobFilterHandler : IFilterHandler
{
    /// <summary>
    /// ISO 8601 format used for full DOB filtering.
    /// </summary>
    private const string DateOfBirthStringFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

    /// <summary>
    /// Applies DOB filter logic to the requestFilters dictionary.
    /// Supports full date filtering or partial (month/year or year-only) filtering.
    /// </summary>
    /// <param name="filter">Current filter meta-data.</param>
    /// <param name="model">User-entered search model.</param>
    /// <param name="requestFilters">Dictionary to populate with DOB filter values.</param>
    public void Apply(CurrentFilterDetail filter, LearnerTextSearchViewModel model, Dictionary<string, string[]> requestFilters)
    {
        // Skip processing if DOB input has validation errors.
        if (model.FilterErrors.DobError) return;

        CustomFilterText dobText = model.SearchFilters.CustomFilterText;

        // If no DOB components are entered, attempt to parse from filter name (fall-back).
        if (dobText.DobDay == 0 && dobText.DobMonth == 0 && dobText.DobYear == 0)
        {
            PupilHelper.ConvertFilterNameToCustomDOBFilterText(filter.FilterName, out int day, out int month, out int year);
            dobText.DobDay = day;
            dobText.DobMonth = month;
            dobText.DobYear = year;
        }

        // Apply full DOB filter if all components are present.
        if (dobText.DobDay > 0 && dobText.DobMonth > 0 && dobText.DobYear > 0)
        {
            AddFullDobFilter(filter.FilterName, requestFilters);
        }
        else
        {
            // Apply partial DOB filter (month/year or year-only).
            AddPartialDobFilter(dobText, requestFilters);
        }
    }

    /// <summary>
    /// Adds a full ISO-formatted DOB filter to the request.
    /// </summary>
    /// <param name="filterValue">Raw filter string to parse into a date.</param>
    /// <param name="requestFilters">Dictionary to populate with ISO DOB.</param>
    private void AddFullDobFilter(string filterValue, Dictionary<string, string[]> requestFilters)
    {
        DateTime parsedDate = DateTime.ParseExact(filterValue, "d/M/yyyy", CultureInfo.InvariantCulture);
        string isoDate = parsedDate.ToString(DateOfBirthStringFormat, CultureInfo.InvariantCulture);
        requestFilters[FilterKeys.Dob] = [isoDate];
    }

    /// <summary>
    /// Adds partial DOB filters (month/year or year only) to the request.
    /// </summary>
    /// <param name="dobText">User-entered DOB components.</param>
    /// <param name="requestFilters">Dictionary to populate with partial DOB values.</param>
    private void AddPartialDobFilter(CustomFilterText dobText, Dictionary<string, string[]> requestFilters)
    {
        if (dobText.DobMonth == 0)
        {
            // Year-only filter - TODO: we need to configure these with the appropriate search filter expressions i.e. equals for this!
            requestFilters[FilterKeys.DobYear] = [dobText.DobYear.ToString()];
        }
        else
        {
            // Month and year filter - TODO: we need to configure these with the appropriate search filter expressions i.e. contains!
            requestFilters[FilterKeys.DobMonth] = [dobText.DobMonth.ToString()];
            requestFilters[FilterKeys.DobYear] = [dobText.DobYear.ToString()];
        }
    }

    /// <summary>
    /// Centralized keys for DOB-related filters used in request payloads.
    /// These keys map to expected query parameters in downstream search APIs.
    /// </summary>
    internal static class FilterKeys
    {
        /// <summary>
        /// Key for full ISO-formatted date of birth (e.g., "2005-03-15T00:00:00.000Z").
        /// Used when day, month, and year are all provided.
        /// </summary>
        public const string Dob = "DOB";

        /// <summary>
        /// Key for year-only DOB filter (e.g., "2005").
        /// Used when only the year is known or entered.
        /// </summary>
        public const string DobYear = "dobyear";

        /// <summary>
        /// Key for month component of DOB (e.g., "3").
        /// Used in combination with DobYear when day is missing.
        /// </summary>
        public const string DobMonth = "dobmonth";
    }

}
