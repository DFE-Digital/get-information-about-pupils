using System.Security.Claims;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Features.Search.LegacyModels;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Tests.Helpers;

public sealed class SearchDownloadHelperTests
{

    [Fact]
    public void DownloadFile_returns_FileContentResult_for_non_zip()
    {
        // Arrange
        ReturnFile downloadFile = new()
        {
            FileType = "plain",
            Bytes = [],
            FileName = "test.txt"
        };

        // Act
        IActionResult result = SearchDownloadHelper.DownloadFile(downloadFile);

        // Assert
        Assert.IsType<FileContentResult>(result);
    }

    internal class SearchDownloadDataTypeListBuilder
    {
        private Dictionary<string, SearchDownloadDataType>? _searchDownloadDataTypeDictionary;

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

        public List<SearchDownloadDataType> Build() => _searchDownloadDataTypeDictionary?.Values.ToList() ?? [];

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
        private string? _name;
        private string? _value;
        private bool _canDownload = true;

        public static SearchDownloadDataTypeBuilder Create() => new();

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
            new()
            {
                Name = _name,
                Value = _value,
                Disabled = false,   // default.
                CanDownload = _canDownload
            };
    }


    private static readonly List<SearchDownloadDataType> _expectedAdminDataTypes =
        SearchDownloadDataTypeListBuilder.Create()
            .WithDefaultSearchDownloadDataTypeList()
            .Build();


    private static readonly List<SearchDownloadDataType> _expectedLADataTypes =
        SearchDownloadDataTypeListBuilder.Create()
            .WithDefaultSearchDownloadDataTypeList()
            .Build();


    private static readonly List<SearchDownloadDataType> _expectedMATAllAgesDataTypes =
       SearchDownloadDataTypeListBuilder.Create()
            .WithDefaultSearchDownloadDataTypeList()
            .Build();


    private static readonly List<SearchDownloadDataType> _expected2to5DataTypes =
        SearchDownloadDataTypeListBuilder.Create()
            .WithDefaultSearchDownloadDataTypeList()
            .WithCannotDownloadForDataTypes(
                [
                    SearchDownloadDataTypeListBuilder.KeyStage1DataTypeKey,
                    SearchDownloadDataTypeListBuilder.KeyStage2DataTypeKey,
                    SearchDownloadDataTypeListBuilder.KeyStage4DataTypeKey
                ])
            .Build();

    private static readonly List<SearchDownloadDataType> _expected2to11DataTypes =
        SearchDownloadDataTypeListBuilder.Create()
            .WithDefaultSearchDownloadDataTypeList()
            .WithCannotDownloadForDataTypes(
                [
                    SearchDownloadDataTypeListBuilder.KeyStage4DataTypeKey ])
            .Build();

    private static readonly List<SearchDownloadDataType> _expected2to25DataTypes =
        SearchDownloadDataTypeListBuilder.Create()
            .WithDefaultSearchDownloadDataTypeList()
            .Build();

    private static readonly List<SearchDownloadDataType> _expected11to25DataTypes =
        SearchDownloadDataTypeListBuilder.Create()
            .WithDefaultSearchDownloadDataTypeList()
            .WithCannotDownloadForDataTypes(
                [
                    SearchDownloadDataTypeListBuilder.EYFSPDataTypeKey,
                    SearchDownloadDataTypeListBuilder.PhonicsDataTypeKey
                ])
            .Build();

    private static readonly List<SearchDownloadDataType> _expected16to25DataTypes =
        SearchDownloadDataTypeListBuilder.Create()
            .WithDefaultSearchDownloadDataTypeList()
            .WithCannotDownloadForDataTypes(
                [
                    SearchDownloadDataTypeListBuilder.EYFSPDataTypeKey,
                    SearchDownloadDataTypeListBuilder.KeyStage1DataTypeKey,
                    SearchDownloadDataTypeListBuilder.KeyStage2DataTypeKey,
                    SearchDownloadDataTypeListBuilder.PhonicsDataTypeKey,
                    SearchDownloadDataTypeListBuilder.MtcDataTypeKey
                ])
            .Build();

    private static readonly List<SearchDownloadDataType> _expected18to25DataTypes =
        SearchDownloadDataTypeListBuilder.Create()
            .WithDefaultSearchDownloadDataTypeList()
            .WithCannotDownloadForDataTypes(
                [
                    SearchDownloadDataTypeListBuilder.EYFSPDataTypeKey,
                    SearchDownloadDataTypeListBuilder.KeyStage1DataTypeKey,
                    SearchDownloadDataTypeListBuilder.KeyStage2DataTypeKey,
                    SearchDownloadDataTypeListBuilder.KeyStage4DataTypeKey,
                    SearchDownloadDataTypeListBuilder.PhonicsDataTypeKey,
                    SearchDownloadDataTypeListBuilder.MtcDataTypeKey
                ])
            .Build();


    private static readonly List<SearchDownloadDataType> _expectedFEDataTypes =
        [
            SearchDownloadDataTypeBuilder.Create()
                .WithName("Pupil Premium")
                .WithValue("PP")
                .Build(),
            SearchDownloadDataTypeBuilder.Create()
                .WithName("Special Educational Needs")
                .WithValue("SEN")
                .Build()
        ];

    private static List<SearchDownloadDataType> _expectedNotFEDataTypes = [
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
        ];

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
        List<object[]> allData =
        [
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(2)
                .WithUpperAgeBoundary(20)
                .WithIsAdmin(true)
                .WithIsLocalAuthority(false)
                .WithIsMAT(false)
                .BuildWithExpectedDataTypes(_expectedAdminDataTypes)
           ],
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(2)
                .WithUpperAgeBoundary(20)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(true)
                .WithIsMAT(false)
                .BuildWithExpectedDataTypes(_expectedLADataTypes)
           ],
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(0)
                .WithUpperAgeBoundary(0)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(false)
                .WithIsMAT(true)
                .BuildWithExpectedDataTypes(_expectedMATAllAgesDataTypes)
           ],
           //MAT user with age range
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(2)
                .WithUpperAgeBoundary(5)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(false)
                .WithIsMAT(true)
                .BuildWithExpectedDataTypes(_expected2to5DataTypes)
           ],
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(2)
                .WithUpperAgeBoundary(5)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(false)
                .WithIsMAT(false)
                .BuildWithExpectedDataTypes(_expected2to5DataTypes)
           ],
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(2)
                .WithUpperAgeBoundary(11)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(false)
                .WithIsMAT(false)
                .BuildWithExpectedDataTypes(_expected2to11DataTypes)
           ],
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(2)
                .WithUpperAgeBoundary(25)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(false)
                .WithIsMAT(false)
                .BuildWithExpectedDataTypes(_expected2to25DataTypes)
           ],
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(11)
                .WithUpperAgeBoundary(25)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(false)
                .WithIsMAT(false)
                .BuildWithExpectedDataTypes(_expected11to25DataTypes)
           ],
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(16)
                .WithUpperAgeBoundary(25)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(false)
                .WithIsMAT(false)
                .BuildWithExpectedDataTypes(_expected16to25DataTypes)
           ],
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(18)
                .WithUpperAgeBoundary(25)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(false)
                .WithIsMAT(false)
                .BuildWithExpectedDataTypes(_expected18to25DataTypes)
           ]
        ];

        return allData;
    }

    public static IEnumerable<object[]> GetFESearchDownloadDataTypeData()
    {
        List<object[]> allData =
        [
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(2)
                .WithUpperAgeBoundary(13)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(false)
                .WithIsMAT(false)
                .WithIsDfe(false)
                .BuildWithExpectedDataTypes(_expectedNotFEDataTypes)
           ],
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(14)
                .WithUpperAgeBoundary(25)
                .WithIsAdmin(false)
                .WithIsLocalAuthority(false)
                .WithIsMAT(false)
                .WithIsDfe(false)
                .BuildWithExpectedDataTypes(_expectedFEDataTypes)
           ],
           [
               DownloadDataTypeTestDataBuilder.Create()
                .WithLowerAgeBoundary(2)
                .WithUpperAgeBoundary(12)
                .WithIsAdmin(true)
                .WithIsLocalAuthority(false)
                .WithIsMAT(false)
                .WithIsDfe(false)
                .BuildWithExpectedDataTypes(_expectedFEDataTypes)
           ]/*,
           new object[]
           {
               DownloadDataTypeTestDataBuilder.Create()
                   .WithIsAdmin(false)
                   .WithIsLocalAuthority(false)
                   .WithIsMAT(false)
                   .WithIsDfe(true)
                   .BuildWithExpectedDataTypes(expectedFEDataTypes)
           }*/
        ];

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
        public List<SearchDownloadDataType> ExpectedDataTypes { get; set; } = [];

        public DownloadDataTypeTestData(int lowAge, int highAge, bool isAdmin, bool isLA, bool isMAT, bool isDfe)
        {
            LowAge = lowAge;
            HighAge = highAge;
            IsAdmin = isAdmin;
            IsLA = isLA;
            IsDfe = isDfe;

            string role = isAdmin switch
            {
                true => AuthRoles.Admin,
                false => AuthRoles.Approver
            };

            string organisationId = isLA switch
            {
                true => DsiKeys.OrganisationCategory.LocalAuthority,
                false => isMAT ? DsiKeys.OrganisationCategory.MultiAcademyTrust : DsiKeys.OrganisationCategory.Establishment
            };

            ClaimsPrincipal user = UserClaimsPrincipalFake.GetSpecificUserClaimsPrincipal(
                organisationId,
                DsiKeys.EstablishmentType.CommunitySchool, // irrelevant for this test..
                role,
                lowAge,
                highAge);

            User = user;
        }
    }
}
