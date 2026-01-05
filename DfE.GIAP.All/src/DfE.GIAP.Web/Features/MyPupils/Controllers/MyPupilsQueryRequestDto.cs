using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers;

public class MyPupilsQueryRequestDto
{
    [FromQuery]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public string SortField { get; set; } = string.Empty;

    [FromQuery]
    public string SortDirection { get; set; } = string.Empty;

    public int PageSize => 20;
}
