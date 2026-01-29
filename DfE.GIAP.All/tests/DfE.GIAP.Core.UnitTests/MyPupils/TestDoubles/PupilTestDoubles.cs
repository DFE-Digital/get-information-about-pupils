using Bogus;
using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;

public static class PupilTestDoubles
{
    public static List<Pupil> Generate(int count = 10, Func<Pupil, bool>? predicate = null)
    {
        Faker<Pupil> faker = CreateGenerator();
        List<Pupil> pupils = [];
        const int maxAttempts = 999;

        for (int i = 0; pupils.Count < count && i < maxAttempts; i++)
        {
            Pupil p = faker.Generate();
            if (predicate?.Invoke(p) != false)
            {
                pupils.Add(p);
            }
        }

        if (pupils.Count < count)
        {
            throw new ArgumentException($"Only generated {pupils.Count} of {count} requested Pupils after {maxAttempts} attempts.");
        }

        return pupils;
    }

    private static Faker<Pupil> CreateGenerator()
    {

        return new Faker<Pupil>()
            .CustomInstantiator((faker) =>
            {
                UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

                string forename = faker.Name.FirstName();
                string surname = faker.Name.LastName();
                PupilName name = new(forename, surname);

                DateTime dob = faker.Date.Past(yearsToGoBack: 18, DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(5));

                Sex sex = faker.PickRandom(Sex.Male, Sex.Female, Sex.Unknown);

                LocalAuthorityCode lac = new(faker.Random.Int(min: 1, max: 999));

                return new Pupil(
                    identifier: upn,
                    pupilType: PupilType.NationalPupilDatabase,
                    name: name,
                    dateOfBirth: dob,
                    sex: sex,
                    localAuthorityCode: lac);
            });
    }
}
