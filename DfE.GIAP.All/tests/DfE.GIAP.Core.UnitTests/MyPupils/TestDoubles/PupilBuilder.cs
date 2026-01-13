using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal sealed class PupilBuilder
{
    private readonly UniquePupilNumber _upn;
    private DateTime? _dateOfBirth;
    private PupilType? _pupilType;
    private Sex? _sex;
    private string? _firstName;
    private string? _surname;
    private LocalAuthorityCode? _localAuthorityCode;

    private PupilBuilder(
        UniquePupilNumber upn)
    {
        _upn = upn;
    }

    internal PupilBuilder WithPupilType(PupilType pupilType)
    {
        _pupilType = pupilType;
        return this;
    }

    internal PupilBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    internal PupilBuilder WithSurname(string surname)
    {
        _surname = surname;
        return this;
    }

    internal PupilBuilder WithDateOfBirth(DateTime? dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    internal PupilBuilder WithSex(Sex? sex)
    {
        _sex = sex;
        return this;
    }

    internal PupilBuilder WithLocalAuthorityCode(LocalAuthorityCode? localAuthorityCode)
    {
        _localAuthorityCode = localAuthorityCode;
        return this;
    }

    internal Pupil Build()
    {
        PupilName name = new(
            _firstName ?? "Default first name",
            _surname ?? "Default surname");

        return new Pupil(
            identifier: _upn,
            pupilType: _pupilType ?? PupilType.NationalPupilDatabase,
            name: name,
            dateOfBirth: _dateOfBirth,
            sex: _sex,
            localAuthorityCode: _localAuthorityCode);
    }
    internal static PupilBuilder CreateBuilder(
        UniquePupilNumber upn)
    {
        return new PupilBuilder(upn);
    }
}
