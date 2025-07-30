﻿using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
internal sealed class MapDecoratedSearchIndexDtoToPupilMapper : IMapper<DecoratedSearchIndexDto, Pupil>
{
    public Pupil Map(DecoratedSearchIndexDto input) =>
        new(
            identifier: new UniquePupilNumber(input.SearchIndexDto.UPN),
            pupilType: input.PupilType,
            name: new(input.SearchIndexDto.Forename, input.SearchIndexDto.Surname),
            dateOfBirth: input.SearchIndexDto.DOB,
            sex: new Sex(input.SearchIndexDto.Sex),
            localAuthorityCode: new LocalAuthorityCode(int.Parse(input.SearchIndexDto.LocalAuthority)));
}
