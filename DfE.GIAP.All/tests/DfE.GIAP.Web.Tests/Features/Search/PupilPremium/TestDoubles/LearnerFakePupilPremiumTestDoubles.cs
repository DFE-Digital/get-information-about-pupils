using Bogus;
using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Learner;

namespace DfE.GIAP.Web.Tests.Features.Search.PupilPremium.TestDoubles;
public static class LearnerFakePupilPremiumTestDoubles
{
    public static Learner Fake()
    {
        Faker faker = new();

        LearnerName name = LearnerNameTestDouble.FakeName(faker);
        LearnerCharacteristics characteristics = LearnerCharacteristicsTestDouble.FakeCharacteristics(faker);
        LocalAuthorityCode localAuthority = LocalAuthorityCodeTestDoubles.Stub();
        return new Learner()
        {
            Id = UniquePupilNumberTestDoubles.Generate().Value,
            Forename = name.FirstName,
            Middlenames = name.MiddleName,
            Surname = name.Surname,
            DOB = characteristics.BirthDate,
            Sex = characteristics.Sex.MapSexDescription(),
            LocalAuthority = localAuthority.Code.ToString()
        };
    }
}
