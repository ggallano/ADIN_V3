using ADIN.WPF.Models;
using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public class TestModeModel
    {
        public List<TestModeListingModel> TestModes { get; set; }
        public TestModeListingModel TestMode { get; set; }

        public TestModeModel()
        {
            TestModes = new List<TestModeListingModel>();
        }
    }
}
