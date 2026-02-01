using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
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
            Middlenames = name.MiddleNames,
            Surname = name.Surname,
            DOB = characteristics.BirthDate,
            Sex = characteristics.Sex.ToString(),
            LocalAuthority = localAuthority.Code.ToString()
        };
    }
}
