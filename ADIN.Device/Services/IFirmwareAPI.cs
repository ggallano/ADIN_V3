using ADI.Register.Models;
using ADIN.Device.Models;
using ADIN.WPF.Models;
using Helper.Feedback;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ADIN.Device.Services
{
    public interface IFirmwareAPI
    {
        event EventHandler<FeedbackModel> ErrorOccured;

        event EventHandler<FrameType> FrameContentChanged;

        event EventHandler<string> FrameGenCheckerTextStatusChanged;

        event EventHandler<string> LinkLengthChanged;

        event EventHandler<LoopBackMode> LoopbackChanged;

        event EventHandler<string> MseValueChanged;

        event EventHandler<AutoNegMasterSlaveAdvertisementItem> NegotiationMasterSlaveChanged;

        event EventHandler<PeakVoltageAdvertisementItem> PeakVoltageChanged;

        event EventHandler<FeedbackModel> ReadProcessCompleted;

        event EventHandler<string> ResetFrameGenCheckerStatisticsChanged;

        event EventHandler<TestModeType> TestModeChanged;

        event EventHandler<FeedbackModel> WriteProcessCompleted;

        /// <summary>
        /// gets or sets the frame gen checker on going
        /// </summary>
        bool isFrameGenCheckerOngoing { get; set; }

        /// <summary>
        /// executes the selected script
        /// </summary>
        /// <param name="script">selected script</param>
        void ExecuteSript(ScriptModel script);

        /// <summary>
        /// gets the An Status
        /// </summary>
        /// <returns></returns>
        string GetAnStatus();

        /// <summary>
        /// gets the frame checker status
        /// </summary>
        void GetFrameCheckerStatus();

        /// <summary>
        /// gets the frame content
        /// </summary>
        /// <param name="eventTrigger">event trigger by an event</param>
        /// <returns>returns the frame selected frame contents</returns>
        FrameType GetFrameContentInitialization(bool eventTrigger = false);

        /// <summary>
        /// gets the generator status
        /// </summary>
        /// <returns></returns>
        string GetFrameGeneratorStatus();

        /// <summary>
        /// gets the link length
        /// </summary>
        /// <returns></returns>
        string GetLinkLength();

        /// <summary>
        /// gets the the link status
        /// </summary>
        /// <returns></returns>
        string GetLinkStatus();

        /// <summary>
        /// checks the current loopback state
        /// </summary>
        /// <param name="eventTrigger"></param>
        /// <returns>return the loopback state</returns>
        LoopBackMode GetLoopbackInitialization(bool eventTrigger = false);

        /// <summary>
        /// gets the loopback state
        /// </summary>
        /// <returns>return the current loopback state</returns>
        LoopBackMode GetLoopbackState();

        /// <summary>
        /// gets the master/slave status
        /// </summary>
        /// <returns></returns>
        string GetMasterSlaveStatus();

        /// <summary>
        /// gets the model number of the board
        /// </summary>
        /// <param name="phyAddress"></param>
        /// <returns></returns>
        BoardType GetModelNum(out uint phyAddress);

        /// <summary>
        /// Gets Master Slave Settings
        /// </summary>
        /// <returns></returns>
        AutoNegMasterSlaveAdvertisementItem GetNegotiationMasterSlaveInitialization(bool eventTrigger = false);

        /// <summary>
        /// gets the Peak Voltage settings
        /// </summary>
        /// <returns></returns>
        PeakVoltageAdvertisementItem GetPeakVoltageInitialization(bool eventTrigger = false);

        /// <summary>
        /// gets the phy state status
        /// </summary>
        /// <returns>returns the phy state status</returns>
        EthPhyState GetPhyState();

        /// <summary>
        /// gets the register json file based on revision number of the board
        /// </summary>
        /// <param name="phyAddress">phyAddress</param>
        /// <returns>returns the regsiter json file number</returns>
        string GetRegisterJsonFile(BoardRevision revNum);

        /// <summary>
        /// gets the revision number of the board
        /// </summary>
        /// <returns></returns>
        BoardRevision GetRevNum();

        /// <summary>
        /// gets the status of the registers from the board
        /// </summary>
        /// <param name="registers">list of registers</param>
        /// <returns>returns the udpate list of registers</returns>
        ObservableCollection<RegisterModel> GetStatusRegisters(/*uint phyAddress, List<RegisterModel> regsiters*/);

        /// <summary>
        /// checks the current testmode state
        /// </summary>
        /// <param name="eventTrigger"></param>
        /// <returns>return sthe testmode state</returns>
        TestModeType GetTestModeInitialization(bool eventTrigger = false);

        /// <summary>
        /// gets the testmode state
        /// </summary>
        /// <returns>returns the current testmode</returns>
        TestModeType GetTestModeState();

        /// <summary>
        /// gets the tx level status
        /// </summary>
        /// <returns></returns>
        string GetTxLevelStatus();

        /// <summary>
        /// Phy (hardware) reset
        /// Format: phyreset\n
        /// </summary>
        void HardwareReset();

        void MdioReadCl22(uint phyAddress, uint regAddress);

        /// <summary>
        /// MDIO (Cluase45) Read
        /// Format: mdiord_cl45 <phyAddress>,<regAddress>\n
        /// </summary>
        /// <param name="phyAddress">assigned phy adress</param>
        /// <param name="regAddress">register address</param>
        /// <returns>register value</returns>
        string MdioReadCl45(uint regAddress);

        void MdioWriteCl22(uint phyAddress, uint regAddress, uint data);

        /// <summary>
        /// MDIO (Cluase45) Write
        /// Format: mdiowr_cl45 <phyAddress>,<regAddress>,<data>\n
        /// </summary>
        /// <param name="phyAddress">assigned phy address</param>
        /// <param name="regAddress">register address</param>
        /// <param name="data">data</param>
        string MdioWriteCl45(uint regAddress, uint data);

        /// <summary>
        /// cable calibration
        /// </summary>
        /// <param name="length">cable length</param>
        /// <returns></returns>
        string PerformCableCalibration(decimal length);

        /// <summary>
        /// performs the fault detector
        /// </summary>
        FaultType PerformFaultDetection();

        /// <summary>
        /// offset calibration
        /// </summary>
        /// <returns></returns>
        string PerformOffsetCalibration();

        /// <summary>
        /// reads the registers of the board
        /// </summary>
        void ReadRegsiters();

        /// <summary>
        /// resets the FC Statistics
        /// </summary>
        void ResetFrameGenCheckerStatistics();

        /// <summary>
        /// resets the phy status
        /// </summary>
        void ResetPhy(ResetType reset);

        /// <summary>
        /// restart auto negotation
        /// </summary>
        void RestartAutoNegotiation();

        /// <summary>
        /// sets the Frame Gen Checker
        /// </summary>
        void SetFrameCheckerSetting(FrameGenCheckerModel frameContent);

        /// <summary>
        /// sets the loopback
        /// </summary>
        /// <param name="loopback">selected loopback</param>
        void SetLoopbackSetting(LoopbackListingModel loopback);

        /// <summary>
        /// sets the mode of the cable diagnostics
        /// </summary>
        void SetMode(CalibrationMode mode);

        /// <summary>
        /// sets the AutoNegMasterSlaveAdvertisement
        /// </summary>
        /// <param name="negotiateMasterSlave">AutoNegMasterSlaveAdvertisementItem</param>
        void SetNegotiateMasterSlaveSetting(AutoNegMasterSlaveAdvertisementItem negotiateMasterSlave);

        /// <summary>
        /// sets the AutoNegTxLevelAdvertisement
        /// </summary>
        /// <param name="pkpkVoltage">AutoNegTxLevelAdvertisementItem</param>
        void SetPeakToPeakVoltageSetting(PeakVoltageAdvertisementItem pkpkVoltage);

        /// <summary>
        /// sets the loopback rx suppression
        /// </summary>
        /// <param name="isRxSuppression">rx suppression value</param>
        void SetRxSuppressionSetting(bool isRxSuppression);

        /// <summary>
        /// sets the test modes
        /// </summary>
        /// <param name="testModeModel">test mode item</param>
        void SetTestModeSetting(TestModeListingModel testModeModel);

        /// <summary>
        /// sets the loopback tx suppression
        /// </summary>
        /// <param name="isTxSuppression">tx suppression value</param>
        void SetTxSuppressionSetting(bool isTxSuppression);

        /// <summary>
        /// sets Software power down
        /// </summary>
        /// <param name="isSoftwarePowerdown"></param>
        void SoftwarePowerdown(bool isSoftwarePowerdown);

        /// <summary>
        /// uC software Reset.
        /// Format: reset\n
        /// </summary>
        void SoftwareReset();
    }
}