using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IClockPinControlAPI
    {
        void SetGpClkPinControl(string gpClkPinCtrl);
        void SetClk25RefPinControl(string clk25RefPinCtrl);
    }
}
