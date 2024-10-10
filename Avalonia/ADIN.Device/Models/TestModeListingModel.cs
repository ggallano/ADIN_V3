using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class TestModeListingModel
    {
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Description { get; set; }
        public bool IsRequiringFrameLength { get; set; }
        public TestModeType TestmodeType { get; set; }
    }
}
