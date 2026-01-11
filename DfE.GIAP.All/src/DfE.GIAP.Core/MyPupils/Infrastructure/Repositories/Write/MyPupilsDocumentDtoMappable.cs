using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application.Models;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;
public record MyPupilsDocumentDtoMappable(UserId UserId, UniquePupilNumbers Upns);
