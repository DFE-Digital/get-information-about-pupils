using DfE.GIAP.Domain.Search.Learner;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DfE.GIAP.Web.ViewComponents;

public class PaginatedResultViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        IEnumerable<Learner> learners,
        string pageLearnerNumbers,
        string learnerNumberLabel,
        bool showMiddleNames,
        bool showNoSelectedError,
        bool allowMultipleSelection,
        int pageNumber,
        int pageSize,
        int total,
        string controllerAction,
        bool showGender,
        bool showPP,
        bool showLocalAuthority,
        string activeSortField,
        string activeSortDirection)
    {
        int numberOfPages = GetNumberOfPages(pageSize, total);

        return View(new PaginatedResultModel()
        {
            Learners = learners,
            PageLearnerNumbers = pageLearnerNumbers,
            LearnerNumberLabel = learnerNumberLabel,
            ShowNoSelectedError = showNoSelectedError,
            ShowMiddleNames = showMiddleNames,
            ShowNext = pageNumber != (numberOfPages - 1),
            AllowMultipleSelection = allowMultipleSelection,
            ShowPrevious = pageNumber != 0,
            PageNumber = pageNumber,
            AvailablePages = GetAvailablePages(numberOfPages, pageNumber),
            ControllerAction = controllerAction,
            ShowGender = showGender,
            ShowPP = showPP,
            ShowLocalAuthority = showLocalAuthority,
            ActiveSortField = activeSortField,
            ActiveSortDirection = activeSortDirection
        });
    }

    private int GetNumberOfPages(int pageSize, int total)
    {
        if (total > 100000)
            return (int)Math.Ceiling(100000m / pageSize);

        return (int)Math.Ceiling((decimal)total / pageSize);
    }

    private List<int> GetAvailablePages(int numberOfPages, int pageNumber)
    {
        var available = new List<int>();

        if (numberOfPages <= 5)
        {
            for (int i = 0; i < numberOfPages; i++)
            {
                available.Add(i);
            }
        }
        else
        {
            if (pageNumber == 0 || pageNumber == 1 || pageNumber == 2)
            {
                available.Add(0);
                available.Add(1);
                available.Add(2);
                if (pageNumber == 2)
                {
                    available.Add(3);
                }
                available.Add(int.MinValue);
                available.Add(numberOfPages - 1);
            }
            else
            {
                available.Add(0);
                available.Add(int.MinValue);
            }

            if (pageNumber > 2 && pageNumber <= numberOfPages - 3)
            {
                available.Add(pageNumber - 1);
                available.Add(pageNumber);
                available.Add(pageNumber + 1);
                if (pageNumber > 2 && pageNumber != numberOfPages - 3)
                {
                    available.Add(int.MinValue);
                }
                available.Add(numberOfPages - 1);
            }
            else if (pageNumber > 2 && pageNumber > numberOfPages - 3)
            {
                available.Add(numberOfPages - 3);
                available.Add(numberOfPages - 2);
                available.Add(numberOfPages - 1);
            }
        }
        return available;
    }

    public class PaginatedResultModel
    {
        public IEnumerable<Learner> Learners { get; set; }
        public string PageLearnerNumbers { get; set; }
        public string LearnerNumberLabel { get; set; }
        public bool ShowNoSelectedError { get; set; }
        public bool ShowMiddleNames { get; set; }
        public bool ShowLocalAuthority { get; set; }
        public bool AllowMultipleSelection { get; set; }
        public bool ShowNext { get; set; }
        public bool ShowPrevious { get; set; }
        public int PageNumber { get; set; }
        public List<int> AvailablePages { get; set; } = new List<int>();
        public string ControllerAction { get; set; }
        public bool ShowPP { get; set; }
        public bool ShowGender { get; set; }
        public string ActiveSortField { get; set; }
        public string ActiveSortDirection { get; set; }
    }
}
