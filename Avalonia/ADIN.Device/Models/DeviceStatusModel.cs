using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class DeviceStatusModel : IDeviceStatus
    {
        public bool IsSoftwarePowerDown { get; set; }
        public List<string> AdvertisedSpeeds { get; set; }
    }
}
