using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.User.Application;
public record User(
    UserId UserId,
    IEnumerable<UniquePupilNumber> UniquePupilNumbers);
