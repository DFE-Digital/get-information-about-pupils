namespace DfE.GIAP.SharedTests.Infrastructure;
public interface IMapper<in TIn, out TOut>
{
    TOut Map(TIn input);
}
