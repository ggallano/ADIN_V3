using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IDownSpeedAPI
    {
        void DownSpeed10Hd(bool dwnSpd10Hd);
        void DownSpeedRetriesSetVal(uint dwnSpdRtryVal);
    }
}
