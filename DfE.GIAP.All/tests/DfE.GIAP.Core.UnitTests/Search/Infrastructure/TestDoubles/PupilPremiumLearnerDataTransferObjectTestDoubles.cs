using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
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

        PupilPremiumLearnerDataTransferObject dto = new()
        {
            UPN = UniquePupilNumberTestDoubles.Generate().Value,
            Forename = faker.Name.FirstName(),
            Middlenames = faker.Name.FirstName(),
            Surname = faker.Name.LastName(),
            DOB = DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(10),
            LocalAuthority = LocalAuthorityCodeTestDoubles.Stub().Code.ToString(),
            Sex = faker.PickRandom(Sex.Male, Sex.Female, Sex.Unknown).ToString()
        };

        return dto;
    }
}
