using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles.Learner;
public static class LearnerCharacteristicsTestDouble
{
    /// <summary>
    /// Generates a fake <see cref="LearnerCharacteristics"/> object with a randomized birth date
    /// and gender selection. Supports testing of demographic filters and facet mapping.
    /// </summary>
    public static LearnerCharacteristics FakeCharacteristics(Faker faker) =>
        new(
            birthDate: faker.Date.PastOffset(yearsToGoBack: 18).Date,
            gender: faker.PickRandom(Gender.Male, Gender.Female, Gender.Other));

}
