using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write.Mapper;
public record MyPupilsDocumentDtoMappable(UserId UserId, IEnumerable<UniquePupilNumber> Upns);
