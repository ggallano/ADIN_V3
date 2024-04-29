using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public interface IClockPinControl
    {
        List<string> GpClkPinControls { get; set; }
        string GpClkPinControl { get; set; }
    }
}