using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers
{
    public class NotSelectedManagerTests
    {
        private readonly SessionFake _testSession = new SessionFake();

        [Fact]
        public void AddAll_adds_all()
        {
            // arrange
            var pages = new HashSet<string>() { "1", "2" };
            _testSession.SetString(NotSelectedManager.NotSelectedKey, JsonConvert.SerializeObject(pages));

            // act
            var manager = GetManager();
            manager.AddAll(pages);

            // assert
            Assert.True(
                JsonConvert.DeserializeObject<HashSet<string>>(
                    _testSession.GetString(NotSelectedManager.NotSelectedKey)).Count == 0);
        }

        [Fact]
        public void RemoveAll_removes_all()
        {
            // arrange
            var pages = new HashSet<string>() { "1", "2" };

            // act
            var manager = GetManager();
            manager.RemoveAll(pages);

            // assert
            Assert.True(
                JsonConvert.DeserializeObject<HashSet<string>>(
                    _testSession.GetString(NotSelectedManager.NotSelectedKey)).Count == 2);
        }

        [Fact]
        public void Clear_clears()
        {
            // arrange
            var pages = new HashSet<string>() { "1", "2" };

            // act
            var manager = GetManager();
            manager.AddAll(pages);
            manager.Clear();

            // assert
            Assert.True(!_testSession.Keys.Contains(NotSelectedManager.NotSelectedKey));
        }

        [Fact]
        public void GetSelected_gets_selected()
        {
            // arrange
            var pages = new HashSet<string>() { "1", "2" };
            _testSession.SetString(NotSelectedManager.NotSelectedKey, JsonConvert.SerializeObject(pages));

            // act
            var manager = GetManager();
            manager.AddAll(pages);

            // assert
            Assert.True(
                JsonConvert.DeserializeObject<HashSet<string>>(
                    _testSession.GetString(NotSelectedManager.NotSelectedKey)).Count == 0);
        }

        private NotSelectedManager GetManager()
        {

            var testContext = new DefaultHttpContext() { Session = _testSession };
            var mockContextAccessor = Substitute.For<IHttpContextAccessor>();
            mockContextAccessor.HttpContext.Returns(testContext);

            return new NotSelectedManager(mockContextAccessor);
        }
    }
}
