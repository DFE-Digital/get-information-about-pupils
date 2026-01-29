using Bogus;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Learner;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
public static class PupilPremiumLearnerDataTransferObjectTestDoubles
{
    public static PupilPremiumLearnerDataTransferObject Stub()
    {
        Faker faker = new();

        string gender = faker.PickRandom(Gender.Male, Gender.Female, Gender.Other).MapSexDescription();

        PupilPremiumLearnerDataTransferObject dto = new()
        {
            UPN = UniquePupilNumberTestDoubles.Generate().Value,
            Forename = faker.Name.FirstName(),
            Middlenames = faker.Name.FirstName(),
            Surname = faker.Name.LastName(),
            DOB = DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(10),
            LocalAuthority = LocalAuthorityCodeTestDoubles.Stub().Code.ToString(),
            Sex = gender,
            Gender = gender
        };

        return dto;
    }
}
