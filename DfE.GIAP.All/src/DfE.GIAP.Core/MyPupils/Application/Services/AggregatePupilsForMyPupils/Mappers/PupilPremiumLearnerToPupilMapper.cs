using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mappers;
internal sealed class PupilPremiumLearnerToPupilMapper : IMapper<PupilPremiumLearner, Pupil>
{
    public Pupil Map(PupilPremiumLearner input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new Pupil(
            input.Identifier,
            PupilType.PupilPremium,
            new LearnerName(
                firstName: input.Name.FirstName,
                surname: input.Name.Surname),
            input.Characteristics.BirthDate,
            input.Characteristics.Sex,
            input.LocalAuthority);
    }
}
