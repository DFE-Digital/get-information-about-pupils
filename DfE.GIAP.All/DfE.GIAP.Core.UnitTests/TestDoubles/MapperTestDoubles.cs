using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;

internal static class MapperTestDoubles
{
    internal static Mock<IMapper<TIn, TOut>> DefaultFromTo<TIn, TOut>() where TOut : class => new();

    internal static Mock<IMapper<TIn, TOut>> MockMapperFromTo<TIn, TOut>(TOut? stub = null) where TOut : class
        => MockMapperFromTo<TIn, TOut>(() => stub!);

    internal static Mock<IMapper<TIn, TOut>> MockMapperFromTo<TIn, TOut>(Func<TOut> stubProvider) where TOut : class
    {
        Mock<IMapper<TIn, TOut>> mapper = DefaultFromTo<TIn, TOut>();

        mapper
            .Setup(t => t.Map(It.IsAny<TIn>()))
            .Returns(stubProvider)
            .Verifiable();

        return mapper;
    }
}
