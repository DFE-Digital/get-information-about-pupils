using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class MapAuthorisationContextToPupilsAuthorisationContextMapper : IMapper<IAuthorisationContext, PupilAuthorisationContext>
{
    public PupilAuthorisationContext Map(IAuthorisationContext input)
    {
        ArgumentNullException.ThrowIfNull(input);
        AgeLimit ageRange = new(input.LowAge, input.HighAge);
        UserRole userRole = new(input.IsAdministrator);
        return new(ageRange, userRole);
    }
}
