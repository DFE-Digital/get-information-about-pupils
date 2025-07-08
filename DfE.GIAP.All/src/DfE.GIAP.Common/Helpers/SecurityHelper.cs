using Ganss.Xss;

namespace DfE.GIAP.Common.Helpers
{
    public static class SecurityHelper
    {
        /// <summary>
        /// Helper to clean text/user input, wrapper around HtmlSanitizer. Will allow classes. 
        /// </summary>
        /// <param name="text">Text to clean</param>
        /// <returns>Cleaned text</returns>
        public static string SanitizeText(string text)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add("class");
            return sanitizer.Sanitize(text);
        }
    }
}
