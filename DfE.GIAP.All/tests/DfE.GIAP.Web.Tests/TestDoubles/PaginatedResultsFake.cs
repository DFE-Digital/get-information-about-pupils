using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Search.Learner;
using Xunit;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
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
                        Gender = "M",
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
                        Gender = "F",
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
                Learners = new List<Learner>()
                {
                    new Learner()
                    {
                        Forename = "Testy",
                        Middlenames = "T",
                        Surname = "McTester",
                        DOB = DateTime.Today.AddDays(1).AddYears(-14),
                        Gender = "M",
                        LearnerNumber = "6424316654",
                        Selected = true
                    },
                    new Learner()
                    {
                        Forename = "Learny",
                        Middlenames = "L",
                        Surname = "McLearner",
                        DOB = DateTime.Today.AddDays(1).AddYears(-13),
                        Gender = "F",
                        LearnerNumber = "7621706219",
                        Selected = false
                    }
                },
                Count = 2
            };
        }

        public PaginatedResponse GetInvalidLearners()
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
                        DOB = DateTime.Today.AddDays(1).AddYears(-13),
                        Gender = "M",
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
                        Gender = "M",
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
                        Gender = "M",
                        LearnerNumber = "this-is-invalid",
                        LearnerNumberId = "this-is-invalid",
                        Selected = false
                    }
                },
                Count = 3
            };
        }

        public PaginatedResponse GetInvalidULNLearners()
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
                        Gender = "M",
                        LearnerNumber = "6424316654",
                        Selected = true
                    },
                    new Learner()
                    {
                        Forename = "Learny",
                        Middlenames = "L",
                        Surname = "McLearner",
                        DOB = DateTime.Today.AddDays(1).AddYears(-13),
                        Gender = "F",
                        LearnerNumber = "7621706219",
                        Selected = false
                    },
                    new Learner()
                    {
                        Forename = "this-is-invalid",
                        Middlenames = "L",
                        Surname = "McLearner",
                        DOB = DateTime.Today.AddDays(1).AddYears(-14),
                        Gender = "M",
                        LearnerNumber = "123",
                        Selected = false
                    }
                },
                Count = 3
            };
        }

        public PaginatedResponse GetLearners(int number)
        {
            var result = new PaginatedResponse();
            result.Count = number;

            for (int i = 1; i <= number; i++)
            {
                result.Learners.Add(new Learner()
                {
                    Forename = $"Learner {i}",
                    Middlenames = $"{i}",
                    Surname = $"Learner {i}",
                    DOB = DateTime.Today.AddDays(1).AddYears(-14),
                    Gender = "M",
                    LearnerNumber = $"A0123456789{i + 10}",
                    LearnerNumberId = $"A0123456789{i + 10}"
                });
            }

            return result;
        }

        public string GetBase64EncodedUpn()
        {
            return "QTIwMzEwMjIwOTA4Mw==-GIAP";
        }

        public string GetUpn()
        {
            return "A203102209083";
        }

        public IEnumerable<MyPupilListItem> GetUpnInMPL()
        {
            var formattedMPLItems = new List<MyPupilListItem>();
            foreach (var item in GetUpn().FormatLearnerNumbers())
            {
                formattedMPLItems.Add(new MyPupilListItem(item, false));
            }
            return formattedMPLItems;
        }

        public string GetInvalidUpn()
        {
            return "this-is-invalid";
        }

        public string GetUpns()
        {
            return "A203102209083\r\nA203202811068\r\n";
        }

        public IEnumerable<MyPupilListItem> GetUpnsInMPL()
        {
            var formattedMPLItems = new List<MyPupilListItem>();
            foreach (var item in GetUpns().FormatLearnerNumbers())
            {
                formattedMPLItems.Add(new MyPupilListItem(item, false));
            }
            return formattedMPLItems;
        }

        public string GetUpnsWithInvalid()
        {
            return "A203102209083\r\nA203202811068\r\nthis-is-invalid\r\n";
        }

        public IEnumerable<MyPupilListItem> GetUpnsWithInvalidInMPL()
        {
            var formattedMPLItems = new List<MyPupilListItem>();
            foreach (var item in GetUpnsWithInvalid().FormatLearnerNumbers())
            {
                formattedMPLItems.Add(new MyPupilListItem(item, false));
            }
            return formattedMPLItems;
        }

        public string GetUpnsWithDuplicates()
        {
            return "A203102209083\r\nA203202811068\r\nA203202811068\r\n";
        }

        public IEnumerable<MyPupilListItem> GetUpnsWithDuplicatesInMPL()
        {
            var formattedMPLItems = new List<MyPupilListItem>();
            foreach (var item in GetUpnsWithDuplicates().FormatLearnerNumbers())
            {
                formattedMPLItems.Add(new MyPupilListItem(item, false));
            }
            return formattedMPLItems;
        }

        public string GetUpnsWithNotFound()
        {
            return "A203102209083\r\nA203202811068\r\nE938218618008\r\n";
        }

        public IEnumerable<MyPupilListItem> GetUpnsWithNotFoundInMPL()
        {
            var formattedMPLItems = new List<MyPupilListItem>();
            foreach (var item in GetUpnsWithNotFound().FormatLearnerNumbers())
            {
                formattedMPLItems.Add(new MyPupilListItem(item, false));
            }
            return formattedMPLItems;
        }

        public string GetUlns()
        {
            return "6424316654\r\n7621706219\r\n";
        }

        public string GetUlnsWithInvalid()
        {
            return "6424316654\r\n7621706219\r\n123\r\n";
        }

        public string GetUlnsWithNotFound()
        {
            return "6424316654\r\n7621706219\r\n753706219\r\n";
        }

        public string GetUlnsWithDuplicates()
        {
            return "6424316654\r\n7621706219\r\n7621706219\r\n";
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
}
