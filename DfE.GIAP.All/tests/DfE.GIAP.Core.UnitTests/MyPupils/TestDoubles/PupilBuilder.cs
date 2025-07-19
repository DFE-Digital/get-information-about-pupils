using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal sealed class PupilBuilder
{
    private readonly UniquePupilNumber _upn;
    private readonly PupilAuthorisationContext _authorisationContext;
    private PupilId? _id;
    private DateTime? _dateOfBirth;
    private PupilType? _pupilType;
    private string? _firstName;
    private string? _surname;

    private PupilBuilder(
        UniquePupilNumber upn,
        PupilAuthorisationContext context)
    {
        _upn = upn;
        _authorisationContext = context;
    }

    internal PupilBuilder WithPupilId(Guid guid)
    {
        _id = new(guid);
        return this;
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

    internal PupilBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    internal Pupil Build()
    {
        PupilName name = new(
            _firstName ?? "Default first name",
            _surname ?? "Default surname");

        return new Pupil(
            identifier: _id ?? new(Guid.NewGuid()),
            pupilType : _pupilType ?? PupilType.NationalPupilDatabase,
            name: name,
            uniquePupilNumber: _upn,
            dateOfBirth: _dateOfBirth,
            authorisationContext: _authorisationContext);
    }
    internal static PupilBuilder CreateBuilder(
        UniquePupilNumber upn,
        PupilAuthorisationContext authorisationContext)
    {
        return new PupilBuilder(upn, authorisationContext);
    }
}
