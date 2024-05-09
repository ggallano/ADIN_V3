using ADIN.Device.Models;

namespace ADIN.WPF.Models
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
