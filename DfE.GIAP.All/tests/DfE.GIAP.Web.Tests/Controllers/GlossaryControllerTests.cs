using System.Security.Claims;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers;

[Trait("Category", "Glossary Controller Unit Tests")]
public class GlossaryControllerTests : IClassFixture<GlossaryResultsFake>
{
}
