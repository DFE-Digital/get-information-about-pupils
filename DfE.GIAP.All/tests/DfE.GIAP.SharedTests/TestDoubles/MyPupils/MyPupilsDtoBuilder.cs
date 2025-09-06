using Bogus;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;

namespace DfE.GIAP.SharedTests.TestDoubles.MyPupils;
public sealed class MyPupilDtoBuilder
{
    private const string DATE_OF_BIRTH_FORMAT = "yyyy-MM-dd";
    private static readonly Faker<MyPupilDto> s_faker = MyPupilDtosTestDoubles.CreateGenerator();

    private string? _uniquePupilNumber;
    private string? _forename;
    private string? _surname;
    private DateTime? _dateOfBirth;
    private string? _sex;
    private bool _isPupilPremium;
    private int? _localAuthorityCode;

    private MyPupilDtoBuilder()
    {
        _uniquePupilNumber = null;
        _forename = null;
        _surname = null;
        _dateOfBirth = null;
        _sex = null;
        _isPupilPremium = false;
        _localAuthorityCode = null;
    }

    public MyPupilDtoBuilder WithPupilPremium(bool isPupilPremium)
    {
        _isPupilPremium = isPupilPremium;
        return this;
    }

    public static MyPupilDtoBuilder Create() => new();
    public MyPupilDto Build()
    {
        MyPupilDto defaulter = s_faker.Generate();

        return new()
        {
            UniquePupilNumber = _uniquePupilNumber ?? defaulter.UniquePupilNumber,
            Forename = _forename ?? defaulter.Forename,
            Surname = _surname ?? defaulter.Surname,
            DateOfBirth = (_dateOfBirth ?? DateTime.UnixEpoch).ToString(DATE_OF_BIRTH_FORMAT),
            Sex = _sex ?? defaulter.Sex,
            IsPupilPremium = _isPupilPremium ? true : false,
            LocalAuthorityCode = _localAuthorityCode ?? defaulter.LocalAuthorityCode
        };
    }
}
