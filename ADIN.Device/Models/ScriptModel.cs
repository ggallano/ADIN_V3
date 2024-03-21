using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public class ScriptModel
    {
        public string Name { get; set; }
        public List<RegisterAccessModel> RegisterAccesses { get; set; }
    }
}
