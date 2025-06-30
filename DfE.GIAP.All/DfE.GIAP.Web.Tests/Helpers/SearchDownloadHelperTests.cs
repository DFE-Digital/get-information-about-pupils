using DfE.GIAP.Common.Constants.DsiConfiguration;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers
{
    public class SearchDownloadHelperTests
    {
        [Theory]
        [MemberData(nameof(GetSearchDownloadDataTypeData))]
        public void AddDownloadDataTypes_correctly_handles_rbac(DownloadDataTypeTestData test)
        {
            // Arrange
            var model = new LearnerDownloadViewModel();

            // Act
            SearchDownloadHelper.AddDownloadDataTypes(model, test.User, test.LowAge, test.HighAge, test.IsLA, test.User.IsOrganisationAllAges());

            // Assert
            Assert.True(test.ExpectedDataTypes.SequenceEqual(model.SearchDownloadDatatypes));
        }

        [Theory]
        [MemberData(nameof(GetFESearchDownloadDataTypeData))]
        public void AddUlnDownloadDataTypes_correctly_handles_rbac(DownloadDataTypeTestData test)
        {
            // Arrange
            var model = new LearnerDownloadViewModel();

            // Act
            SearchDownloadHelper.AddUlnDownloadDataTypes(model, test.User, test.HighAge, test.IsDfe);

            // Assert
            Assert.True(test.ExpectedDataTypes.SequenceEqual(model.SearchDownloadDatatypes));
        }

        [Fact]
        public void DownloadFile_returns_FileContentResult_for_non_zip()
        {
            // Arrange
            var downloadFile = new ReturnFile()
            {
                FileType = "plain",
                Bytes = new byte[0],
                FileName = "test.txt"
            };

            // Act
            var result = SearchDownloadHelper.DownloadFile(downloadFile);

            // Assert
            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public void DisableDownloadDataTypes_correctly_disables_types()
        {
            // arrange
            var model =
                new LearnerDownloadViewModel
                {
                    SearchDownloadDatatypes =
                        new List<SearchDownloadDataType> {
                            SearchDownloadDataTypeBuilder.Create()
                                .WithName("Key Stage 1")
                                .WithValue("KS1").Build()
                        }
                };

            var notAvailable = new List<DownloadDataType>() { DownloadDataType.KS1 };

            // act
            SearchDownloadHelper.DisableDownloadDataTypes(model, notAvailable);

            // assert
            Assert.True(model.SearchDownloadDatatypes.First().Disabled);
        }

        internal class SearchDownloadDataTypeListBuilder
        {
            private Dictionary<string, SearchDownloadDataType> _searchDownloadDataTypeDictionary;

            public static SearchDownloadDataTypeListBuilder Create() => new SearchDownloadDataTypeListBuilder();

            public SearchDownloadDataTypeListBuilder WithDefaultSearchDownloadDataTypeList()
            {
                _searchDownloadDataTypeDictionary = defaultSearchDownloadDataTypeDictionary;
                return this;
            }

            public SearchDownloadDataTypeListBuilder WithCannotDownloadForDataTypes(List<string> cannotDownloadDataTypes)
            {
                if (_searchDownloadDataTypeDictionary != null)
                {
                    cannotDownloadDataTypes.ForEach(cannotDownloadDataType =>
                        _searchDownloadDataTypeDictionary[cannotDownloadDataType].CanDownload = false);
                }
                return this;
            }

            public List<SearchDownloadDataType> Build() => _searchDownloadDataTypeDictionary.Values.ToList();

            private readonly Dictionary<string, SearchDownloadDataType> defaultSearchDownloadDataTypeDictionary =
                new Dictionary<string, SearchDownloadDataType> {
                    { AutumnCensusDataTypeKey,
                        SearchDownloadDataTypeBuilder.Create().WithName("Autumn Census").WithValue("Census_Autumn").Build() },
                    { SpringCensusDataTypeKey,
                        SearchDownloadDataTypeBuilder.Create().WithName("Spring Census").WithValue("Census_Spring").Build() },
                    { SummerCensusDataTypeKey,
                        SearchDownloadDataTypeBuilder.Create().WithName("Summer Census").WithValue("Census_Summer").Build() },
                    { EYFSPDataTypeKey,
                        SearchDownloadDataTypeBuilder.Create().WithName("EYFSP").WithValue("EYFSP").Build() },
                    { KeyStage1DataTypeKey,
                        SearchDownloadDataTypeBuilder.Create().WithName("Key Stage 1").WithValue("KS1").Build() },
                    { KeyStage2DataTypeKey,
                        SearchDownloadDataTypeBuilder.Create().WithName("Key Stage 2").WithValue("KS2").Build() },
                    { KeyStage4DataTypeKey,
                        SearchDownloadDataTypeBuilder.Create().WithName("Key Stage 4").WithValue("KS4").Build() },
                    { PhonicsDataTypeKey,
                        SearchDownloadDataTypeBuilder.Create().WithName("Phonics").WithValue("Phonics").Build() },
                    { MtcDataTypeKey,
                        SearchDownloadDataTypeBuilder.Create().WithName("MTC").WithValue("MTC").Build() }
            };

            public static readonly string AutumnCensusDataTypeKey = "AutumnCensus";
            public static readonly string SpringCensusDataTypeKey = "SpringCensus";
            public static readonly string SummerCensusDataTypeKey = "SummerCensus";
            public static readonly string EYFSPDataTypeKey = "EYFSP";
            public static readonly string KeyStage1DataTypeKey = "KeyStage1";
            public static readonly string KeyStage2DataTypeKey = "KeyStage2";
            public static readonly string KeyStage4DataTypeKey = "KeyStage4";
            public static readonly string PhonicsDataTypeKey = "Phonics";
            public static readonly string MtcDataTypeKey = "MTC";
        }

        internal class SearchDownloadDataTypeBuilder
        {
            private string _name;
            private string _value;
            private bool _canDownload = true;

            public static SearchDownloadDataTypeBuilder Create() => new SearchDownloadDataTypeBuilder();

            public SearchDownloadDataTypeBuilder WithName(string name)
            {
                _name = name;
                return this;
            }

            public SearchDownloadDataTypeBuilder WithValue(string value)
            {
                _value = value;
                return this;
            }

            public SearchDownloadDataTypeBuilder WithCannotDownload()
            {
                _canDownload = false;
                return this;
            }

            public SearchDownloadDataType Build() =>
                new SearchDownloadDataType()
                {
                    Name = _name,
                    Value = _value,
                    Disabled = false,   // default.
                    CanDownload = _canDownload
                };
        }

        #region expected administrator, LA & all ages download data types

        private static readonly List<SearchDownloadDataType> expectedAdminDataTypes =
            SearchDownloadDataTypeListBuilder.Create()
                .WithDefaultSearchDownloadDataTypeList()
                .Build();

        #endregion expected administrator, LA & all ages download data types

        #region expected LA download data types

        private static readonly List<SearchDownloadDataType> expectedLADataTypes =
            SearchDownloadDataTypeListBuilder.Create()
                .WithDefaultSearchDownloadDataTypeList()
                .Build();

        #endregion expected LA download data types

        #region expected MAT all ages download data types

        private static readonly List<SearchDownloadDataType> expectedMATAllAgesDataTypes =
           SearchDownloadDataTypeListBuilder.Create()
                .WithDefaultSearchDownloadDataTypeList()
                .Build();

        #endregion expected MAT all ages download data types

        #region 2-5 download data types

        private static readonly List<SearchDownloadDataType> expected2to5DataTypes =
            SearchDownloadDataTypeListBuilder.Create()
                .WithDefaultSearchDownloadDataTypeList()
                .WithCannotDownloadForDataTypes(
                    new List<string> {
                        SearchDownloadDataTypeListBuilder.KeyStage1DataTypeKey,
                        SearchDownloadDataTypeListBuilder.KeyStage2DataTypeKey,
                        SearchDownloadDataTypeListBuilder.KeyStage4DataTypeKey
                    })
                .Build();

        #endregion 2-5 download data types

        #region 2-11 download data types

        private static readonly List<SearchDownloadDataType> expected2to11DataTypes =
            SearchDownloadDataTypeListBuilder.Create()
                .WithDefaultSearchDownloadDataTypeList()
                .WithCannotDownloadForDataTypes(
                    new List<string> {
                        SearchDownloadDataTypeListBuilder.KeyStage4DataTypeKey })
                .Build();

        #endregion 2-11 download data types

        #region 2-25 download data types

        private static readonly List<SearchDownloadDataType> expected2to25DataTypes =
            SearchDownloadDataTypeListBuilder.Create()
                .WithDefaultSearchDownloadDataTypeList()
                .Build();

        #endregion 2-25 download data types

        #region 11-25 download data types

        private static readonly List<SearchDownloadDataType> expected11to25DataTypes =
            SearchDownloadDataTypeListBuilder.Create()
                .WithDefaultSearchDownloadDataTypeList()
                .WithCannotDownloadForDataTypes(
                    new List<string> {
                        SearchDownloadDataTypeListBuilder.EYFSPDataTypeKey,
                        SearchDownloadDataTypeListBuilder.PhonicsDataTypeKey
                    })
                .Build();

        #endregion 11-25 download data types

        #region 16-25 download data types

        private static readonly List<SearchDownloadDataType> expected16to25DataTypes =
            SearchDownloadDataTypeListBuilder.Create()
                .WithDefaultSearchDownloadDataTypeList()
                .WithCannotDownloadForDataTypes(
                    new List<string> {
                        SearchDownloadDataTypeListBuilder.EYFSPDataTypeKey,
                        SearchDownloadDataTypeListBuilder.KeyStage1DataTypeKey,
                        SearchDownloadDataTypeListBuilder.KeyStage2DataTypeKey,
                        SearchDownloadDataTypeListBuilder.PhonicsDataTypeKey,
                        SearchDownloadDataTypeListBuilder.MtcDataTypeKey
                    })
                .Build();

        #endregion 16-25 download data types

        #region 18-25 download data types

        private static readonly List<SearchDownloadDataType> expected18to25DataTypes =
            SearchDownloadDataTypeListBuilder.Create()
                .WithDefaultSearchDownloadDataTypeList()
                .WithCannotDownloadForDataTypes(
                    new List<string> {
                        SearchDownloadDataTypeListBuilder.EYFSPDataTypeKey,
                        SearchDownloadDataTypeListBuilder.KeyStage1DataTypeKey,
                        SearchDownloadDataTypeListBuilder.KeyStage2DataTypeKey,
                        SearchDownloadDataTypeListBuilder.KeyStage4DataTypeKey,
                        SearchDownloadDataTypeListBuilder.PhonicsDataTypeKey,
                        SearchDownloadDataTypeListBuilder.MtcDataTypeKey
                    })
                .Build();

        #endregion 18-25 download data types

        #region FE download data types

        private static readonly List<SearchDownloadDataType> expectedFEDataTypes =
            new List<SearchDownloadDataType>(){
                SearchDownloadDataTypeBuilder.Create()
                    .WithName("Pupil Premium")
                    .WithValue("PP")
                    .Build(),
                SearchDownloadDataTypeBuilder.Create()
                    .WithName("Special Educational Needs")
                    .WithValue("SEN")
                    .Build()
            };

        private static List<SearchDownloadDataType> expectedNotFEDataTypes = new List<SearchDownloadDataType>(){
                SearchDownloadDataTypeBuilder.Create()
                    .WithName("Pupil Premium")
                    .WithValue("PP")
                    .WithCannotDownload()
                    .Build(),
                SearchDownloadDataTypeBuilder.Create()
                    .WithName("Special Educational Needs")
                    .WithValue("SEN")
                    .WithCannotDownload()
                    .Build()
            };

        #endregion FE download data types

        internal class DownloadDataTypeTestDataBuilder
        {
            private int _lowerAgeBoundary;
            private int _upperAgeBoundary;
            private bool _isAdmin;
            private bool _isLocalAuth;
            private bool _isMAT;
            private bool _isDfe;

            public static DownloadDataTypeTestDataBuilder Create() => new DownloadDataTypeTestDataBuilder();

            public DownloadDataTypeTestDataBuilder WithLowerAgeBoundary(int lowerAgeBoundary)
            {
                _lowerAgeBoundary = lowerAgeBoundary;
                return this;
            }

            public DownloadDataTypeTestDataBuilder WithUpperAgeBoundary(int upperAgeBoundary)
            {
                _upperAgeBoundary = upperAgeBoundary;
                return this;
            }

            public DownloadDataTypeTestDataBuilder WithIsAdmin(bool isAdmin)
            {
                _isAdmin = isAdmin;
                return this;
            }

            public DownloadDataTypeTestDataBuilder WithIsLocalAuthority(bool isLocalAuth)
            {
                _isLocalAuth = isLocalAuth;
                return this;
            }

            public DownloadDataTypeTestDataBuilder WithIsMAT(bool isMAT)
            {
                _isMAT = isMAT;
                return this;
            }

            public DownloadDataTypeTestDataBuilder WithIsDfe(bool isDfe)
            {
                _isDfe = isDfe;
                return this;
            }

            public DownloadDataTypeTestData BuildWithExpectedDataTypes(
                List<SearchDownloadDataType> searchDownloadDataTypes) =>
                    new DownloadDataTypeTestData(
                        lowAge: _lowerAgeBoundary,
                        highAge: _upperAgeBoundary,
                        isAdmin: _isAdmin,
                        isLA: _isLocalAuth,
                        isMAT: _isMAT,
                        isDfe: _isDfe)
                    {
                        ExpectedDataTypes = searchDownloadDataTypes
                    };
        }

        public static IEnumerable<object[]> GetSearchDownloadDataTypeData()
        {
            var allData = new List<object[]>
            {
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(2)
                    .WithUpperAgeBoundary(20)
                    .WithIsAdmin(true)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(false)
                    .BuildWithExpectedDataTypes(expectedAdminDataTypes)
               },
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(2)
                    .WithUpperAgeBoundary(20)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(true)
                    .WithIsMAT(false)
                    .BuildWithExpectedDataTypes(expectedLADataTypes)
               },
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(0)
                    .WithUpperAgeBoundary(0)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(true)
                    .BuildWithExpectedDataTypes(expectedMATAllAgesDataTypes)
               },
               //MAT user with age range
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(2)
                    .WithUpperAgeBoundary(5)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(true)
                    .BuildWithExpectedDataTypes(expected2to5DataTypes)
               },
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(2)
                    .WithUpperAgeBoundary(5)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(false)
                    .BuildWithExpectedDataTypes(expected2to5DataTypes)
               },
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(2)
                    .WithUpperAgeBoundary(11)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(false)
                    .BuildWithExpectedDataTypes(expected2to11DataTypes)
               },
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(2)
                    .WithUpperAgeBoundary(25)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(false)
                    .BuildWithExpectedDataTypes(expected2to25DataTypes)
               },
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(11)
                    .WithUpperAgeBoundary(25)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(false)
                    .BuildWithExpectedDataTypes(expected11to25DataTypes)
               },
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(16)
                    .WithUpperAgeBoundary(25)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(false)
                    .BuildWithExpectedDataTypes(expected16to25DataTypes)
               },
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(18)
                    .WithUpperAgeBoundary(25)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(false)
                    .BuildWithExpectedDataTypes(expected18to25DataTypes)
               }
            };

            return allData;
        }

        public static IEnumerable<object[]> GetFESearchDownloadDataTypeData()
        {
            var allData = new List<object[]>
            {
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(2)
                    .WithUpperAgeBoundary(13)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(false)
                    .WithIsDfe(false)
                    .BuildWithExpectedDataTypes(expectedNotFEDataTypes)
               },
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(14)
                    .WithUpperAgeBoundary(25)
                    .WithIsAdmin(false)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(false)
                    .WithIsDfe(false)
                    .BuildWithExpectedDataTypes(expectedFEDataTypes)
               },
               new object[] {
                   DownloadDataTypeTestDataBuilder.Create()
                    .WithLowerAgeBoundary(2)
                    .WithUpperAgeBoundary(12)
                    .WithIsAdmin(true)
                    .WithIsLocalAuthority(false)
                    .WithIsMAT(false)
                    .WithIsDfe(false)
                    .BuildWithExpectedDataTypes(expectedFEDataTypes)
               }/*,
               new object[]
               {
                   DownloadDataTypeTestDataBuilder.Create()
                       .WithIsAdmin(false)
                       .WithIsLocalAuthority(false)
                       .WithIsMAT(false)
                       .WithIsDfe(true)
                       .BuildWithExpectedDataTypes(expectedFEDataTypes)
               }*/
            };

            return allData;
        }

        public class DownloadDataTypeTestData
        {
            public int LowAge { get; set; }
            public int HighAge { get; set; }
            public bool IsAdmin { get; set; }
            public bool IsLA { get; set; }
            public bool IsDfe { get; set; }
            public ClaimsPrincipal User { get; set; }
            public Controller Controller { get; set; }
            public List<SearchDownloadDataType> ExpectedDataTypes { get; set; } = new List<SearchDownloadDataType>();

            public DownloadDataTypeTestData(int lowAge, int highAge, bool isAdmin, bool isLA, bool isMAT, bool isDfe)
            {
                LowAge = lowAge;
                HighAge = highAge;
                IsAdmin = isAdmin;
                IsLA = isLA;
                IsDfe = isDfe;

                var role = isAdmin switch
                {
                    true => Role.Admin,
                    false => Role.Approver
                };

                var organisationId = isLA switch
                {
                    true => OrganisationCategory.LocalAuthority,
                    false => isMAT ? OrganisationCategory.MultiAcademyTrust : OrganisationCategory.Establishment
                };

                var user = new UserClaimsPrincipalFake().GetSpecificUserClaimsPrincipal(
                    organisationId,
                    EstablishmentType.CommunitySchool, // irrelevant for this test..
                    role,
                    lowAge,
                    highAge);

                User = user;
            }
        }
    }
}