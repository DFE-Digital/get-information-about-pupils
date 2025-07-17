using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
internal sealed class AuthorisationContextToMyPupilsAuthorisationContextMapper : IMapper<IAuthorisationContext, MyPupilsAuthorisationContext>
{
    public MyPupilsAuthorisationContext Map(IAuthorisationContext input)
    {
        AgeRange ageRange = new(input.LowAge, input.HighAge);
        UserRole userRole = new(input.IsAdministrator);
        return new(ageRange, userRole);
    }
}
