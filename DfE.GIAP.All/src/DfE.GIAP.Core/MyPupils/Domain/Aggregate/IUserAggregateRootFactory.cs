using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;

namespace DfE.GIAP.Core.MyPupils.Domain.Aggregate;
internal interface IUserAggregateRootFactory
{
    Task<UserAggregateRoot> CreateAsync(
        IAuthorisationContext authorisationContext,
        PupilQuery pupilQuery);
}
