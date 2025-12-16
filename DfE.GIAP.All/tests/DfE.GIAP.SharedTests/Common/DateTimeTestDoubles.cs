namespace DfE.GIAP.SharedTests.Common;
public static class DateTimeTestDoubles
{
    public static DateTime GenerateFor(int year, int month, int day)
    {
        return new(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    }

    public static DateTime GenerateFutureDate()
    {
        return DateTime.Now.AddDays(1);
    }

    public static DateTime GenerateDateOfBirthForAgeOf(int age)
    {
        return DateTime.Today.AddYears(age < 0 ? age : -age);
    }

    public static DateTime GenerateDateOfBirthThatHasAlreadyOccuredThisYear()
    {
        return GenerateDateOfBirthForAgeOf(10).AddDays(-1);
    }

    public static DateTime GenerateDateOfBirthThatHasNotOccuredYetThisYear()
    {
        return GenerateDateOfBirthForAgeOf(10).AddDays(1);
    }
}
