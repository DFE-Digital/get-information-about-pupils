using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers;

public record MyPupilsQueryRequestDto
{
    [FromQuery]
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be 1 or greater.")]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public string SortField { get; set; } = string.Empty;

    [FromQuery]
    [RegularExpression("^(?i)(asc|desc)$", ErrorMessage = "SortDirection must be 'asc' or 'desc'.")]
    public string SortDirection { get; set; } = string.Empty;

    public int PageSize => 20;
}
