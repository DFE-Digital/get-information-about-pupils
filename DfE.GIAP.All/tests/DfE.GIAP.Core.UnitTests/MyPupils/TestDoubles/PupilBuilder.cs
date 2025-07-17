using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal sealed class PupilBuilder
{
    private readonly UniquePupilNumber _upn;
    private readonly MyPupilsAuthorisationContext _authorisationContext;
    private PupilId? _id;
    private DateTime? _dateOfBirth;

    private PupilBuilder(
        UniquePupilNumber upn,
        MyPupilsAuthorisationContext context)
    {
        _upn = upn;
        _authorisationContext = context;
    }

    internal PupilBuilder WithPupilId(Guid guid)
    {
        _id = new(guid);
        return this;
    }

    internal PupilBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    internal Pupil Build()
        => new Pupil(
            _id ?? new(Guid.NewGuid()),
            _upn,
            _dateOfBirth,
            _authorisationContext);
    internal static PupilBuilder CreateBuilder(
        UniquePupilNumber upn,
        MyPupilsAuthorisationContext authorisationContext)
    {
        return new PupilBuilder(upn, authorisationContext);
    }
}
