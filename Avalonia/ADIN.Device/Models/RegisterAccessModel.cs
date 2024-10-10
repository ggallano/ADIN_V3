using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class RegisterAccessModel
    {
        public string MemoryMap { get; set; }
        public string RegisterName { get; set; }
        public string RegisterAddress { get; set; }
        public string Value { get; set; }
    }
}
