namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal static class DateTimeTestDoubles
{
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
