namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// What's done to a column's values when a data file is anonymised.
/// </summary>
public enum ColumnTransform
{
    /// <summary>
    /// Left as it is: the column isn't personal data.
    /// </summary>
    Keep,

    /// <summary>
    /// A PupilMatchingRef, permuted into a reserved range of fake values.
    /// </summary>
    Pmr,

    /// <summary>
    /// A UPN, permuted, with a valid check letter.
    /// </summary>
    Upn,

    /// <summary>
    /// A ULN, permuted among the valid ULNs.
    /// </summary>
    Uln,

    /// <summary>
    /// A number, e.g. a KS2 candidate number, permuted among the numbers of the same length.
    /// </summary>
    CandidateNumber,

    /// <summary>
    /// A GUID, permuted.
    /// </summary>
    Guid,

    Forename,

    Surname,

    MiddleNames,

    /// <summary>
    /// Moved to another day in the same month and year.
    /// </summary>
    Dob,

    /// <summary>
    /// Cut to its outward code, e.g. <c>SW1A 1AA</c> to <c>SW1A</c>.
    /// </summary>
    Postcode,

    /// <summary>
    /// Blanked.
    /// </summary>
    Address,

    /// <summary>
    /// Blanked.
    /// </summary>
    Blank
}
