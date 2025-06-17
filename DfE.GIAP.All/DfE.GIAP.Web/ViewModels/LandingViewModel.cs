using DfE.GIAP.Core.Contents.Application.Models;
using System.Diagnostics.CodeAnalysis;
namespace DfE.GIAP.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class LandingViewModel
{
    public Content LandingResponse { get; set; }
    public Content PlannedMaintenanceResponse { get; set; }
    public Content PublicationScheduleResponse { get; set; }
    public Content FAQResponse { get; set; }
}
