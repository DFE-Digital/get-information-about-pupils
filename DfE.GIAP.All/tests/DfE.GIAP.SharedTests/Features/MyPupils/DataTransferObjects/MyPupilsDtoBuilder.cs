using Bogus;
using DfE.GIAP.Core.Common.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.Features.MyPupils.Application;

namespace DfE.GIAP.SharedTests.Features.MyPupils.DataTransferObjects;
public sealed class MyPupilDtoBuilder
{
    private const string DATE_OF_BIRTH_FORMAT = "yyyy-MM-dd";
    private static readonly Faker<MyPupilsModel> s_faker = MyPupilsModelTestDoubles.CreateGenerator();

    private UniquePupilNumber? _uniquePupilNumber;
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

    public MyPupilDtoBuilder WithUniquePupilNumber(UniquePupilNumber upn)
    {
        ArgumentNullException.ThrowIfNull(upn);
        _uniquePupilNumber = upn;
        return this;
    }

    public static MyPupilDtoBuilder Create() => new();

    public static MyPupilsModel BuildDefault() => new MyPupilDtoBuilder().Build();

    public MyPupilsModel Build()
    {
        MyPupilsModel defaulter = s_faker.Generate();

        return new()
        {
            UniquePupilNumber = _uniquePupilNumber?.Value ?? defaulter.UniquePupilNumber,
            Forename = _forename ?? defaulter.Forename,
            Surname = _surname ?? defaulter.Surname,
            DateOfBirth = (_dateOfBirth ?? DateTime.UnixEpoch).ToString(DATE_OF_BIRTH_FORMAT),
            Sex = _sex ?? defaulter.Sex,
            IsPupilPremium = _isPupilPremium ? true : false,
            LocalAuthorityCode = _localAuthorityCode ?? defaulter.LocalAuthorityCode
        };
    }
}
