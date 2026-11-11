using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Services;

public sealed class PaginatePupilsHandlerTests
{
    [Fact]
    public void PaginatePupils_Returns_Empty_When_Pupils_Is_Null()
    {
        // Arrange
        PaginatePupilsHandler sut = new();
        PaginationOptions options = new(page: 1, resultsSize: 10);

        // Act
        IEnumerable<Pupil> result = sut.PaginatePupils(null!, options);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void PaginatePupils_Returns_Empty_When_Pupils_Is_Empty()
    {
        // Arrange
        PaginatePupilsHandler sut = new();
        PaginationOptions options = new(page: 10, resultsSize: 1);

        // Act
        IEnumerable<Pupil> result = sut.PaginatePupils([], options);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void PaginatePupils_Returns_SameOrder_When_Options_Is_Null()
    {
        // Arrange
        PaginatePupilsHandler sut = new();
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 7);

        // Act
        IEnumerable<Pupil> result = sut.PaginatePupils(pupils, null!);

        // Assert
        Assert.Equal(pupils, result);
    }

    [Fact]
    public void PaginatePupils_Page1_Size5_Returns_First5()
    {
        // Arrange
        PaginatePupilsHandler sut = new();
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 20);
        PaginationOptions options = new(page: 1, resultsSize: 5);

        // Act
        IEnumerable<Pupil> result = sut.PaginatePupils(pupils, options);

        // Assert
        IEnumerable<Pupil> expected = pupils.Take(5);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PaginatePupils_Page2_Size5_Returns_Second5()
    {
        // Arrange
        PaginatePupilsHandler sut = new();
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 20);
        PaginationOptions options = new(page: 2, resultsSize: 5);

        // Act
        IEnumerable<Pupil> result = sut.PaginatePupils(pupils, options);

        // Assert
        IEnumerable<Pupil> expected = pupils.Skip(5).Take(5);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PaginatePupils_LastFullPage_Returns_ExactBlock()
    {
        // Arrange
        PaginatePupilsHandler sut = new();
        // 30 items, size 10 → last full page is page 3 → items [20..29]
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 30);
        PaginationOptions options = new(page: 3, resultsSize: 10);

        // Act
        IEnumerable<Pupil> result = sut.PaginatePupils(pupils, options);

        // Assert
        IEnumerable<Pupil> expected = pupils.Skip(20).Take(10);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PaginatePupils_LastPartialPage_Returns_Remainder()
    {
        // Arrange
        PaginatePupilsHandler sut = new();
        // 26 items, size 10 → last partial page is page 3 → items [20..25] (6 items)
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 26);
        PaginationOptions options = new(page: 3, resultsSize: 10);

        // Act
        IEnumerable<Pupil> result = sut.PaginatePupils(pupils, options);

        // Assert
        IEnumerable<Pupil> expected = pupils.Skip(20).Take(10);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PaginatePupils_PageBeyondRange_Returns_SameOrder()
    {
        // Arrange
        PaginatePupilsHandler sut = new();
        // 15 items, size 10 → page 3 would skip 20 >= 15 → return original (per handler)
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 15);
        PaginationOptions options = new(page: 3, resultsSize: 10);

        // Act
        IEnumerable<Pupil> result = sut.PaginatePupils(pupils, options);

        // Assert
        Assert.Equal(pupils, result);
    }

    [Fact]
    public void PaginatePupils_PageOne_SkipZero_TakeSize()
    {
        // Arrange
        PaginatePupilsHandler sut = new();
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 9);
        PaginationOptions options = new(page: 1, resultsSize: 4);

        // Act
        IEnumerable<Pupil> result = sut.PaginatePupils(pupils, options);

        // Assert
        IEnumerable<Pupil> expected = pupils.Take(4);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PaginatePupils_ExactEdge_SkipEqualsCountMinusSize_Returns_LastWindow()
    {
        // Arrange
        PaginatePupilsHandler sut = new();
        // 25 items, size 10 → page 2 => skip 10, page 3 => skip 20 exact edge
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 25);

        PaginationOptions options = new(page: 3, resultsSize: 10);

        // Act
        IEnumerable<Pupil> result = sut.PaginatePupils(pupils, options);

        // Assert
        IEnumerable<Pupil> expected = pupils.Skip(20).Take(10);
        Assert.Equal(expected, result);
    }
}
