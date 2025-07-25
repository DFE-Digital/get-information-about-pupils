using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal static class PupilAuthorisationContextTestDoubles
{
    internal static PupilAuthorisationContext Default()
        => Generate(
            low: 5,
            high: 18,
            isAdministrator: false);

    internal static PupilAuthorisationContext GenerateWithDefaultAgeLimits()
        => Generate(
            low: 0,
            high: 0,
            isAdministrator: false);

    internal static PupilAuthorisationContext GenerateAsAdminisrativeUser()
        => Generate(
            low: 5,
            high: 6,
            isAdministrator: true);

    internal static PupilAuthorisationContext Generate(
        int low,
        int high,
        bool isAdministrator)
    {
        UserRole userRole = new(isAdministrator);

        return new PupilAuthorisationContext(
            new AgeLimit(low, high),
            userRole);
    }
}
