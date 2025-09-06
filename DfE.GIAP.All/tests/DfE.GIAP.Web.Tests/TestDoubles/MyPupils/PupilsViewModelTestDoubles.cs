﻿using Bogus;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel;

namespace DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
internal static class PupilsViewModelTestDoubles
{
    internal static PupilsViewModel Generate(int count)
    {
        return PupilsViewModel.Create(
            pupils: GeneratePupils(count));
    }

    private static List<PupilViewModel> GeneratePupils(int count)
    {
        return CreateGenerator().Generate(count);
    }

    internal static Faker<PupilViewModel> CreateGenerator()
    {
        Faker<PupilViewModel> faker = new();
        faker.StrictMode(true);
        faker.RuleFor(t => t.UniquePupilNumber, (f) => UniquePupilNumberTestDoubles.Generate().Value);
        faker.RuleFor(t => t.DateOfBirth, (f)
            => new DateOfBirth(
                DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(f.Random.Number(5, 18))).ToString());
        faker.RuleFor(t => t.Forename, (f) => f.Name.FirstName());
        faker.RuleFor(t => t.Surname, (f) => f.Name.LastName());
        faker.RuleFor(t => t.LocalAuthorityCode, (f) => f.Random.Number(0, 999).ToString());
        faker.RuleFor(t => t.Sex, (f) => f.PickRandom(Sex.Female.ToString(), Sex.Male.ToString()));
        faker.RuleFor(t => t.PupilPremiumLabel, (f) => f.PickRandom("Yes", "No"));
        faker.RuleFor(t => t.IsSelected, (f) => f.Random.Bool());
        return faker;
    }
}
