using DfE.GIAP.Domain.Models.Search.UniqueLearnerNumber;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
    public class UlnResultsFake
    {
        public async Task<List<UniqueLearnerNumberSearchResults>> GetUlnResults()
        {
            return await Task.FromResult(new List<UniqueLearnerNumberSearchResults>
            {
                new UniqueLearnerNumberSearchResults { ULN = "9998148420",
                                    Forename = "John",
                                    Surname = "Smith",
                                    Gender = 'M',
                                    DOB = DateTime.Parse("2013-02-15T00:00:00.000Z") },
                new UniqueLearnerNumberSearchResults { ULN = "9998941902",
                                Forename = "Mary",
                                Surname = "Smithson",
                                Gender = 'F',
                                DOB = DateTime.Parse("2001-02-15T00:00:00.000Z") }
            });
        }

        public async Task<NonUniqueLearnerNumberSearchResults> GetNonUlnResults(bool returnNullResultsList)
        {
            return await Task.FromResult(new NonUniqueLearnerNumberSearchResults
            {
                Count = 2,
                Filters = new NonUniqueLearnerNumberResultFilters
                {
                    GenderFilters = new List<NonUniqueLearnerNumberResultFiltersDetail>()
                    {
                        new NonUniqueLearnerNumberResultFiltersDetail()
                        {
                            Count = 0,
                            Value = ""
                        }
                    }.ToArray(),
                    ForenameFilters = new List<NonUniqueLearnerNumberResultFiltersDetail>()
                    {
                        new NonUniqueLearnerNumberResultFiltersDetail()
                        {
                            Count = 0,
                            Value = ""
                        }
                    }.ToArray(),
                    SurnameFilters = new List<NonUniqueLearnerNumberResultFiltersDetail>()
                    {
                        new NonUniqueLearnerNumberResultFiltersDetail()
                        {
                            Count = 0,
                            Value = ""
                        }
                    }.ToArray()
                },
                ResultsList = returnNullResultsList ? null : new List<NonUniqueLearnerNumberResultsList>()
                {
                    new NonUniqueLearnerNumberResultsList()
                    {
                        Score = 5,
                        Uln = "9998148420",
                        Forename = "John",
                        Surname = "Smith",
                        Gender = 'M',
                        Dob = DateTime.Parse("2013-02-15T00:00:00.000Z")
                    },
                    new NonUniqueLearnerNumberResultsList()
                    {
                        Score = 2,
                        Uln = "9998941902",
                        Forename = "Mary",
                        Surname = "Smithson",
                        Gender = 'F',
                        Dob = DateTime.Parse("2001-02-15T00:00:00.000Z")
                    }
                }.ToArray()
            });
        }

        public string GetUlns()
        {
            return "9998148420\r\n9998941902";
        }

        public string GetDuplicateUlns()
        {
            return "9999730832\r\n9999730832";
        }

        public string GetOneValidUln()
        {
            return "9999730832";
        }

        public string GetInvalidUln()
        {
            return "testuln1";
        }

        public string GetInvalidOrNotFoundUlns()
        {
            return "testuln1\r\n9999730832";
        }

        public string[] GetUlnArray()

        {
            return new string[] {
                "9998148420",
                "9998941902"
            };
        }

        public string[] GetUlnArrayDuplicates()

        {
            return new string[] {
                "9998148420",
                "9998148420"
            };
        }
    }
}