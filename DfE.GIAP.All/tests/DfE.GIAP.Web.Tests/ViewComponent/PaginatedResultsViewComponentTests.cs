using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;
using DfE.GIAP.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Xunit;
using static DfE.GIAP.Web.ViewComponents.PaginatedResultViewComponent;

namespace DfE.GIAP.Web.Tests.ViewComponent;

public class PaginatedResultsViewComponentTests
{
    [Theory]
    [MemberData(nameof(GetPaginatedResulTestData))]
    public void Invoke_creates_correct_number_of_pages(PaginatedResultTestData testData)
    {
        List<Learner> learners = new List<Learner>();
        string pageLearnerNumbers = "test";
        string learnerNumberLabel = "label";
        bool showMiddleNames = true;
        bool showSelectedError = true;
        bool allowMultipleSelection = true;
        string controllerAction = "test";
        bool showPP = false;
        bool showLocalAuthority = true;
        string activeSortField = "test";
        string activeSortDirection = "desc";

        // act
        IViewComponentResult result = new PaginatedResultViewComponent().Invoke(
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
            showPP,
            showLocalAuthority,
            activeSortField,
            activeSortDirection
            );

        // assert
        ViewViewComponentResult viewComponentResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewComponentResult.ViewData);

        PaginatedResultModel model = Assert.IsType<PaginatedResultModel>(viewComponentResult.ViewData.Model);                
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
        List<object[]> allData =
        [
           [ // 1
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 10,
                   AvailablePages = new List<int>() { 0 },
                   ShowNext = false,
                   ShowPrevious = false,
                   PageNumber = 0
               }
           ],
           [ // 2
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 40,
                   AvailablePages = new List<int>() { 0, 1 },
                   ShowNext = true,
                   ShowPrevious = false,
                   PageNumber = 0
               }
           ],
           [ // 3
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 60,
                   AvailablePages = new List<int>() { 0, 1, 2 },
                   ShowNext = true,
                   ShowPrevious = false,
                   PageNumber = 0
               }
           ],
           [ // 4
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 80,
                   AvailablePages = new List<int>() { 0, 1, 2, 3 },
                   ShowNext = true,
                   ShowPrevious = false,
                   PageNumber = 0
               }
           ],
           [ // 5
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 100,
                   AvailablePages = new List<int>() { 0, 1, 2, 3, 4},
                   ShowNext = true,
                   ShowPrevious = false,
                   PageNumber = 0
               }
           ],
           [ // 6
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 120,
                   AvailablePages = new List<int>() { 0, 1, 2, int.MinValue, 5},
                   ShowNext = true,
                   ShowPrevious = false,
                   PageNumber = 0
               }
           ],
           [ // 7
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 140,
                   AvailablePages = new List<int>() { 0, 1, 2, int.MinValue, 6},
                   ShowNext = true,
                   ShowPrevious = false,
                   PageNumber = 0
               }
           ],
           [ // 8
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 140,
                   AvailablePages = new List<int>() { 0, 1, 2, int.MinValue, 6},
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 1
               }
           ],
              [ // 9
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 140,
                   AvailablePages = new List<int>() { 0, 1, 2, 3, int.MinValue, 6},
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 2
               }
           ],
                [ // 10
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 140,
                   AvailablePages = new List<int>() {  0, int.MinValue, 2, 3, 4, int.MinValue, 6},
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 3
               }
           ],
                  [ // 11
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, int.MinValue, 6, 7, 8, 9},
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 7
               }
           ],
                   [ // 12
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, int.MinValue, 7, 8, 9},
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 8
               }
           ],
                    [ // 13
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, int.MinValue, 7, 8, 9},
                   ShowNext = false,
                   ShowPrevious = true,
                   PageNumber = 9
               }
           ],
           [ // 14
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, 1, 2, int.MinValue, 9 },
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 1
               }
           ],
           [ // 15
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, 1, 2, int.MinValue, 9 },
                   ShowNext = true,
                   ShowPrevious = false,
                   PageNumber = 0
               }
           ],
           [ // 16
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, 1, 2, 3, int.MinValue, 9 },
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 2
               }
           ],
           [ // 17
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, int.MinValue, 4, 5, 6, int.MinValue, 9 },
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 5
               }
           ],
           [ // 18
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, int.MinValue, 5, 6, 7, int.MinValue, 9 },
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 6
               }
           ],
           [ // 19
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, int.MinValue, 6, 7, 8, 9 },
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 7
               }
           ],
           [ // 20
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, int.MinValue, 7, 8, 9 },
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 8
               }
           ],
           [ // 21
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 200,
                   AvailablePages = new List<int>() { 0, int.MinValue, 7, 8, 9 },
                   ShowNext = false,
                   ShowPrevious = true,
                   PageNumber = 9
               }
           ],
           [ // 22
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 100,
                   AvailablePages = new List<int>() { 0, 1, 2, 3, 4 },
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 3
               }
           ],
           [ // 23
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 100,
                   AvailablePages = new List<int>() { 0, 1, 2, 3, 4 },
                   ShowNext = false,
                   ShowPrevious = true,
                   PageNumber = 4
               }
           ],
           [ // 24
               new PaginatedResultTestData()
               {
                   PageSize = 20,
                   Total = 80,
                   AvailablePages = new List<int>() { 0, 1, 2, 3 },
                   ShowNext = true,
                   ShowPrevious = true,
                   PageNumber = 1
               }
           ],
        ];

        return allData;
    }
}

public class PaginatedResultTestData
{
    public int PageSize { get; set; }
    public int Total { get; set; }
    public List<int>? AvailablePages { get; set; }
    public bool ShowNext { get; set; }
    public bool ShowPrevious { get; set; }
    public int PageNumber { get; set; }
}
