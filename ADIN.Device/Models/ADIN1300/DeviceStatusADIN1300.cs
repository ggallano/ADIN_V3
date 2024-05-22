using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1300
{
    public class DeviceStatusADIN1300 : IDeviceStatus
    {
        public bool IsSoftwarePowerDown { get; set; } = true;
    }
}
