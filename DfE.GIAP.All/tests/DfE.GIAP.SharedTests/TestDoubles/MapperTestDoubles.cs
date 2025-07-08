using DfE.GIAP.Core.Common.CrossCutting;
using Moq;

namespace DfE.GIAP.Core.SharedTests.TestDoubles;

public static class MapperTestDoubles
{
    public static Mock<IMapper<TIn, TOut>> Default<TIn, TOut>() where TOut : class => new();

    public static Mock<IMapper<TIn, TOut>> MockFor<TIn, TOut>(TOut? stub = null) where TOut : class
        => MockFor<TIn, TOut>(() => stub!);

    public static Mock<IMapper<TIn, TOut>> MockFor<TIn, TOut>(Func<TOut> stubProvider) where TOut : class
    {
        Mock<IMapper<TIn, TOut>> mapper = Default<TIn, TOut>();

        mapper
            .Setup(t => t.Map(It.IsAny<TIn>()))
            .Returns(stubProvider)
            .Verifiable();

        return mapper;
    }
}
