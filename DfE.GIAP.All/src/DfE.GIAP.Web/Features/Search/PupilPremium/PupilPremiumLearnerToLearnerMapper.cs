using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Domain.Search.Learner;

namespace DfE.GIAP.Web.Features.Search.PupilPremium;

public class PupilPremiumLearnerToLearnerMapper : IMapper<PupilPremiumLearner, Learner>
{
    public Learner Map(PupilPremiumLearner input)
    {
        ArgumentNullException.ThrowIfNull(input);

        Learner learner = new()
        {
            Id = input.Identifier.Value.ToString(),
            LearnerNumber = input.Identifier.Value.ToString(),

            Forename = input.Name.FirstName,
            Middlenames = input.Name.MiddleName,
            Surname = input.Name.Surname,

            Sex = input.Characteristics.Sex.MapSexDescription(),

            DOB = input.Characteristics.BirthDate,

            LocalAuthority = input.LocalAuthority.Code.ToString()            
        };

        return learner;
    }
}
