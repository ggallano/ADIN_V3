using ADIN.Device.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface ITimeDomainReflectometryAPI
    {
        string GetNvp();
        string GetOffset();
        string PerformCableCalibration(decimal length);
        FaultType PerformFaultDetection();
        string PerformOffsetCalibration();
        List<string> SetNvp(decimal nvpValue);
        string SetOffset(decimal offset);
    }
}
