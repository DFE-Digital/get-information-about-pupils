using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.Mappers.TestDoubles;

/// <summary>
/// Provides reusable scaffolds for creating view-model-layer <see cref="FurtherEducationLearner"/> objects.
/// Wraps <see cref="ApplicationModelLearnerTestDouble.Stub"/> for consistent and readable test setup.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ApplicationModelLearnersTestDouble
{
    /// <summary>
    /// Creates multiple view model learners from a collection of value tuples.
    /// </summary>
    /// <param name="learners">
    /// A collection of tuples containing learner attributes:
    /// (ULN, FirstName, Surname, BirthDate, Gender).
    /// </param>
    /// <returns>A list of scaffolded <see cref="FurtherEducationLearner"/> objects.</returns>
    public static List<FurtherEducationLearner> CreateLearnersStub(
        IEnumerable<(string Uln, string FirstName, string Surname, DateTime BirthDate, Gender Gender)> learners)
    {
        List<FurtherEducationLearner> result = [];
        foreach ((string uln, string firstName, string surname, DateTime birthDate, Gender gender) in learners)
        {
            result.Add(ApplicationModelLearnerTestDouble.Stub(uln, firstName, surname, birthDate, gender));
        }
        return result;
    }
}
