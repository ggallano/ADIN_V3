using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1300
{
    public class ClockPinControlADIN1300 : IClockPinControl
    {
        public ClockPinControlADIN1300()
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

            Clk25RefPinControls = new List<string>()
            {
                "None",
                "25 MHz Reference"
            };
            Clk25RefPnCtrl = Clk25RefPinControls[0];
        }

        public List<string> Clk25RefPinControls { get; set; }
        public string Clk25RefPnCtrl { get; set; }
        public List<string> GpClkPinControls { get; set; }
        public string GpClkPinControl { get; set; }
    }
}
