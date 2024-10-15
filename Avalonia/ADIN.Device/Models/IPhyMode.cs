using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface IPhyMode
    {
        public string ActivePhyMode { get; set; }
        public string MacInterface { get; set; }
    }
}
