using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using DfE.GIAP.Common.Constants;

namespace DfE.GIAP.Web.Helpers.Navigation
{
    [HtmlTargetElement(Attributes = "is-active-route")]
    public class NavigationLinkHelper : AnchorTagHelper
    {
        public NavigationLinkHelper(IHtmlGenerator generator)
            : base(generator)
        {
        }

        /// <summary>
        /// Method to check if current controller matches <a> link controller attribute (or sub controller) and adds active class for styling and accessability
        /// </summary>
        /// <param name="context"> Tag helper context</param>
        /// <param name="output"> Tag helper output</param>
        /// <returns>Tag helper output for anchor tag with updated active class if relevant</returns>

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var routeData = ViewContext.RouteData.Values;
            var currentController = routeData["controller"] as string;

            var active = string.Equals(Controller, currentController, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Controller, GetCurrentSection(currentController), StringComparison.OrdinalIgnoreCase);

            if (active)
            {
                var existingClasses = output.Attributes["class"]?.Value.ToString();
                if (output.Attributes["class"] != null)
                {
                    output.Attributes.Remove(output.Attributes["class"]);
                }

                output.Attributes.Add("class", $"{existingClasses} webapp-header-link--active");
                output.Attributes.Add("aria-current", "page");
            }
        }

        /// <summary>
        /// Method to check if current controller sits under navigation sections
        /// </summary>
        /// <param name="currentController">Name of current controller</param>
        /// <returns>Relevant controller name for navigation link section, or empty string if outside the primary navigation</returns>

        private string GetCurrentSection(string currentController)
        {
            switch (currentController)
            {
                case Global.NPDTextSearchController:
                case Global.NPDLearnerNumberSearchController:
                    return Global.NPDLearnerNumberSearchController;

                case Global.FELearnerTextSearchController:
                case Global.FELearnerNumberSearchController:
                    return Global.FELearnerNumberSearchController;

                case Global.PPNonUpnController:
                case Global.PPLearnerNumberSearchController:
                    return Global.PPLearnerNumberSearchController;

                case "Admin":
                case "ManageDocuments":
                case "SecurityReportByPupilStudentRecord":
                    return "Admin";

                default:
                    return "";
            };
        }
    }
}