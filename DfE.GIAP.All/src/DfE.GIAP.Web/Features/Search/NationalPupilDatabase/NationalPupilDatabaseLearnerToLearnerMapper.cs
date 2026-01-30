using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Domain.Search.Learner;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase;

internal sealed class NationalPupilDatabaseLearnerToLearnerMapper : IMapper<NationalPupilDatabaseLearner, Learner>
{
    public Learner Map(NationalPupilDatabaseLearner input)
    {
        ArgumentNullException.ThrowIfNull(input);

        Learner learner = new()
        {
            Id = input.Identifier.Value.ToString(),
            LearnerNumber = input.Identifier.Value.ToString(),

            Forename = input.Name.FirstName,
            Middlenames = input.Name.MiddleNames,
            Surname = input.Name.Surname,

            Sex = input.Characteristics.Sex.MapSexDescription(),

            DOB = input.Characteristics.BirthDate,

            LocalAuthority = input.LocalAuthority.Code.ToString()
        };

        return learner;
    }
}
