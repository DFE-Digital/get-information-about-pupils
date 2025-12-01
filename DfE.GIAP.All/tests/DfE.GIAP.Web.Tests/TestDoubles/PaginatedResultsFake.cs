using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Search.Learner;
using Xunit;

namespace DfE.GIAP.Web.Tests.TestDoubles;

public class PaginatedResultsFake
{
    public PaginatedResponse GetValidLearners()
    {
        return new PaginatedResponse()
        {
            Learners = new List<Learner>()
            {
                new Learner()
                {
                    Forename = "Testy",
                    Middlenames = "T",
                    Surname = "McTester",
                    DOB = DateTime.Today.AddDays(1).AddYears(-14),
                    Sex = "M",
                    LearnerNumber = "A203102209083",
                    LearnerNumberId = "A203102209083",
                    Selected = true
                },
                new Learner()
                {
                    Forename = "Learny",
                    Middlenames = "L",
                    Surname = "McLearner",
                    DOB = DateTime.Today.AddDays(1).AddYears(-13),
                    Sex = "F",
                    LearnerNumber = "A203202811068",
                    LearnerNumberId = "A203202811068",
                    Selected = false
                }
            },
            Count = 2,
            Filters = new List<FilterData>()
            {
                new FilterData()
                {
                    Name = "Gender",
                    Items = new List<FilterDataItem>()
                    {
                        new FilterDataItem()
                        {
                            Value = "M",
                            Count = 1
                        },
                        new FilterDataItem()
                        {
                            Value = "F",
                            Count = 1
                        }
                    }
                }
            }
        };
    }

    public PaginatedResponse GetValidULNLearners()
    {
        return new PaginatedResponse()
        {
            Learners =
            [
                new Learner()
                {
                    Forename = "Testy",
                    Middlenames = "T",
                    Surname = "McTester",
                    DOB = DateTime.Today.AddDays(1).AddYears(-14),
                    Sex = "M",
                    LearnerNumber = "6424316654",
                    Selected = true
                },
                new Learner()
                {
                    Forename = "Learny",
                    Middlenames = "L",
                    Surname = "McLearner",
                    DOB = DateTime.Today.AddDays(1).AddYears(-13),
                    Sex = "F",
                    LearnerNumber = "7621706219",
                    Selected = false
                }
            ],
            Count = 2
        };
    }

    public PaginatedResponse GetInvalidLearners()
    {
        return new PaginatedResponse()
        {
            Learners = [
                new Learner()
                {
                    Forename = "Testy",
                    Middlenames = "T",
                    Surname = "McTester",
                    DOB = DateTime.Today.AddDays(1).AddYears(-13),
                    Sex = "M",
                    LearnerNumber = "A203102209083",
                    LearnerNumberId = "A203102209083",
                    Selected = true
                },
                new Learner()
                {
                    Forename = "Learny",
                    Middlenames = "L",
                    Surname = "McLearner",
                    DOB = DateTime.Today.AddDays(1).AddYears(-14),
                    Sex = "M",
                    LearnerNumber = "A203202811068",
                    LearnerNumberId = "A203202811068",
                    Selected = false
                },
                new Learner()
                {
                    Forename = "this-is-invalid",
                    Middlenames = "L",
                    Surname = "McLearner",
                    DOB = DateTime.Today.AddDays(1).AddYears(-14),
                    Sex = "M",
                    LearnerNumber = "this-is-invalid",
                    LearnerNumberId = "this-is-invalid",
                    Selected = false
                }
            ],
            Count = 3
        };
    }

    public PaginatedResponse GetInvalidULNLearners()
    {
        return new PaginatedResponse()
        {
            Learners = [
                new Learner()
                {
                    Forename = "Testy",
                    Middlenames = "T",
                    Surname = "McTester",
                    DOB = DateTime.Today.AddDays(1).AddYears(-14),
                    Sex = "M",
                    LearnerNumber = "6424316654",
                    Selected = true
                },
                new Learner()
                {
                    Forename = "Learny",
                    Middlenames = "L",
                    Surname = "McLearner",
                    DOB = DateTime.Today.AddDays(1).AddYears(-13),
                    Sex = "F",
                    LearnerNumber = "7621706219",
                    Selected = false
                },
                new Learner()
                {
                    Forename = "this-is-invalid",
                    Middlenames = "L",
                    Surname = "McLearner",
                    DOB = DateTime.Today.AddDays(1).AddYears(-14),
                    Sex = "M",
                    LearnerNumber = "123",
                    Selected = false
                }
            ],
            Count = 3
        };
    }

    public PaginatedResponse GetLearners(int number)
    {
        PaginatedResponse result = new();
        result.Count = number;

        for (int i = 1; i <= number; i++)
        {
            result.Learners.Add(new Learner()
            {
                Forename = $"Learner {i}",
                Middlenames = $"{i}",
                Surname = $"Learner {i}",
                DOB = DateTime.Today.AddDays(1).AddYears(-14),
                Sex = "M",
                LearnerNumber = $"A0123456789{i + 10}",
                LearnerNumberId = $"A0123456789{i + 10}"
            });
        }

        return result;
    }

    public string GetBase64EncodedUpn() => "QTIwMzEwMjIwOTA4Mw==-GIAP";
    public string GetUpn() => "A203102209083";
    public string GetInvalidUpn() => "this-is-invalid";
    public string GetUpns() => "A203102209083\r\nA203202811068\r\n";
    public string GetUpnsWithInvalid() => "A203102209083\r\nA203202811068\r\nthis-is-invalid\r\n";
    public string GetUlnsWithNotFound() => "6424316654\r\n7621706219\r\n753706219\r\n";
    public string GetUlnsWithDuplicates() => "6424316654\r\n7621706219\r\n7621706219\r\n";
    public string GetUpnsWithDuplicates() => "A203102209083\r\nA203202811068\r\nA203202811068\r\n";
    public string GetUpnsWithNotFound() => "A203102209083\r\nA203202811068\r\nE938218618008\r\n";
    public string GetUlns() => "6424316654\r\n7621706219\r\n";
    public string GetUlnsWithInvalid() => "6424316654\r\n7621706219\r\n123\r\n";

    public IEnumerable<MyPupilListItem> GetUpnInMPL()
    {
        List<MyPupilListItem> formattedMPLItems = [];
        foreach (string? item in GetUpn().FormatLearnerNumbers())
        {
            formattedMPLItems.Add(new MyPupilListItem(item, false));
        }
        return formattedMPLItems;
    }

    public IEnumerable<MyPupilListItem> GetUpnsInMPL()
    {
        List<MyPupilListItem> formattedMPLItems = [];
        foreach (string? item in GetUpns().FormatLearnerNumbers())
        {
            formattedMPLItems.Add(new MyPupilListItem(item, false));
        }
        return formattedMPLItems;
    }

    public string TotalSearchResultsSessionKey => "totalSearch";
    public string TotalSearchResultsSessionValue => "20";
}

public static class PaginatedResultsFakeExtensions
{
    public static void ToggleSelectAll(this PaginatedResponse response, bool selected)
    {
        foreach (var learner in response.Learners)
        {
            learner.Selected = selected;
        }
    }

    public static void AssertSelected(this IEnumerable<Learner> learners, bool selected)
    {
        foreach (var learner in learners)
        {
            Assert.Equal(selected, learner.Selected);
        }
    }
}
