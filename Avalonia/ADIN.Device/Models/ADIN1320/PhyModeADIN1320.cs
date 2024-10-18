using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1320
{
    public class PhyModeADIN1320 : IPhyMode
    {
        public PhyModeADIN1320()
        {
            //ActivePhyMode = "Copper Media Only";
            //ActivePhyMode = "Fiber Media Only";
            //ActivePhyMode = "Media Converter";

            PhyModes = new ObservableCollection<string>()
            {
                "Copper Media Only",
                "Fiber Media Only",
                "Backplane",
                "Auto Media Detect_Cu",
                "Auto Media Detect_Fi",
                "Media Converter"
            };
            ActivePhyMode = PhyModes[0];

            MacInterface = "RMII";
        }
        public string ActivePhyMode { get; set; }
        public string MacInterface { get; set; }

        public ObservableCollection<string> PhyModes { get; set; }
    }
}
