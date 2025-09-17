using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.SharedTests.TestDoubles;
using System.Linq;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.AddToMyPupils;
public sealed class AddPupilsToMyPupilsUseCaseTests
{
    [Fact]
    public void Constructor_Throws_When_ReadRepository_Is_Null()
    {
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Func<AddPupilsToMyPupilsUseCase> construct = () => new(null!, writeRepoMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_WriteRepository_Is_Null()
    {
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();

        Func<AddPupilsToMyPupilsUseCase> construct = () => new(readRepoMock.Object, null!);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();

        AddPupilsToMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object);

        //Act
        Func<Task> act = async () => await sut.HandleRequestAsync(request: null!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_DoesNotSave_InvalidUpns()
    {
        MyPupilsId id = new("id");

        List<UniquePupilNumber> originalMyPupils = UniquePupilNumberTestDoubles.Generate(count: 15);

        Core.MyPupils.Domain.AggregateRoot.MyPupils myPupilsAggregate = MyPupilsAggregateRootTestDoubles.Create(id, UniquePupilNumbers.Create(originalMyPupils));

        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupils(stub: myPupilsAggregate);

        AddPupilsToMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object);

        // Act
        List<string> upnsWithInvalidUpns = ["Invalid1", UniquePupilNumberTestDoubles.Generate().Value, "Invalid2"];

        await sut.HandleRequestAsync(
            new AddPupilsToMyPupilsRequest(
                userId: id.Value,
                pupils: upnsWithInvalidUpns));

        readRepoMock.Verify(t => t.GetMyPupils(It.Is<MyPupilsId>(t => t.Value.Equals("id"))), Times.Once);
        writeRepoMock.Verify(t => t.SaveMyPupilsAsync(It.IsAny<Core.MyPupils.Domain.AggregateRoot.MyPupils>()), Times.Once);

        List<UniquePupilNumber> expectedUniquePupilNumbers = originalMyPupils.Concat([new UniquePupilNumber(upnsWithInvalidUpns[1])]).ToList();

        Assert.Equal(16, myPupilsAggregate.PupilCount);
        Assert.False(myPupilsAggregate.HasNoPupils);
        Assert.Equivalent(expectedUniquePupilNumbers, myPupilsAggregate.GetMyPupils());
    }

    //[Fact]
    //public async Task HandleAsync_AddsUpns()
    //{

    //}
}
