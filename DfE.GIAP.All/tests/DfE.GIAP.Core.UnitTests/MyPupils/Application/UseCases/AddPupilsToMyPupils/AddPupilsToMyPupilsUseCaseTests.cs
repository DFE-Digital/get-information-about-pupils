using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.AddPupilsToMyPupils;

public sealed class AddPupilsToMyPupilsUseCaseTests
{
    [Fact]
    public void Constructor_Throws_When_ReadRepository_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock = MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        Func<AddPupilsToMyPupilsUseCase> construct = () => new(null!, writeRepoMock.Object, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_WriteRepository_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();
        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock = MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        Func<AddPupilsToMyPupilsUseCase> construct = () => new(readRepoMock.Object, null!, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Func<AddPupilsToMyPupilsUseCase> construct = () => new(readRepoMock.Object, writeRepoMock.Object, null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }


    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();
        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock = MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        AddPupilsToMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object, mapperMock.Object);
        Func<Task> act = async () => await sut.HandleRequestAsync(request: null!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_DoesNotSave_InvalidUpns()
    {
        MyPupilsId id = MyPupilsIdTestDoubles.Default();

        List<UniquePupilNumber> originalMyPupils = UniquePupilNumberTestDoubles.Generate(count: 15);

        MyPupilsAggregate myPupilsAggregate = MyPupilsAggregateTestDoubles.Create(id, UniquePupilNumbers.Create(originalMyPupils));

        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupils(stub: myPupilsAggregate);

        UniquePupilNumber validUpn = UniquePupilNumberTestDoubles.Generate();

        List<string> someInvalidUpns = [
            "Invalid1",
            validUpn.Value,
            "Invalid2"];

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.MockFor<IEnumerable<string>, UniquePupilNumbers>(
                stub: UniquePupilNumbers.Create([validUpn]));

        AddPupilsToMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object, mapperMock.Object);

        // Act

        await sut.HandleRequestAsync(
            new AddPupilsToMyPupilsRequest(
                userId: id.Value,
                pupils: someInvalidUpns));

        // Assert
        mapperMock.Verify(t => t.Map(It.Is<IEnumerable<string>>(ups => ups.SequenceEqual(someInvalidUpns))), Times.Once);
        readRepoMock.Verify(t => t.GetMyPupils(It.Is<MyPupilsId>(t => t.Value.Equals(id.Value))), Times.Once);
        writeRepoMock.Verify(t => t.SaveMyPupilsAsync(It.IsAny<MyPupilsAggregate>()), Times.Once);

        List<UniquePupilNumber> expectedUniquePupilNumbers = [.. originalMyPupils, validUpn];

        Assert.Equal(16, myPupilsAggregate.PupilCount);
        Assert.False(myPupilsAggregate.HasNoPupils);
        Assert.Equivalent(expectedUniquePupilNumbers, myPupilsAggregate.GetMyPupils());
    }

    [Theory]
    [MemberData(nameof(ValidUpns))]
    public async Task HandleAsync_AddsUpns_And_Saves(List<UniquePupilNumber> addUpns)
    {
        MyPupilsId id = MyPupilsIdTestDoubles.Default();

        List<UniquePupilNumber> originalMyPupils = UniquePupilNumberTestDoubles.Generate(count: 5);

        MyPupilsAggregate myPupilsAggregate = MyPupilsAggregateTestDoubles.Create(id, UniquePupilNumbers.Create(originalMyPupils));

        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupils(stub: myPupilsAggregate);

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.MockFor<IEnumerable<string>, UniquePupilNumbers>(
                stub: UniquePupilNumbers.Create(addUpns));

        AddPupilsToMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object, mapperMock.Object);

        // Act
        List<string> addUpnValues = addUpns.Select(upn => upn.Value).ToList();

        await sut.HandleRequestAsync(
            new AddPupilsToMyPupilsRequest(
                userId: id.Value,
                pupils: addUpnValues));

        // Assert
        readRepoMock.Verify(t => t.GetMyPupils(It.Is<MyPupilsId>(t => t.Value.Equals(id.Value))), Times.Once);

        writeRepoMock.Verify(t => t.SaveMyPupilsAsync(It.IsAny<MyPupilsAggregate>()), Times.Once);

        mapperMock.Verify(t => t.Map(It.Is<IEnumerable<string>>(ups => ups.SequenceEqual(addUpnValues))), Times.Once);

        List<UniquePupilNumber> expectedUniquePupilNumbers = [.. originalMyPupils, .. addUpns];

        Assert.Equal(originalMyPupils.Count + addUpns.Count, myPupilsAggregate.PupilCount);
        Assert.False(myPupilsAggregate.HasNoPupils);
        Assert.Equivalent(expectedUniquePupilNumbers, myPupilsAggregate.GetMyPupils());
    }

    public static TheoryData<List<UniquePupilNumber>> ValidUpns => new(
        UniquePupilNumberTestDoubles.Generate(count: 1),
        UniquePupilNumberTestDoubles.Generate(count: 15));
}
