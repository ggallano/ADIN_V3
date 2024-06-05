using ADIN.Device.Models;

namespace ADIN.Device.Services
{
    public interface IADIN1100API : IFirmwareAPI, IMDIOAPI, ITestModeAPI, IFrameGenCheckerAPI, ILoopbackAPI, IMasterSlaveSettingsAPI, ITxLevelAPI
    {
        string GetAnStatus();
        string GetMasterSlaveStatus();
        string GetTxLevelStatus();
        BoardRevision GetRevNum();
    }
}
