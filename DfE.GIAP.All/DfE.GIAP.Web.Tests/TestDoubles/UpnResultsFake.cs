using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Domain.Models.Search;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
    public class UpnResultsFake
    {
        public async Task<List<UpnResults>> GetUpnResults()
        {
            return await Task.FromResult(new List<UpnResults>
            {
                new UpnResults { UPN = "A111111111111",
                                 Forename = "John",
                                 Surname = "Smith",
                                 Gender = 'M',
                                 DOB = DateTime.Parse("2013-02-15T00:00:00.000Z") },
                new UpnResults { UPN = "A222222222222",
                                Forename = "Maid",
                                Surname = "Marion",
                                Gender = 'F',
                                DOB = DateTime.Parse("2001-02-15T00:00:00.000Z") }
        });
        }

        public IEnumerable<UpnResults> GetStoredUpnResults()
        {
            return new List<UpnResults>
            {
               new UpnResults { UPN = "A111111111111",
                                 Forename = "John",
                                 Surname = "Smith",
                                 Gender = 'M',
                                 DOB = DateTime.Parse("2013-02-15T00:00:00.000Z") },
                    new UpnResults { UPN = "A222222222222",
                                 Forename = "Maid",
                                 Surname = "Marion",
                                 Gender = 'F',
                                 DOB = DateTime.Parse("2001-02-15T00:00:00.000Z") }
            };
        }

        public async Task<NonUpnResults> GetNonUpnResults(bool returnNullResultsList)
        {
            return await Task.FromResult(new NonUpnResults
            {
                Count = 2,
                Filters = new NonUpnResultFilters
                {
                    GenderFilters = new List<NonUpnResultFiltersDetail>()
                    {
                        new NonUpnResultFiltersDetail()
                        {
                            Count = 0,
                            Value = ""
                        }
                    }.ToArray(),
                    ForenameFilters = new List<NonUpnResultFiltersDetail>()
                    {
                        new NonUpnResultFiltersDetail()
                        {
                            Count = 0,
                            Value = ""
                        }
                    }.ToArray(),
                    SurnameFilters = new List<NonUpnResultFiltersDetail>()
                    {
                        new NonUpnResultFiltersDetail()
                        {
                            Count = 0,
                            Value = ""
                        }
                    }.ToArray()
                },
                ResultsList = returnNullResultsList ? null : new List<NonUpnResultsList>()
                {
                    new NonUpnResultsList()
                    {
                        Score = 5,
                        LearnerNumber = "A202465216005",
                        Forename = "John",
                        Surname = "Smith",
                        Gender = 'M',
                        DOB = DateTime.Parse("2016-02-15T00:00:00.000Z")
                    },
                    new NonUpnResultsList()
                    {
                        Score = 2,
                        LearnerNumber = "A204286217041",
                        Forename = "Mary",
                        Surname = "Smithson",
                        Gender = 'F',
                        DOB = DateTime.Parse("2015-02-15T00:00:00.000Z")
                    }
                }.ToArray()
            });
        }

        public NonUpnResults NonUpnResults()
        {
            return new NonUpnResults
            {
                ResultsList = new NonUpnResultsList[]
                {
                    new NonUpnResultsList { LearnerNumber = "A111111111111", Forename = "John", Surname = "Smith", Gender = 'M', DOB= DateTime.Now },
                    new NonUpnResultsList { LearnerNumber = "A222222222222", Forename = "Jennifer", Surname = "Brown", Gender = 'F', DOB = DateTime.Now}
                }
            };
        }

        public MyPupilListResults MyPupilListStoredResults()
        {
            return new MyPupilListResults
            {
                PupilList = new List<PupilDetail>
                {
                    new PupilDetail
                    {
                        UniquePupilNumber = "A111111111111",
                        FirstName = "John",
                        MiddleName = "James",
                        Surname = "Smith",
                        Gender = 'M',
                        DateOfBirth = "03/02/2019",
                    },
                    new PupilDetail
                    {
                        UniquePupilNumber = "A222222222222",
                        FirstName = "Maid",
                        MiddleName = "Van",
                        Surname = "Marion",
                        Gender = 'F',
                        DateOfBirth = "03/02/2019"
                    }
                },
                MyPupilListArray = new string[2] { "A111111111111", "A222222222222" }
            };
        }

        public MyPupilListResults MyPupilListExtendedStoredResults()
        {
            return new MyPupilListResults
            {
                PupilList = new List<PupilDetail>
                {
                    new PupilDetail
                    {
                        UniquePupilNumber = "A111111111111",
                        FirstName = "John",
                        MiddleName = "James",
                        Surname = "Smith",
                        Gender = 'M',
                        DateOfBirth = "03/02/2019",
                    },
                    new PupilDetail
                    {
                        UniquePupilNumber = "A222222222222",
                        FirstName = "Maid",
                        MiddleName = "Van",
                        Surname = "Marion",
                        Gender = 'F',
                        DateOfBirth = "03/02/2019"
                    },
                    new PupilDetail
                    {
                        UniquePupilNumber = "A222222222222",
                        FirstName = "Maid",
                        MiddleName = "Van",
                        Surname = "Marion",
                        Gender = 'F',
                        DateOfBirth = "03/02/2019"
                    },
                    new PupilDetail
                    {
                        UniquePupilNumber = "A222222222222",
                        FirstName = "Maid",
                        MiddleName = "Van",
                        Surname = "Marion",
                        Gender = 'F',
                        DateOfBirth = "03/02/2019"
                    },
                    new PupilDetail
                    {
                        UniquePupilNumber = "A222222222222",
                        FirstName = "Maid",
                        MiddleName = "Van",
                        Surname = "Marion",
                        Gender = 'F',
                        DateOfBirth = "03/02/2019"
                    },
                    new PupilDetail
                    {
                        UniquePupilNumber = "A222222222222",
                        FirstName = "Maid",
                        MiddleName = "Van",
                        Surname = "Marion",
                        Gender = 'F',
                        DateOfBirth = "03/02/2019"
                    }
                },
                MyPupilListArray = new string[6] { "A111111111111", "A222222222222", "A222222222222", "A222222222222", "A222222222222", "A222222222222" }
            };
        }

        public MyPupilListResults MyPupilListEmptyResults()
        {
            return new MyPupilListResults { PupilList = new List<PupilDetail>(), MyPupilListArray = new string[0] { } };
        }

        public string GetUpns()
        {
            return "A111111111111\r\nA222222222222";
        }

        public string GetDuplicateUpns()
        {
            return "A202465216005\r\nA202465216005";
        }

        public string GetOneValidUpn()
        {
            return "A111111111111";
        }

        public string GetOneStarredUpn()
        {
            return "=";
        }

        public string GetInvalidOrNotFoundUpns()
        {
            return "testupn1\r\nA202465216005";
        }

        public string GetOneInvalidUpn()
        {
            return "1234";
        }

        public string[] GetUpnArrayOneInvalid()
        {
            return new string[] {
                "A111111111111",
                "A222222222222",
                "1234"
            };
        }

        public string[] GetUpnArray()
        {
            return new string[] {
                "A111111111111",
                "A222222222222"
            };
        }

        public string[] GetUpnArrayDuplicates()
        {
            return new string[] {
                "A111111111111",
                "A111111111111"
            };
        }
    }
}