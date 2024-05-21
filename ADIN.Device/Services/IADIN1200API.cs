using ADIN.Device.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IADIN1200API : IMDIOAPI, IAdvertisedSpeedAPI, IClockPinControlAPI, IDownSpeedAPI, IAutoMDIXAPI, IEnergyDetectAPI, ITestModeAPI, ILoopbackAPI, IFrameGenCheckerAPI
    {
    }
}
