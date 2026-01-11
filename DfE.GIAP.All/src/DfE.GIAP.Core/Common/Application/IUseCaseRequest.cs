namespace DfE.GIAP.Core.Common.Application;

/// <summary>
/// Marker contract which defines the response object to be associated with a use-case request.
/// </summary>
/// <typeparam name="TUseCaseResponse">The runtime type definition of the use-case response object.</typeparam>
#pragma warning disable S2326 // Unused type parameters should be removed
public interface IUseCaseRequest<out TUseCaseResponse> where TUseCaseResponse : class
#pragma warning restore S2326 // Unused type parameters should be removed
{
}

/// <summary>
/// Marker contract which defines response-less use-case request.
/// </summary>
public interface IUseCaseRequest
{
}
