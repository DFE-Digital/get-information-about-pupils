using DfE.GIAP.Web.Helpers.Consent;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers
{
    public class ConsentHelperTests
    {
        [Fact]
        public void ConsentHelper_sets_and_retrieves_consent_correctly()
        {
            var context = CreateContextWithSession();
            ConsentHelper.SetConsent(context);
            Assert.True(ConsentHelper.HasGivenConsent(context));
        }

        [Fact]
        public void ConsentHelper_HasGivenConsent_returns_false_if_consent_not_set()
        {
            var context = CreateContextWithSession();
            Assert.False(ConsentHelper.HasGivenConsent(context));
        }

        [Fact]
        public void ConsentHelper_RemoveConsent_works_and_HasGivenConsent_fails()
        {
            var context = CreateContextWithSession();
            ConsentHelper.SetConsent(context);
            ConsentHelper.RemoveConsent(context);
            Assert.False(ConsentHelper.HasGivenConsent(context));
        }

        private HttpContext CreateContextWithSession()
        {
            var context = new DefaultHttpContext();
            context.Session = new TestSession();
            return context;
        }      
    }
}
