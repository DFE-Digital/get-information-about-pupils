using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Adaptors;
internal sealed class NationalPupilDatabaseLearnerToPupilMapper : IMapper<NationalPupilDatabaseLearner, Pupil>
{
    public Pupil Map(NationalPupilDatabaseLearner input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new Pupil(
            input.Identifier,
            PupilType.NationalPupilDatabase,
            new LearnerName(
                firstName: input.Name.FirstName,
                surname: input.Name.Surname),
            input.Characteristics.BirthDate,
            input.Characteristics.Sex,
            input.LocalAuthority);
    }
}
