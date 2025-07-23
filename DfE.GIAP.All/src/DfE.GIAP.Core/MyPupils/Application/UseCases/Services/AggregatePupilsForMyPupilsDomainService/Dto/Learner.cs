using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Dto;

#nullable disable
// TODO can I delete this and just map out from AzureSearchIndexEntity to my outbound PupilMode?

/// <summary>
/// Data about learners that is stored in the cognitive search index. Returned based on searches.
/// </summary>
[ExcludeFromCodeCoverage]
public class Learner
{
    /// <summary>
    /// Cognitive search ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Learner number - may be UPN or ULN.
    /// </summary>
    public string UPN { get; set; }

    /// <summary>
    /// The local authority code for the learner.
    /// </summary>
    public string LocalAuthority { get; set; }

    /// <summary>
    /// Learners surname
    /// </summary>
    public string Surname { get; set; }

    /// <summary>
    /// Learners forename. May be multiple names
    /// </summary>
    public string Forename { get; set; }

    /// <summary>
    /// Learners sex.
    /// </summary>
    public string Sex { get; set; }

    /// <summary>
    /// Learners date of birth.
    /// </summary>
    public DateTime? Dob { get; set; }
}
