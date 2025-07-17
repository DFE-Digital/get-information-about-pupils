using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository;
public record User(UserId UserId, IEnumerable<UniquePupilNumber> PupilIdentifiers);

