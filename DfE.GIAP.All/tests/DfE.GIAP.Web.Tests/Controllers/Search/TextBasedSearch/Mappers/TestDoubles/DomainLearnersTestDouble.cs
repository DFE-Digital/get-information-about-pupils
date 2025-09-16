﻿using DfE.GIAP.Domain.Search.Learner;
using static DfE.GIAP.Core.Search.Application.Models.Learner.LearnerCharacteristics;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;

/// <summary>
/// Provides deterministic scaffolds for creating domain-layer <see cref="Learner"/> objects.
/// Designed for unit tests that require explicit control over learner attributes.
/// </summary>
public static class DomainLearnersTestDouble
{
    /// <summary>
    /// Creates a list of <see cref="Learner"/> objects from a collection of value tuples.
    /// Enables bulk scaffolding for tests that validate multi-learner workflows.
    /// </summary>
    /// <param name="learners">
    /// A collection of tuples containing learner attributes:
    /// (Id, Forename, Surname, DOB, Gender).
    /// </param>
    /// <returns>A list of domain-layer <see cref="Learner"/> objects.</returns>
    public static List<Learner> CreateLearnersStub(
        IEnumerable<(string Id, string Forename, string Surname, DateTime DOB, Gender Gender)> learners)
    {
        List<Learner> result = [];
        foreach ((string id, string forename, string surname, DateTime dob, Gender gender) in learners)
        {
            result.Add(CreateLearnerStub(id, forename, surname, dob, gender));
        }
        return result;
    }

    /// <summary>
    /// Creates a single <see cref="Learner"/> instance using provided values.
    /// Useful for tests that require symbolic traceability across learner identity and demographics.
    /// </summary>
    /// <param name="id">Unique learner identifier (e.g., ULN).</param>
    /// <param name="forename">Learner's first name.</param>
    /// <param name="surname">Learner's last name.</param>
    /// <param name="dob">Date of birth for age-based filtering or diagnostics.</param>
    /// <param name="gender">Learner's gender, used for demographic assertions.</param>
    /// <returns>A fully populated <see cref="Learner"/> object.</returns>
    public static Learner CreateLearnerStub(
        string id,
        string forename,
        string surname,
        DateTime dob,
        Gender gender) =>
        new()
        {
            Id = id,
            Forename = forename,
            Surname = surname,
            DOB = dob,
            Gender = gender.ToString()
        };
}
