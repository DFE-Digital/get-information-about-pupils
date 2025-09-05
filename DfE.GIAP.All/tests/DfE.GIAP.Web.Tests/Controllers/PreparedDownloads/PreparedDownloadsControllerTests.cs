using System.Security.Claims;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Web.Controllers.PreparedDownload;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.PrePreparedDownload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.PrePreparedDownloads;

[Trait("PreparedDownloads", "PreparedDownloads Controller Unit Tests")]
public class PreparedDownloadsControllerTests : IClassFixture<UserClaimsPrincipalFake>
{
  
}
