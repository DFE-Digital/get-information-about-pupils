using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.AddToMyPupils;
public sealed class AddPupilsToMyPupilsUseCaseTests
{
    [Fact]
    public void Constructor_Throws_When_ReadRepository_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Func<AddPupilsToMyPupilsUseCase> construct = () => new(null!, writeRepoMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_WriteRepository_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();

        Func<AddPupilsToMyPupilsUseCase> construct = () => new(readRepoMock.Object, null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();

        AddPupilsToMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object);
        Func<Task> act = async () => await sut.HandleRequestAsync(request: null!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_DoesNotSave_InvalidUpns()
    {
        MyPupilsId id = new("id");

        List<UniquePupilNumber> originalMyPupils = UniquePupilNumberTestDoubles.Generate(count: 15);

        MyPupilsAggregate myPupilsAggregate = MyPupilsAggregateTestDoubles.Create(id, UniquePupilNumbers.Create(originalMyPupils));

        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupils(stub: myPupilsAggregate);

        AddPupilsToMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object);

        // Act
        List<string> upnsWithInvalidUpns = ["Invalid1", UniquePupilNumberTestDoubles.Generate().Value, "Invalid2"];

        await sut.HandleRequestAsync(
            new AddPupilsToMyPupilsRequest(
                userId: id.Value,
                pupils: upnsWithInvalidUpns));

        // Assert
        readRepoMock.Verify(t => t.GetMyPupils(It.Is<MyPupilsId>(t => t.Value.Equals("id"))), Times.Once);
        writeRepoMock.Verify(t => t.SaveMyPupilsAsync(It.IsAny<MyPupilsAggregate>()), Times.Once);

        List<UniquePupilNumber> expectedUniquePupilNumbers = originalMyPupils.Concat([new UniquePupilNumber(upnsWithInvalidUpns[1])]).ToList();

        Assert.Equal(16, myPupilsAggregate.PupilCount);
        Assert.False(myPupilsAggregate.HasNoPupils);
        Assert.Equivalent(expectedUniquePupilNumbers, myPupilsAggregate.GetMyPupils());
    }

    [Theory]
    [MemberData(nameof(ValidUpns))]
    public async Task HandleAsync_AddsUpns_And_Saves(List<UniquePupilNumber> addUpns)
    {
        MyPupilsId id = new("id");

        List<UniquePupilNumber> originalMyPupils = UniquePupilNumberTestDoubles.Generate(count: 5);

        MyPupilsAggregate myPupilsAggregate = MyPupilsAggregateTestDoubles.Create(id, UniquePupilNumbers.Create(originalMyPupils));

        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupils(stub: myPupilsAggregate);

        AddPupilsToMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object);

        // Act
        await sut.HandleRequestAsync(
            new AddPupilsToMyPupilsRequest(userId: id.Value, pupils: addUpns.Select(upn => upn.Value)));

        // Assert
        readRepoMock.Verify(t => t.GetMyPupils(It.Is<MyPupilsId>(t => t.Value.Equals("id"))), Times.Once);
        writeRepoMock.Verify(t => t.SaveMyPupilsAsync(It.IsAny<MyPupilsAggregate>()), Times.Once);

        List<UniquePupilNumber> expectedUniquePupilNumbers = originalMyPupils.Concat(addUpns).ToList();

        Assert.Equal(originalMyPupils.Count + addUpns.Count, myPupilsAggregate.PupilCount);
        Assert.False(myPupilsAggregate.HasNoPupils);
        Assert.Equivalent(expectedUniquePupilNumbers, myPupilsAggregate.GetMyPupils());
    }

    public static TheoryData<List<UniquePupilNumber>> ValidUpns => new(
        UniquePupilNumberTestDoubles.Generate(count: 1),
        UniquePupilNumberTestDoubles.Generate(count: 15));
}
