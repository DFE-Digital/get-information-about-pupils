using System.Diagnostics.CodeAnalysis;
using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.TestDoubles;

[ExcludeFromCodeCoverage]
public static class FurtherEducationLearnerTestDouble
{
    public static FurtherEducationLearner Stub(
        string uniqueLearnerNumber,
        string firstname,
        string surname,
        DateTime birthDate,
        Sex sex)
    {

        FurtherEducationUniqueLearnerIdentifier learnerIdentifier = new(uniqueLearnerNumber);
        LearnerName learnerName = new(firstname, surname);
        LearnerCharacteristics learnerCharacteristics = new(birthDate, sex);

        // Compose and return the full Learner object
        return new(learnerIdentifier, learnerName, learnerCharacteristics);
    }

    public static FurtherEducationLearner Stub()
    {
        Faker faker = new();

        return Stub(
            FurtherEducationUniqueLearnerNumberIdentifierTestDoubles.CreateUniqueLearnerNumber(faker),
            faker.Name.FirstName(),
            faker.Name.LastName(),
            DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(13),
            Sex.Male);
    }
}
