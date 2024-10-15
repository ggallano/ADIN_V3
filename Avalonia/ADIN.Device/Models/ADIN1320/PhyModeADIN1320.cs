using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1320
{
    public class PhyModeADIN1320 : IPhyMode
    {
        public PhyModeADIN1320()
        {
            ActivePhyMode = "Copper Media Only";
            MacInterface = "RMII";
        }
        public string ActivePhyMode { get; set; }
        public string MacInterface { get; set; }
    }
}
