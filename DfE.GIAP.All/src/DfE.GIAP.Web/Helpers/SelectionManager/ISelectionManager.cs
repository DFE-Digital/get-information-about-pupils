using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Helpers.SelectionManager
{
    public interface ISelectionManager
    {
        public void AddAll(IEnumerable<string> upns);
        public void RemoveAll(IEnumerable<string> upns);
        public void Clear();
        public HashSet<string> GetSelected(string[] available);
    }
}
