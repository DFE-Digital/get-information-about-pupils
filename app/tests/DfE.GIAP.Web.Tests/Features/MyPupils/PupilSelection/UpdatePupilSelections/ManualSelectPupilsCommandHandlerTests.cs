using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.Handlers;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PupilSelection.UpdatePupilSelections;

public sealed class ManualSelectPupilsCommandHandlerTests
{
    [Fact]
    public async Task SelectAll_Ignores_Unchecked_Pupils_On_Current_Page()
    {
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionState.CreateDefault();
        MyPupilsPupilSelectionsRequestDto request = new()
        {
            SelectAll = true,
            CurrentPupils = ["current-page-pupil"]
        };

        await new SelectAllPupilsCommandHandler().HandleAsync(new(request, state));
        await new ManualSelectPupilsCommandHandler().HandleAsync(new(request, state));

        Assert.True(state.IsPupilSelected("current-page-pupil"));
        Assert.True(state.IsPupilSelected("other-page-pupil"));
    }

    [Fact]
    public async Task DeselectAll_Ignores_Checked_Pupils_On_Current_Page()
    {
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionState.CreateDefault();
        state.SelectAll();
        MyPupilsPupilSelectionsRequestDto request = new()
        {
            SelectAll = false,
            CurrentPupils = ["current-page-pupil"],
            SelectedPupils = ["current-page-pupil"]
        };

        await new DeselectAllPupilsCommandHandler().HandleAsync(new(request, state));
        await new ManualSelectPupilsCommandHandler().HandleAsync(new(request, state));

        Assert.False(state.IsPupilSelected("current-page-pupil"));
        Assert.False(state.IsPupilSelected("other-page-pupil"));
    }

    [Fact]
    public async Task ManualSelection_Updates_Current_Page()
    {
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionState.CreateDefault();
        state.Select("unchecked-pupil");
        MyPupilsPupilSelectionsRequestDto request = new()
        {
            CurrentPupils = ["checked-pupil", "unchecked-pupil"],
            SelectedPupils = ["checked-pupil"]
        };

        await new ManualSelectPupilsCommandHandler().HandleAsync(new(request, state));

        Assert.True(state.IsPupilSelected("checked-pupil"));
        Assert.False(state.IsPupilSelected("unchecked-pupil"));
    }
}
