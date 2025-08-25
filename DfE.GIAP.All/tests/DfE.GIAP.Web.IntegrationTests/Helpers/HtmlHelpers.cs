using System.Net.Http.Headers;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;

namespace DfE.GIAP.Web.IntegrationTests.Helpers;
internal static class HtmlHelpers
{
    internal static async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        IDocument document = await BrowsingContext.New()
            .OpenAsync(ResponseFactory, CancellationToken.None);
        return (IHtmlDocument)document;

        void ResponseFactory(VirtualResponse htmlResponse)
        {
            htmlResponse
                .Address(response.RequestMessage.RequestUri)
                .Status(response.StatusCode);

            MapHeaders(response.Headers);
            MapHeaders(response.Content.Headers);

            htmlResponse.Content(content);

            void MapHeaders(HttpHeaders headers)
            {
                foreach (var header in headers)
                {
                    foreach (var value in header.Value)
                    {
                        htmlResponse.Header(header.Key, value);
                    }
                }
            }
        }
    }
}
