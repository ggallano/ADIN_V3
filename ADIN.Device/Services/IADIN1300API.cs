using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IADIN1300API : IMDIOAPI, IAdvertisedSpeedAPI, IClockPinControlAPI, IDownSpeedAPI, IAutoMDIXAPI, IEnergyDetectAPI, ITestModeAPI, ILoopbackAPI, IFrameGenCheckerAPI
    {
        void Speed1000EEEAdvertisement(bool spd1000EEEAdv_st);
        void Speed1000FdAdvertisement(bool spd1000FdAdv_st);
        void Speed1000HdAdvertisement(bool spd1000HdAdv_st);
        void DownSpeed100Hd(bool dwnSpd100Hd);
    }
}
