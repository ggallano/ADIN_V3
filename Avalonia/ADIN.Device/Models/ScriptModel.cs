using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class ScriptModel
    {
        public string Name { get; set; }
        public List<RegisterAccessModel> RegisterAccesses { get; set; }
    }
}
