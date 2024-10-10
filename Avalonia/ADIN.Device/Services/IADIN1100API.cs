using ADIN.Device.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IADIN1100API : IFirmwareAPI, IMDIOAPI, ITestModeAPI, IFrameGenCheckerAPI, ILoopbackAPI, IMasterSlaveSettingsAPI, ITxLevelAPI
    {
        string GetAnStatus();

        string GetMasterSlaveStatus();

        string GetTxLevelStatus();

        double GetMaxSlicer();

        uint GetSpikeCount();

        void ResetMaxSlicer();

        void ResetSpikeCount();

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
