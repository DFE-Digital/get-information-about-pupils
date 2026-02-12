using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.Ports;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;


namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Services;

public sealed class AggregatePupilsForMyPupilsApplicationServiceTests
{

    [Fact]
    public void Constuctor_Throws_When_QueryPort_Is_Null()
    {
        // Arrange
        
        Func<AggregatePupilsForMyPupilsApplicationService> construct = () =>
            new(
                queryMyPupilsPort: null!,
                new Mock<IOrderPupilsHandler>().Object,
                new Mock<IPaginatePupilsHandler>().Object);

        // Act Assert
        Assert.ThrowsAny<ArgumentException>(() => construct());
    }

    [Fact]
    public void Constuctor_Throws_When_OrderHandler_Is_Null()
    {
        // Arrange
        Func<AggregatePupilsForMyPupilsApplicationService> construct = () =>
            new AggregatePupilsForMyPupilsApplicationService(
                queryMyPupilsPort: new Mock<IQueryMyPupilsPort>().Object,
                orderPupilsHandler: null!,
                paginatePupilsHandler: new Mock<IPaginatePupilsHandler>().Object);

        // Act Assert
        Assert.ThrowsAny<ArgumentException>(() => construct());
    }

    [Fact]
    public void Constuctor_Throws_When_PaginateHandler_Is_Null()
    {
        // Arrange
        Func<AggregatePupilsForMyPupilsApplicationService> construct = () =>
            new AggregatePupilsForMyPupilsApplicationService(
                queryMyPupilsPort: new Mock<IQueryMyPupilsPort>().Object,
                orderPupilsHandler: new Mock<IOrderPupilsHandler>().Object,
                paginatePupilsHandler: null!);

        // Act Assert
        Assert.ThrowsAny<ArgumentException>(() => construct());
    }

    [Fact]
    public async Task GetPupilsAsync_ReturnsEmpty_WhenNoUPNsProvided()
    {
        // Arrange
        Mock<IQueryMyPupilsPort> queryPort = new();
        Mock<IOrderPupilsHandler> order = new();
        Mock<IPaginatePupilsHandler> paginate = new();

        AggregatePupilsForMyPupilsApplicationService sut =
            new(queryPort.Object, order.Object, paginate.Object);

        UniquePupilNumbers request = UniquePupilNumbers.Create(Array.Empty<UniquePupilNumber>());
        MyPupilsQueryModel query = MyPupilsQueryModel.CreateDefault();

        // Act
        IEnumerable<Pupil> result = await sut.GetPupilsAsync(request, query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
        queryPort.VerifyNoOtherCalls();
        order.VerifyNoOtherCalls();
        paginate.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPupilsAsync_Throws_WhenUPNCountExceedsLimit()
    {
        // Arrange
        Mock<IQueryMyPupilsPort> queryPort = new();
        Mock<IOrderPupilsHandler> order = new();
        Mock<IPaginatePupilsHandler> paginate = new();

        AggregatePupilsForMyPupilsApplicationService sut =
            new(queryPort.Object, order.Object, paginate.Object);

        const int exceedsLimit = 4001;
        IEnumerable<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(exceedsLimit);
        UniquePupilNumbers request = UniquePupilNumbers.Create(upns);

        MyPupilsQueryModel query = MyPupilsQueryModel.CreateDefault();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.GetPupilsAsync(request, query, CancellationToken.None));

        queryPort.VerifyNoOtherCalls();
        order.VerifyNoOtherCalls();
        paginate.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPupilsAsync_DoesNotThrow_WhenUPNCountEqualsLimit()
    {
        // Arrange
        Mock<IQueryMyPupilsPort> queryPort = new();
        Mock<IOrderPupilsHandler> order = new();
        Mock<IPaginatePupilsHandler> paginate = new();

        List<Pupil> portResponse = PupilTestDoubles.Generate(2);
        queryPort
            .Setup(p => p.QueryAsync(
                It.IsAny<UniquePupilNumbers>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(portResponse);

        AggregatePupilsForMyPupilsApplicationService sut =
            new(queryPort.Object, order.Object, paginate.Object);

        const int atLimit = 4000;
        IEnumerable<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(atLimit);
        UniquePupilNumbers request = UniquePupilNumbers.Create(upns);

        // Null query means it will return all pupils from the port
        MyPupilsQueryModel? query = null;

        // Act
        IEnumerable<Pupil> result = await sut.GetPupilsAsync(request, query, CancellationToken.None);

        // Assert
        Assert.Same(result, portResponse);
        queryPort.Verify(p => p.QueryAsync(
                It.Is<UniquePupilNumbers>(r => r.Count == atLimit),
                It.IsAny<CancellationToken>()),
            Times.Once);

        order.VerifyNoOtherCalls();
        paginate.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPupilsAsync_WhenQueryIsNull_ReturnsAllPupils_AndSkips_OrderAndPaginate()
    {
        // Arrange
        Mock<IQueryMyPupilsPort> queryPort = new();
        Mock<IOrderPupilsHandler> order = new();
        Mock<IPaginatePupilsHandler> paginate = new();

        List<Pupil> allPupils = PupilTestDoubles.Generate(5);
        queryPort
            .Setup(p => p.QueryAsync(
                It.IsAny<UniquePupilNumbers>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(allPupils);

        AggregatePupilsForMyPupilsApplicationService sut =
            new(queryPort.Object, order.Object, paginate.Object);

        UniquePupilNumbers requestUpns = 
            UniquePupilNumbers.Create(
                UniquePupilNumberTestDoubles.Generate(3));
        
        CancellationTokenSource cts = new();

        // Act
        IEnumerable<Pupil> result = await sut.GetPupilsAsync(
            uniquePupilNumbers: requestUpns, 
            query: null, 
            ctx: cts.Token);

        // Assert
        Assert.Same(result, allPupils);

        queryPort.Verify(p => p.QueryAsync(
                It.Is<UniquePupilNumbers>(r => r.Count == 3),
                It.Is<CancellationToken>(t => t == cts.Token)),
            Times.Once);

        order.VerifyNoOtherCalls();
        paginate.VerifyNoOtherCalls();
    }


    [Fact]
    public async Task GetPupilsAsync_WhenQueryProvided_Orders_Then_Paginates_InSequence_And_Passes_Correct_Arguments()
    {
        // Arrange
        Mock<IQueryMyPupilsPort> queryPort = new();
        Mock<IOrderPupilsHandler> order = new();
        Mock<IPaginatePupilsHandler> paginate = new();

        List<Pupil> allPupils = PupilTestDoubles.Generate(6);
        List<Pupil> orderedPupils = PupilTestDoubles.Generate(5);
        List<Pupil> pagedPupils = PupilTestDoubles.Generate(3);

        queryPort
            .Setup(p => p.QueryAsync(
                It.IsAny<UniquePupilNumbers>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(allPupils);

        MyPupilsQueryModel query = MyPupilsQueryModel.CreateDefault();

        List<Pupil>? observedOrderInput = null;

        MockSequence sequence = new();

        order
            .InSequence(sequence)
            .Setup(o => o.Order(
                It.IsAny<IEnumerable<Pupil>>(),
                It.Is<OrderOptions>(orderOptions => ReferenceEquals(orderOptions, query.Order))))
            .Callback<IEnumerable<Pupil>, OrderOptions>((input, _) =>
            {
                observedOrderInput = input.ToList();
            })
            .Returns(orderedPupils);

        paginate
            .InSequence(sequence)
            .Setup(p => p.PaginatePupils(
                It.Is<IEnumerable<Pupil>>(arg => ReferenceEquals(arg, orderedPupils)),
                It.Is<PaginationOptions>(paginateOptions => ReferenceEquals(paginateOptions, query.PaginateOptions))))
            .Returns(pagedPupils);

        AggregatePupilsForMyPupilsApplicationService sut =
            new(queryPort.Object, order.Object, paginate.Object);

        UniquePupilNumbers request = 
            UniquePupilNumbers.Create(
                UniquePupilNumberTestDoubles.Generate(6));

        // Act
        IEnumerable<Pupil> result = await sut.GetPupilsAsync(request, query, CancellationToken.None);

        // Assert
        Assert.Same(pagedPupils, result);

        // Ensure the query port was called once with our request
        queryPort.Verify(p => p.QueryAsync(
                It.Is<UniquePupilNumbers>(r => r.Count == 6),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Ensure order was invoked with the *port result* and our query.Order
        order.Verify(o => o.Order(
                It.IsAny<IEnumerable<Pupil>>(),
                It.Is<OrderOptions>(oo => ReferenceEquals(oo, query.Order))),
            Times.Once);

        // Ensure paginate was invoked with the *ordered result* and our query.PaginateOptions
        paginate.Verify(p => p.PaginatePupils(
                It.Is<IEnumerable<Pupil>>(arg => ReferenceEquals(arg, orderedPupils)),
                It.Is<PaginationOptions>(po => ReferenceEquals(po, query.PaginateOptions))),
            Times.Once);

        // Ensure the exact input to Order was the full set returned by the port
        Assert.NotNull(observedOrderInput);
        Assert.Equal(observedOrderInput, allPupils);
    }
}