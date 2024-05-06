using ADIN.WPF.Models;
using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public interface ITestMode
    {
        List<TestModeListingModel> TestModes { get; set; }
        TestModeListingModel TestMode { get; set; }
    }
}