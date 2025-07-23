using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using static DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.TempAggregatePupilsForMyPupilsDomainService;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService;
internal record MappableLearnerWithAuthorisationContext(
    Learner Learner,
    PupilType PupilType,
    PupilAuthorisationContext AuthorisationContext);
