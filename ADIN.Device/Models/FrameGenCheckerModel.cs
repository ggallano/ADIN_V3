using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public class FrameGenCheckerModel
    {
        public FrameGenCheckerModel()
        {
            FrameContents = new List<FrameContentModel>();
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