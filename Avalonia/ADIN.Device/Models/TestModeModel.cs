using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
