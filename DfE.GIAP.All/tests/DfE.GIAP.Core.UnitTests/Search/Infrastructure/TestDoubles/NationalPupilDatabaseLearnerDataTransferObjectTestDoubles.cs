using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Learner;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
public static class NationalPupilDatabaseLearnerDataTransferObjectTestDoubles
{
    public static NationalPupilDatabaseLearnerDataTransferObject Stub()
    {
        Faker faker = new();

        string gender = faker
            .PickRandom(Sex.Male, Sex.Female, Sex.Unknown)
            .ToString();

        NationalPupilDatabaseLearnerDataTransferObject dto = new()
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
