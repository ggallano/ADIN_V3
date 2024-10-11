using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IADIN1300API : IFirmwareAPI, IMDIOAPI, IAdvertisedSpeedAPI, IAdvertisedGbSpeedAPI, IClockPinControlAPI, IDownSpeedAPI, IAutoMDIXAPI, IEnergyDetectAPI, ITestModeAPI, ILoopbackAPI, IMasterSlaveSettingsAPI, IFrameGenCheckerAPI, ICableDiagAPI
    {
    }
}
