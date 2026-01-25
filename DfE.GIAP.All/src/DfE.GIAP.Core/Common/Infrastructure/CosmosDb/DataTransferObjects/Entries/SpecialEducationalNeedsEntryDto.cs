using Newtonsoft.Json;

namespace DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects.Entries;

public class SpecialEducationalNeedsEntryDto
{
    [JsonProperty("NCYear")]
    public string? NationalCurriculumYear { get; set; }

    [JsonProperty("SEN_PROVISION")]
    public string? Provision { get; set; }

    [JsonProperty("ACAD_YEAR")]
    public string? AcademicYear { get; set; }
}
