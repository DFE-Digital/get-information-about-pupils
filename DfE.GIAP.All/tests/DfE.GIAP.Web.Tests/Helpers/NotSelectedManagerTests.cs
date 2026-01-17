using System.Text;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers;

public sealed class NotSelectedManagerTests
{
    private readonly SessionFake _testSession = new();

    [Fact]
    public void AddAll_adds_all()
    {
        // arrange
        HashSet<string> pages = ["1", "2"];
        _testSession.SetString(NotSelectedManager.NotSelectedKey, JsonConvert.SerializeObject(pages));

        // act
        NotSelectedManager manager = GetManager();
        manager.AddAll(pages);

        // assert
        Assert.Empty(JsonConvert.DeserializeObject<HashSet<string>>(
                _testSession.GetString(NotSelectedManager.NotSelectedKey)!)!);
    }

    [Fact]
    public void RemoveAll_removes_all()
    {
        // arrange
        HashSet<string> pages = ["1", "2"];

        // act
        NotSelectedManager manager = GetManager();
        manager.RemoveAll(pages);

        // assert
        Assert.Equal(2, JsonConvert.DeserializeObject<HashSet<string>>(
                _testSession.GetString(NotSelectedManager.NotSelectedKey)!)!.Count);
    }

    [Fact]
    public void Clear_clears()
    {
        // arrange
        HashSet<string> pages = ["1", "2"];

        // act
        NotSelectedManager manager = GetManager();
        manager.AddAll(pages);
        manager.Clear();

        // assert
        Assert.True(!_testSession.Keys.Contains(NotSelectedManager.NotSelectedKey));
    }

    [Fact]
    public void GetSelected_gets_selected()
    {
        // arrange
        HashSet<string> pages = ["1", "2"];
        _testSession.SetString(NotSelectedManager.NotSelectedKey, JsonConvert.SerializeObject(pages));

        // act
        NotSelectedManager manager = GetManager();
        manager.AddAll(pages);

        // assert
        Assert.Empty(JsonConvert.DeserializeObject<HashSet<string>>(
                _testSession.GetString(NotSelectedManager.NotSelectedKey)!)!);
    }

    private NotSelectedManager GetManager()
    {

        DefaultHttpContext testContext = new() { Session = _testSession };
        IHttpContextAccessor mockContextAccessor = Substitute.For<IHttpContextAccessor>();
        mockContextAccessor.HttpContext.Returns(testContext);

        return new NotSelectedManager(mockContextAccessor);
    }
}
