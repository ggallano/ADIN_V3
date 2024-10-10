using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface IClockPinControl
    {
        List<string> GpClkPinControls { get; set; }
        List<string> Clk25RefPinControls { get; set; }
        string GpClkPinControl { get; set; }
        string Clk25RefPnCtrl { get; set; }
    }
}
