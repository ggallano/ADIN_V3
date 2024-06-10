using ADIN.Device.Models;
using System.Collections.Generic;

namespace ADIN.Device.Services
{
    public interface IADIN1100API : IFirmwareAPI, IMDIOAPI, ITestModeAPI, IFrameGenCheckerAPI, ILoopbackAPI, IMasterSlaveSettingsAPI, ITxLevelAPI
    {
        string GetAnStatus();
        string GetMasterSlaveStatus();
        string GetTxLevelStatus();

        #region Cable Diag
        List<string> GetCoeff();

        decimal GetFaultDistance();

        string GetNvp();

        string GetOffset();

        string PerformCableCalibration(decimal length);

        FaultType PerformFaultDetection();

        string PerformOffsetCalibration();

        List<string> SetCoeff(decimal nvp, decimal coeff0, decimal coeffi);

        void SetMode(CalibrationMode mode);

        List<string> SetNvp(decimal nvpValue);

        string SetOffset(decimal offset);

        void TDRInit();
        #endregion
    }
}
