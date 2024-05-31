using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface IDeviceStatus
    {
        bool IsSoftwarePowerDown { get; set; }
        List<string> AdvertisedSpeeds { get; set; }
    }
}
