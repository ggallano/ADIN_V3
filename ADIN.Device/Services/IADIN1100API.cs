using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IADIN1100API : IFirmwareAPI, IMDIOAPI, ITestModeAPI, IFrameGenCheckerAPI, ILoopbackAPI, IMasterSlaveSettingsAPI, ITxLevelAPI
    {
    }
}
