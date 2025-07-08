using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static DfE.GIAP.Web.ViewComponents.PaginatedResultViewComponent;

namespace DfE.GIAP.Web.Tests.ViewComponent
{
    public class PaginatedResultsViewComponentTests
    {
        [Theory]
        [MemberData(nameof(GetPaginatedResulTestData))]
        public void Invoke_creates_correct_number_of_pages(PaginatedResultTestData testData)
        {
            var learners = new List<Learner>();
            var pageLearnerNumbers = "test";
            var learnerNumberLabel = "label";
            var showMiddleNames = true;
            var showSelectedError = true;
            var allowMultipleSelection = true;
            var controllerAction = "test";
            var showGender = false;
            var showPP = false;
            var showLocalAuthority = true;
            var activeSortField = "test";
            var activeSortDirection = "desc";

            // act
            var result = new PaginatedResultViewComponent().Invoke(
                learners,
                pageLearnerNumbers,
                learnerNumberLabel,
                showMiddleNames,
                showSelectedError,
                allowMultipleSelection,
                testData.PageNumber,
                testData.PageSize,
                testData.Total,
                controllerAction,
                showGender,
                showPP,
                showLocalAuthority,
                activeSortField,
                activeSortDirection
                );

            // assert
            Assert.IsType<ViewViewComponentResult>(result);
            var viewComponentResult = result as ViewViewComponentResult;
            
            Assert.IsType<PaginatedResultModel>(viewComponentResult.ViewData.Model);
            var model = viewComponentResult.ViewData.Model as PaginatedResultModel;
            
            Assert.Equal(learners, model.Learners);
            Assert.Equal(pageLearnerNumbers, model.PageLearnerNumbers);
            Assert.Equal(showMiddleNames, model.ShowMiddleNames);
            Assert.Equal(showSelectedError, model.ShowNoSelectedError);
            Assert.Equal(allowMultipleSelection, model.AllowMultipleSelection);
            Assert.Equal(controllerAction, model.ControllerAction);
            Assert.Equal(activeSortField, model.ActiveSortField);
            Assert.Equal(activeSortDirection, model.ActiveSortDirection);

            Assert.Equal(testData.ShowNext, model.ShowNext);
            Assert.Equal(testData.ShowPrevious, model.ShowPrevious);
            Assert.Equal(testData.AvailablePages, model.AvailablePages);
            Assert.Equal(testData.PageNumber, model.PageNumber);
        }

        public static IEnumerable<object[]> GetPaginatedResulTestData()
        {
            var allData = new List<object[]>
            {
               new object[] { // 1
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 10,
                       AvailablePages = new List<int>() { 0 },
                       ShowNext = false,
                       ShowPrevious = false,
                       PageNumber = 0
                   }
               },
               new object[] { // 2
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 40,
                       AvailablePages = new List<int>() { 0, 1 },
                       ShowNext = true,
                       ShowPrevious = false,
                       PageNumber = 0
                   }
               },
               new object[] { // 3
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 60,
                       AvailablePages = new List<int>() { 0, 1, 2 },
                       ShowNext = true,
                       ShowPrevious = false,
                       PageNumber = 0
                   }
               },
               new object[] { // 4
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 80,
                       AvailablePages = new List<int>() { 0, 1, 2, 3 },
                       ShowNext = true,
                       ShowPrevious = false,
                       PageNumber = 0
                   }
               },
               new object[] { // 5
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 100,
                       AvailablePages = new List<int>() { 0, 1, 2, 3, 4},
                       ShowNext = true,
                       ShowPrevious = false,
                       PageNumber = 0
                   }
               },
               new object[] { // 6
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 120,
                       AvailablePages = new List<int>() { 0, 1, 2, int.MinValue, 5},
                       ShowNext = true,
                       ShowPrevious = false,
                       PageNumber = 0
                   }
               },
               new object[] { // 7
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 140,
                       AvailablePages = new List<int>() { 0, 1, 2, int.MinValue, 6},
                       ShowNext = true,
                       ShowPrevious = false,
                       PageNumber = 0
                   }
               },
               new object[] { // 8
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 140,
                       AvailablePages = new List<int>() { 0, 1, 2, int.MinValue, 6},
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 1
                   }
               },
                  new object[] { // 9
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 140,
                       AvailablePages = new List<int>() { 0, 1, 2, 3, int.MinValue, 6},
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 2
                   }
               },
                    new object[] { // 10
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 140,
                       AvailablePages = new List<int>() {  0, int.MinValue, 2, 3, 4, int.MinValue, 6},
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 3
                   }
               },
                      new object[] { // 11
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, int.MinValue, 6, 7, 8, 9},
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 7
                   }
               },
                       new object[] { // 12
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, int.MinValue, 7, 8, 9},
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 8
                   }
               },
                        new object[] { // 13
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, int.MinValue, 7, 8, 9},
                       ShowNext = false,
                       ShowPrevious = true,
                       PageNumber = 9
                   }
               },
               new object[] { // 14
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, 1, 2, int.MinValue, 9 },
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 1
                   }
               },
               new object[] { // 15
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, 1, 2, int.MinValue, 9 },
                       ShowNext = true,
                       ShowPrevious = false,
                       PageNumber = 0
                   }
               },
               new object[] { // 16
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, 1, 2, 3, int.MinValue, 9 },
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 2
                   }
               },
               new object[] { // 17
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, int.MinValue, 4, 5, 6, int.MinValue, 9 },
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 5
                   }
               },
               new object[] { // 18
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, int.MinValue, 5, 6, 7, int.MinValue, 9 },
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 6
                   }
               },
               new object[] { // 19
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, int.MinValue, 6, 7, 8, 9 },
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 7
                   }
               },
               new object[] { // 20
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, int.MinValue, 7, 8, 9 },
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 8
                   }
               },
               new object[] { // 21
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 200,
                       AvailablePages = new List<int>() { 0, int.MinValue, 7, 8, 9 },
                       ShowNext = false,
                       ShowPrevious = true,
                       PageNumber = 9
                   }
               },
               new object[] { // 22
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 100,
                       AvailablePages = new List<int>() { 0, 1, 2, 3, 4 },
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 3
                   }
               },
               new object[] { // 23
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 100,
                       AvailablePages = new List<int>() { 0, 1, 2, 3, 4 },
                       ShowNext = false,
                       ShowPrevious = true,
                       PageNumber = 4
                   }
               },
               new object[] { // 24
                   new PaginatedResultTestData()
                   {
                       PageSize = 20,
                       Total = 80,
                       AvailablePages = new List<int>() { 0, 1, 2, 3 },
                       ShowNext = true,
                       ShowPrevious = true,
                       PageNumber = 1
                   }
               },
            };

            return allData;
        }
    }
  
    public class PaginatedResultTestData
    {
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<int> AvailablePages { get; set; }
        public bool ShowNext { get; set; }
        public bool ShowPrevious { get; set; }
        public int PageNumber { get; set; }
    }
}
