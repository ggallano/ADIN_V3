using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class LoopbackFrameGenCheckerModel
    {
        public LoopbackFrameGenCheckerModel()
        {
            FrameContents = new List<FrameContentModel>();
        }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public LoopBackMode EnumLoopbackType { get; set; }

        public string DestMacAddress { get; set; }
        public string DestOctet { get; set; }
        public bool EnableContinuousMode { get; set; }
        public bool EnableMacAddress { get; set; }
        public uint FrameBurst { get; set; }
        public FrameContentModel FrameContent { get; set; }
        public List<FrameContentModel> FrameContents { get; set; }
        public string FrameGeneratorButtonText { get; set; }
        public uint FrameLength { get; set; }
        public FrameType SelectedFrameContent { get; set; }
        public string SrcMacAddress { get; set; }
        public string SrcOctet { get; set; }
        public bool IsSerDesSelected { get; set; }
    }
}

