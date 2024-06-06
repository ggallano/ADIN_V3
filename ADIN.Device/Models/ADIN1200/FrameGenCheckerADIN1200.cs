using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1200
{
    public class FrameGenCheckerADIN1200 : IFrameGenChecker
    {
        public FrameGenCheckerADIN1200()
        {
            EnableMacAddress = false;

            FrameContents = new List<FrameContentModel>()
            {
                new FrameContentModel()
                {
                    Name = "Random",
                    FrameContentType = FrameType.Random
                },
                new FrameContentModel()
                {
                    Name = "All 0s",
                    FrameContentType = FrameType.All0s
                },
                new FrameContentModel()
                {
                    Name = "All 1s",
                    FrameContentType = FrameType.All1s
                },
                new FrameContentModel()
                {
                    Name = "All 10s",
                    FrameContentType = FrameType.Alt10s
                }
            };

            SrcMacAddress = null;
            DestMacAddress = null;
        }

        public string DestMacAddress { get; set; }
        public string DestOctet { get; set; }
        public bool EnableContinuousMode { get; set; }
        public bool EnableMacAddress { get; set; }
        public uint FrameBurst { get; set; }
        public FrameContentModel FrameContent { get; set; }
        public List<FrameContentModel> FrameContents { get; set; }
        public uint FrameLength { get; set; }
        public FrameType SelectedFrameContent { get; set; }
        public string SrcMacAddress { get; set; }
        public string SrcOctet { get; set; }
    }
}
