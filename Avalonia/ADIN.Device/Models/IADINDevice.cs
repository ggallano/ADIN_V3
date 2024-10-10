using ADIN.Device.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface IADINDevice
    {
        string BoardName { get; set; }
        string SerialNumber { get; set; }
        IFirmwareAPI FirmwareAPI { get; set; }
    }
}
