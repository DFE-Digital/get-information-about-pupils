using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;

namespace DfE.GIAP.Web.Features.Search.PupilPremium;

internal sealed class PupilPremiumLearnerToLearnerMapper : IMapper<PupilPremiumLearner, Learner>
{
    public Learner Map(PupilPremiumLearner input)
    {
        ArgumentNullException.ThrowIfNull(input);

        Learner learner = new()
        {
            Id = input.Identifier.Value.ToString(),
            LearnerNumber = input.Identifier.Value.ToString(),

            Forename = input.Name.FirstName,
            Middlenames = input.Name.MiddleNames,
            Surname = input.Name.Surname,

            Sex = input.Characteristics.Sex.ToString(),

            DOB = input.Characteristics.BirthDate,

            LocalAuthority = input.LocalAuthority.Code.ToString()            
        };

        return learner;
    }
}
