using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Core;
internal sealed class MapAuthorisationContextToMyPupilsAuthorisationContextMapper : IMapper<IAuthorisationContext, PupilAuthorisationContext>
{
    public PupilAuthorisationContext Map(IAuthorisationContext input)
    {
        ArgumentNullException.ThrowIfNull(input);
        AgeLimit ageRange = new(input.LowAge, input.HighAge);
        UserRole userRole = new(input.IsAdministrator);
        return new(ageRange, userRole);
    }
}
