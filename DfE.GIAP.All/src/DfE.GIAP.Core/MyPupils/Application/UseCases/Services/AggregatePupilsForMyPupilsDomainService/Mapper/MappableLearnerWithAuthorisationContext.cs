using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Mapper;
internal record MappableLearnerWithAuthorisationContext(
    PupilId Pupilid,
    Learner Learner,
    PupilType PupilType,
    PupilAuthorisationContext AuthorisationContext);
