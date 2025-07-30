﻿using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
internal sealed class MapMappableLearnerToPupilMapper : IMapper<MappableLearner, Pupil>
{
    public Pupil Map(MappableLearner input) =>
        new(
            identifier: input.uniquePupilNumber,
            pupilType: input.PupilType,
            name: new(input.SearchIndexDto.Forename, input.SearchIndexDto.Surname),
            dateOfBirth: input.SearchIndexDto.DOB,
            sex: new Sex(input.SearchIndexDto.Sex),
            localAuthorityCode: new LocalAuthorityCode(int.Parse(input.SearchIndexDto.LocalAuthority)));
}
