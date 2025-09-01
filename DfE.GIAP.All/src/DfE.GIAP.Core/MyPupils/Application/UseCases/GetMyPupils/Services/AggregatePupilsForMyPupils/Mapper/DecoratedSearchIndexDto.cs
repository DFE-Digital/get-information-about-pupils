using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Mapper;
public sealed record DecoratedSearchIndexDto(
    AzureIndexEntity SearchIndexDto,
    PupilType PupilType);
