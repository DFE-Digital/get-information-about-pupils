using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.Users.Application;

public record User(
    UserId UserId,
    IEnumerable<UniquePupilNumber> UniquePupilNumbers,
    DateTime LatestNewsAccessedDateTime);
