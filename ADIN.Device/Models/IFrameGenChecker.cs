using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface IFrameGenChecker
    {
        string DestMacAddress { get; set; }
        string DestOctet { get; set; }
        bool EnableContinuousMode { get; set; }
        bool EnableMacAddress { get; set; }
        uint FrameBurst { get; set; }
        FrameContentModel FrameContent { get; set; }
        List<FrameContentModel> FrameContents { get; set; }
        uint FrameLength { get; set; }
        FrameType SelectedFrameContent { get; set; }
        string SrcMacAddress { get; set; }
        string SrcOctet { get; set; }
    }
}
