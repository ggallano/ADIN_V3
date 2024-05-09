using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IAdvertisedSpeedAPI
    {
        void AdvertisedForcedSpeed(string advFrcSpd);
        void Speed100EEEAdvertisement(bool spd100EEEAdv_st);
        void Speed100FdAdvertisement(bool spd100FdAdv_st);
        void Speed100HdAdvertisement(bool spd100HdAdv_st);
        void Speed10FdAdvertisement(bool spd10FdAdv_st);
        void Speed10HdAdvertisement(bool spd10HdAdv_st);
        void CheckAdvertisedSpeed(List<string> listAdvSpd);
    }
}
