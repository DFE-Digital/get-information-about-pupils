using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace DfE.GIAP.Web.Helpers.SelectionManager
{
    public class TextSearchSelectionManager : ITextSearchSelectionManager
    {
        public const string SelectedKey = "radioSelected";
        private readonly HttpContext context;

        public TextSearchSelectionManager(IHttpContextAccessor contextAccessor)
        {
            this.context = contextAccessor.HttpContext;
        }
      
        public void Add(string selectedLearnerNumber)
        {
            var inSession = GetSelectedFromSession();

            if (!inSession.Contains(selectedLearnerNumber))
            {
                inSession= selectedLearnerNumber;
            }
            
            UpdateSession(inSession);
        }

        public string GetSelectedFromSession()
        {
            if (context.Session.Keys.Contains(SelectedKey))
            {
                return JsonConvert.DeserializeObject<string>(context.Session.GetString(SelectedKey));
            }

            return string.Empty;
        }

        public void Clear()
        {
            if (context.Session.Keys.Contains(SelectedKey))
            {
                context.Session.Remove(SelectedKey);
            }
        }

        private void UpdateSession(string selected)
        {
            context.Session.SetString(SelectedKey, JsonConvert.SerializeObject(selected));
        }   
     
    }
}
