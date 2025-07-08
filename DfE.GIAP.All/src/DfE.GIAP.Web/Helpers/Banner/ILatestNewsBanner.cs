using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Helpers.Banner
{
    public interface ILatestNewsBanner
    {
        public Task SetLatestNewsStatus();
        public Task RemoveLatestNewsStatus();
        public bool ShowNewsBanner();
    }
}
