using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN1100_Eval
{
    public class FrameGeneratorChecker
    {
        public int FramesBurst { get; set; }
        public int FrameLength { get; set; }
        public int FrameContent { get; set; }
        public string SourceMacAddress { get; set; }
        public string DestinationMacAddress { get; set; }
        public bool IsFrameRunning { get; set; }
        public bool IsContinuousMode { get; set; }
        public bool MacAddressEnable { get; set; }
    }
}
