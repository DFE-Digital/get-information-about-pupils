using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.User.Application.Repository;
public record User(UserId UserId, IEnumerable<UniquePupilNumber> PupilIdentifiers);

