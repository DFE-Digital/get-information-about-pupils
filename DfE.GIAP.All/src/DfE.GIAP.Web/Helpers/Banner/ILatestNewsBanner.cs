namespace DfE.GIAP.Web.Helpers.Banner;

public interface ILatestNewsBanner
{
    public Task SetLatestNewsStatus();
    public Task RemoveLatestNewsStatus();
}
