using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface ITestMode
    {
        List<TestModeListingModel> TestModes { get; set; }
        TestModeListingModel TestMode { get; set; }
        uint TestModeFrameLength { get; set; }
    }
}
