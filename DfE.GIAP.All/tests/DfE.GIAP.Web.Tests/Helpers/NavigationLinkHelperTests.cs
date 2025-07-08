using DfE.GIAP.Web.Helpers.Navigation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers
{
    public class NavigationLinkHelperTests
    {
        [Theory]
        [ClassData(typeof(ActiveControllerLinkStubs))]
        public void Process_Generates_Expected_Attributes_Active_Link(string tagController, string currentController)
        {
            // Arrange
            const string expectedTagName = "is-active-route";
            var metadataProvider = new EmptyModelMetadataProvider();

            var tagHelperContext = new TagHelperContext(
                tagName: expectedTagName,
                allAttributes: new TagHelperAttributeList
                {
                    { "asp-controller", tagController },
                    { "class", "class" }
                },
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                expectedTagName,
                attributes: new TagHelperAttributeList
                {
                    { "class", "class" }
                },
                getChildContentAsync: (_, __) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
            var viewContext = TestableHtmlGenerator.GetViewContext(
               model: null,
               htmlGenerator: htmlGenerator,
               metadataProvider: metadataProvider);
            viewContext.RouteData.Values.Add("controller", currentController);

            var navigationLinkHelper = new NavigationLinkHelper(htmlGenerator)
            {
                Controller = tagController,
                ViewContext = viewContext
            };

            // Act
            navigationLinkHelper.Process(tagHelperContext, output);

            // Assert
            var classAttribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("class"));
            Assert.Equal("class webapp-header-link--active", classAttribute.Value.ToString());
            var ariaCurrentAttribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("aria-current"));
            Assert.Equal("page", ariaCurrentAttribute.Value.ToString());
        }

        [Theory]
        [ClassData(typeof(NonActiveControllerLinkStubs))]
        public void Process_Generates_Expected_Attributes_Non_Active_Link(string tagController, string currentController)

        {
            // Arrange
            var expectedTagName = "is-active-route";
            var metadataProvider = new EmptyModelMetadataProvider();

            var tagHelperContext = new TagHelperContext(
                tagName: expectedTagName,
                allAttributes: new TagHelperAttributeList
                {
                    { "asp-controller", tagController },
                    { "class", "class" }
                },
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                expectedTagName,
                attributes: new TagHelperAttributeList
                {
                    { "class", "class" }
                },
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
            var viewContext = TestableHtmlGenerator.GetViewContext(
               model: null,
               htmlGenerator: htmlGenerator,
               metadataProvider: metadataProvider);
            viewContext.RouteData.Values.Add("controller", currentController);

            var navigationLinkHelper = new NavigationLinkHelper(htmlGenerator)
            {
                Controller = tagController,
                ViewContext = viewContext
            };

            // Act
            navigationLinkHelper.Process(tagHelperContext, output);

            // Assert
            var classAttribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("class"));
            Assert.DoesNotContain("webapp-header-link--active", classAttribute.Value.ToString());
            Assert.DoesNotContain(output.Attributes, attr => attr.Name.Equals("aria-current"));
        }

        [Fact]
        public void Process_Generates_Expected_Attributes_Active_Link_Null_Input_Class()

        {
            // Arrange
            var expectedTagName = "is-active-route";
            var metadataProvider = new EmptyModelMetadataProvider();

            var tagHelperContext = new TagHelperContext(
                tagName: expectedTagName,
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                expectedTagName,
                attributes: new TagHelperAttributeList
                {
                    { "asp-controller", "test" }
                },
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
            var viewContext = TestableHtmlGenerator.GetViewContext(
               model: null,
               htmlGenerator: htmlGenerator,
               metadataProvider: metadataProvider);
            viewContext.RouteData.Values.Add("controller", "test");

            var navigationLinkHelper = new NavigationLinkHelper(htmlGenerator)
            {
                Controller = ControllerKeys.test,
                ViewContext = viewContext
            };

            // Act
            navigationLinkHelper.Process(tagHelperContext, output);

            // Assert
            var classAttribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("class"));
            Assert.Equal(" webapp-header-link--active", classAttribute.Value.ToString());
            var ariaCurrentAttribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("aria-current"));
            Assert.Equal("page", ariaCurrentAttribute.Value.ToString());
        }

        [Fact]
        public void Process_Generates_Expected_Attributes_Non_Active_Link_Null_Input_Class()

        {
            // Arrange
            var expectedTagName = "is-active-route";
            var metadataProvider = new EmptyModelMetadataProvider();

            var tagHelperContext = new TagHelperContext(
                tagName: expectedTagName,
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                expectedTagName,
                attributes: new TagHelperAttributeList
                {
                    { "asp-controller", "test" }
                },
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
            var viewContext = TestableHtmlGenerator.GetViewContext(
               model: null,
               htmlGenerator: htmlGenerator,
               metadataProvider: metadataProvider);
            viewContext.RouteData.Values.Add("controller", "");

            var navigationLinkHelper = new NavigationLinkHelper(htmlGenerator)
            {
                Controller = ControllerKeys.test,
                ViewContext = viewContext
            };

            // Act
            navigationLinkHelper.Process(tagHelperContext, output);

            // Assert
            Assert.DoesNotContain(output.Attributes, attr => attr.Name.Equals("class"));
            Assert.DoesNotContain(output.Attributes, attr => attr.Name.Equals("aria-current"));
        }

        internal abstract class ControllerLinkStubs : IEnumerable<object[]>
        {
            public List<object[]> FlattenControllersDictionary(
                Dictionary<string, string[]> controllersDictionary) =>
                    controllersDictionary.SelectMany(kvp =>
                        kvp.Value.Select(value => new object[] { kvp.Key, value })).ToList();

            public abstract IEnumerator<object[]> GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal sealed class ActiveControllerLinkStubs : ControllerLinkStubs
        {
            public Dictionary<string, string[]> ActiveControllersDictionary =>
                new Dictionary<string, string[]>
                {
                    { ControllerKeys.NPDLearnerNumberSearch,
                       new string[] { ControllerKeys.NPDLearnerNumberSearch, ControllerKeys.NPDLearnerTextSearch } },
                    { ControllerKeys.FELearnerNumber,
                        new string[] { ControllerKeys.FELearnerNumber, ControllerKeys.FELearnerTextSearch } },
                    { ControllerKeys.PupilPremiumLearnerNumber,
                        new string[] { ControllerKeys.PPLearnerTextSearch, ControllerKeys.PupilPremiumLearnerNumber } },
                    { ControllerKeys.Admin,
                        new string[] { ControllerKeys.Admin, ControllerKeys.ManageDocuments, ControllerKeys.SecurityReportByPupilStudentRecord } },
                    { ControllerKeys.PreparedDownloads,
                        new string[] { ControllerKeys.PreparedDownloads } },
                    { ControllerKeys.SearchMyPupilList,
                        new string[] { ControllerKeys.SearchMyPupilList } },
                    { ControllerKeys.News,
                        new string[] { ControllerKeys.News } }
                };

            public override IEnumerator<object[]> GetEnumerator() =>
                FlattenControllersDictionary(ActiveControllersDictionary).GetEnumerator();
        }

        internal sealed class NonActiveControllerLinkStubs : ControllerLinkStubs
        {
            public Dictionary<string, string[]> NonActiveControllersDictionary =>
                new Dictionary<string, string[]>
                {
                    { ControllerKeys.NPDLearnerNumberSearch,
                        new string[] {
                            ControllerKeys.SecurityReportByPupilStudentRecord, ControllerKeys.Admin, ControllerKeys.ManageDocuments, ControllerKeys.FELearnerNumber,
                            ControllerKeys.PupilPremiumLearnerNumber, ControllerKeys.FELearnerTextSearch, ControllerKeys.PPLearnerTextSearch, ControllerKeys.PreparedDownloads,
                            ControllerKeys.SearchMyPupilList, ControllerKeys.Accessibility, ControllerKeys.Authentication, ControllerKeys.Cookies, ControllerKeys.Glossary,
                            ControllerKeys.Landing, ControllerKeys.News, ControllerKeys.Privacy, ControllerKeys.Terms, ControllerKeys.test, ControllerKeys.Empty } },
                    { ControllerKeys.FELearnerNumber,
                        new string[] {
                            ControllerKeys.SecurityReportByPupilStudentRecord, ControllerKeys.Admin, ControllerKeys.ManageDocuments, ControllerKeys.NPDLearnerNumberSearch,
                            ControllerKeys.PupilPremiumLearnerNumber, ControllerKeys.NPDLearnerTextSearch, ControllerKeys.PPLearnerTextSearch, ControllerKeys.PreparedDownloads,
                            ControllerKeys.SearchMyPupilList, ControllerKeys.Accessibility, ControllerKeys.Authentication, ControllerKeys.Cookies, ControllerKeys.Glossary,
                            ControllerKeys.Landing, ControllerKeys.News, ControllerKeys.test, ControllerKeys.Terms, ControllerKeys.Privacy, ControllerKeys.Empty } },
                    { ControllerKeys.PupilPremiumLearnerNumber,
                        new string[] {
                           ControllerKeys.SecurityReportByPupilStudentRecord, ControllerKeys.Admin, ControllerKeys.ManageDocuments, ControllerKeys.FELearnerNumber, ControllerKeys.NPDLearnerNumberSearch,
                           ControllerKeys.FELearnerTextSearch, ControllerKeys.NPDLearnerTextSearch, ControllerKeys.PreparedDownloads, ControllerKeys.SearchMyPupilList, ControllerKeys.Accessibility,
                            ControllerKeys.Authentication, ControllerKeys.Cookies, ControllerKeys.Glossary, ControllerKeys.Landing, ControllerKeys.News, ControllerKeys.Privacy, ControllerKeys.Terms, ControllerKeys.test, ControllerKeys.Empty } },
                    { ControllerKeys.Admin,
                        new string[] {
                            ControllerKeys.FELearnerNumber, ControllerKeys.NPDLearnerNumberSearch, ControllerKeys.PupilPremiumLearnerNumber, ControllerKeys.FELearnerTextSearch,
                            ControllerKeys.NPDLearnerTextSearch, ControllerKeys.PPLearnerTextSearch, ControllerKeys.PreparedDownloads, ControllerKeys.SearchMyPupilList, ControllerKeys.Accessibility,
                            ControllerKeys.Authentication, ControllerKeys.Cookies, ControllerKeys.Glossary, ControllerKeys.Landing, ControllerKeys.News, ControllerKeys.Privacy, ControllerKeys.Terms, ControllerKeys.test, ControllerKeys.Empty } },
                    { ControllerKeys.PreparedDownloads,
                        new string[] {
                            ControllerKeys.SecurityReportByPupilStudentRecord, ControllerKeys.Admin, ControllerKeys.ManageDocuments, ControllerKeys.FELearnerNumber, ControllerKeys.PupilPremiumLearnerNumber,
                            ControllerKeys.NPDLearnerNumberSearch, ControllerKeys.FELearnerTextSearch, ControllerKeys.NPDLearnerTextSearch, ControllerKeys.PPLearnerTextSearch, ControllerKeys.SearchMyPupilList,
                            ControllerKeys.Accessibility, ControllerKeys.Authentication, ControllerKeys.Cookies, ControllerKeys.Glossary, ControllerKeys.Landing, ControllerKeys.News, ControllerKeys.Privacy, ControllerKeys.Terms, ControllerKeys.test, ControllerKeys.Empty } },
                    { ControllerKeys.SearchMyPupilList,
                        new string[] {
                            ControllerKeys.SecurityReportByPupilStudentRecord, ControllerKeys.Admin, ControllerKeys.ManageDocuments, ControllerKeys.FELearnerNumber, ControllerKeys.PupilPremiumLearnerNumber,
                            ControllerKeys.NPDLearnerNumberSearch, ControllerKeys.FELearnerTextSearch, ControllerKeys.NPDLearnerTextSearch, ControllerKeys.PPLearnerTextSearch, ControllerKeys.PreparedDownloads,
                            ControllerKeys.Accessibility, ControllerKeys.Authentication, ControllerKeys.Cookies, ControllerKeys.Glossary, ControllerKeys.Landing, ControllerKeys.News, ControllerKeys.Privacy, ControllerKeys.Terms, ControllerKeys.test, ControllerKeys.Empty  } },
                    { ControllerKeys.News,
                        new string[] {
                            ControllerKeys.SecurityReportByPupilStudentRecord, ControllerKeys.Admin, ControllerKeys.ManageDocuments, ControllerKeys.FELearnerNumber, ControllerKeys.PupilPremiumLearnerNumber,
                            ControllerKeys.NPDLearnerNumberSearch, ControllerKeys.FELearnerTextSearch, ControllerKeys.NPDLearnerTextSearch, ControllerKeys.PPLearnerTextSearch, ControllerKeys.PreparedDownloads,
                            ControllerKeys.Accessibility, ControllerKeys.Authentication, ControllerKeys.Cookies, ControllerKeys.Glossary, ControllerKeys.Landing, ControllerKeys.SearchMyPupilList, ControllerKeys.Privacy, ControllerKeys.Terms, ControllerKeys.test, ControllerKeys.Empty } },
                };

            public override IEnumerator<object[]> GetEnumerator() =>
                FlattenControllersDictionary(NonActiveControllersDictionary).GetEnumerator();
        }

        internal static class ControllerKeys
        {
            public const string Empty = "";
            public const string test = "test";
            public const string News = "News";
            public const string Admin = "Admin";
            public const string Terms = "Terms";
            public const string Privacy = "Privacy";
            public const string Cookies = "Cookies";
            public const string Glossary = "Glossary";
            public const string Landing = "Landing";
            public const string Accessibility = "Accessibility";
            public const string Authentication = "Authentication";
            public const string FELearnerNumber = "FELearnerNumber";
            public const string FELearnerTextSearch = "FELearnerTextSearch";
            public const string PPLearnerTextSearch = "PPLearnerTextSearch";
            public const string ManageDocuments = "ManageDocuments";
            public const string NPDLearnerNumberSearch = "NPDLearnerNumberSearch";
            public const string NPDLearnerTextSearch = "NPDLearnerTextSearch";
            public const string PreparedDownloads = "PreparedDownloads";
            public const string SearchMyPupilList = "SearchMyPupilList";
            public const string PupilPremiumLearnerNumber = "PupilPremiumLearnerNumber";
            public const string SecurityReportByPupilStudentRecord = "SecurityReportByPupilStudentRecord";
        }
    }
}