using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using static DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.TempAggregatePupilsForMyPupilsDomainService;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services;
internal record MappableLearnerWithAuthorisationContext(
    Learner Learner,
    PupilType PupilType,
    PupilAuthorisationContext AuthorisationContext);
