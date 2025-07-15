using DfE.GIAP.Core.Pupil.Domain;
using DfE.GIAP.Core.User.Domain.Aggregate;

namespace DfE.GIAP.Core.User.Application.Repository;
public record User(UserIdentifier userId, IEnumerable<UniquePupilIdentifier> MyPupilsIds);

