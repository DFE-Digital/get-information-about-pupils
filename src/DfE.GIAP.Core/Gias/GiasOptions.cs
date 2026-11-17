using System.ComponentModel.DataAnnotations;

namespace DfE.GIAP.Core.Gias;

public sealed class GiasOptions
{
    public const string SectionName = "Gias";

    [Required]
    public Uri BaseUrl { get; set; } = new("https://ea-edubase-api-prod.azurewebsites.net/edubase/downloads/public/");
}
