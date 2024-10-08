using ADIN.Device.Models.ADIN1300;
using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public class ClockPinControlModel : IClockPinControl
    {
        public List<string> GpClkPinControls { get; set; }
        public string GpClkPinControl { get; set; }
    }
}