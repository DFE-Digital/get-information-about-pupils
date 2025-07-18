namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal static class DateTimeTestDoubles
{

    internal static DateTime GenerateFor(int year, int month, int day)
    {
        return new(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    }

    internal static DateTime GenerateFutureDate()
    {
        return DateTime.Now.AddDays(1);
    }

    internal static DateTime GenerateDateOfBirthForAgeOf(int age)
    {
        return DateTime.Today.AddYears(age < 0 ? age : -age);
    }

    internal static DateTime GenerateDateOfBirthThatHasAlreadyOccuredThisYear()
    {
        return GenerateDateOfBirthForAgeOf(10).AddDays(-1);
    }
    internal static DateTime GenerateDateOfBirthThatHasNotOccuredYetThisYear()
    {
        return GenerateDateOfBirthForAgeOf(10).AddDays(1);
    }
}
