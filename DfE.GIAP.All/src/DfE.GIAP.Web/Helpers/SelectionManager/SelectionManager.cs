using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Helpers.SelectionManager
{
    /// <summary>
    /// note that because the default for the UPN/PP/ULN pages is for everything to be selected,
    /// to keep the content of session small, this actually tracks the things that are NOT selected..
    /// </summary>
    public class NotSelectedManager : ISelectionManager
    {
        public const string NotSelectedKey = "notSelected";
        private readonly HttpContext context;

        public NotSelectedManager(IHttpContextAccessor contextAccessor)
        {
            this.context = contextAccessor.HttpContext;
        }
      
        public void AddAll(IEnumerable<string> upns)
        {
            // if it exists, remove it.
            var inSession = GetFromSession();
            foreach (var upn in upns)
            {
                if (inSession.Contains(upn))
                {
                    inSession.Remove(upn);
                }
            }
            UpdateSession(inSession);
        }

        public void RemoveAll(IEnumerable<string> upns)
        {
            var inSession = GetFromSession();
            foreach (var upn in upns)
            {
                if (!inSession.Contains(upn))
                {
                    inSession.Add(upn);
                }
            }
            UpdateSession(inSession);
        }

        public void Clear()
        {
            if (context.Session.Keys.Contains(NotSelectedKey))
            {
                context.Session.Remove(NotSelectedKey);
            }
        }

        public HashSet<string> GetSelected(string[] available)
        {
            // compare available and remove everything that exists in session..
            var inSession = GetFromSession();
            return available.Except(inSession).ToHashSet<string>();
        }

        private void UpdateSession(HashSet<string> notSelected)
        {
            context.Session.SetString(NotSelectedKey, JsonConvert.SerializeObject(notSelected));
        }   
        
        private HashSet<string> GetFromSession()
        {
            if (context.Session.Keys.Contains(NotSelectedKey))
            {
                return JsonConvert.DeserializeObject<HashSet<string>>(context.Session.GetString(NotSelectedKey));
            }

            return new HashSet<string>();
        }
    }
}
