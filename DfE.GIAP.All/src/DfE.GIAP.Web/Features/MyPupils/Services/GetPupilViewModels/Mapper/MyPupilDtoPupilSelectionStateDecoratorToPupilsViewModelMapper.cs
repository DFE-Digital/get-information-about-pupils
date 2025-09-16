﻿using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.Mapper;

internal sealed class MyPupilDtoPupilSelectionStateDecoratorToPupilsViewModelMapper : IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>
{
    public PupilsViewModel Map(MyPupilsDtoSelectionStateDecorator input)
    {
        ArgumentNullException.ThrowIfNull(input);

        IEnumerable<PupilViewModel> pupils = input.PupilDtos.Values.Select((dto) =>
        {
            PupilViewModel viewModel = new()
            {
                UniquePupilNumber = dto.UniquePupilNumber,
                Forename = dto.Forename,
                Surname = dto.Surname,
                DateOfBirth = dto.DateOfBirth,
                PupilPremiumLabel = dto.IsPupilPremium ? "Yes" : "No",
                Sex = dto.Sex,
                LocalAuthorityCode = dto.LocalAuthorityCode.ToString(),
                IsSelected = input.SelectionState.IsPupilSelected(dto.UniquePupilNumber)
            };
            return viewModel;
        });

        return PupilsViewModel.Create(pupils);
    }
}
