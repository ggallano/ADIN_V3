// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;

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
