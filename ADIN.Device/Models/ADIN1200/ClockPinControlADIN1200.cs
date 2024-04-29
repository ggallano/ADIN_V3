using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ADIN.Device.Models.ADIN1200
{
    public class ClockPinControlADIN1200 : IClockPinControl
    {
        public ClockPinControlADIN1200()
        {
            GpClkPinControls = new List<string>()
            {
                "None",
                "125 MHz PHY Recovered",
                "125 MHz PHY Free Running",
                "Recovered HeartBeat",
                "Free Running HeartBeat",
                "25 MHz Reference"
            };
            GpClkPinControl = GpClkPinControls[0];
        }

        public List<string> GpClkPinControls { get; set; }
        public string GpClkPinControl { get; set; }
    }
}
