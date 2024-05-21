using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1200
{
    public class DeviceStatusADIN1200 : IDeviceStatus
    {
        public bool IsSoftwarePowerDown { get; set; } = true;
    }
}
