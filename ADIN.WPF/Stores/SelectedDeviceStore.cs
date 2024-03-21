using ADIN.Device.Models;
using ADIN.WPF.Models;
using Helper.Feedback;
using System;
using System.Windows;

namespace ADIN.WPF.Stores
{
    public class SelectedDeviceStore
    {
        private ADINDeviceModel _selectedDevice;

        public event Action<FeedbackModel> ErrorOccured;

        public event Action<FrameType> FrameContentChanged;

        public event Action<string> FrameGenCheckerResetDisplay;

        public event Action<string> FrameGenCheckerStatusChanged;

        public event Action<string> LinkLengthChanged;

        public event Action<string> LinkStatusChanged;

        public event Action<LoopBackMode> LoopbackChanged;

        public event Action<LoopbackListingModel> LoopbackStateChanged;

        public event Action<string> MseValueChanged;

        public event Action<AutoNegMasterSlaveAdvertisementItem> NegotiationMasterSlaveChanged;

        public event Action<PeakVoltageAdvertisementItem> PeakVoltageChanged;

        public event Action<FeedbackModel> ProcessCompleted;

        public event Action SelectedDeviceChanged;

        public event Action<string> SoftwarePowerDownChanged;

        public event Action<TestModeType> TestmodeChanged;

        public event Action<TestModeListingModel> TestModeStateChanged;

        public event Action<FeedbackModel> ViewModelErrorOccured;

        public event Action RegistersValueChanged;

        public ADINDeviceModel SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (_selectedDevice != null)
                {
                    _selectedDevice.FirmwareAPI.WriteProcessCompleted -= FirmwareAPI_WriteProcessCompleted;
                    _selectedDevice.FirmwareAPI.FrameGenCheckerTextStatusChanged -= FirmwareAPI_FrameGenCheckerStatusCompleted;
                    _selectedDevice.FirmwareAPI.ResetFrameGenCheckerStatisticsChanged -= FirmwareAPI_ResetFrameGenCheckerStatisticsChanged;
                    _selectedDevice.FirmwareAPI.ErrorOccured -= FirmwareAPI_ErrorOccured;
                    _selectedDevice.FirmwareAPI.NegotiationMasterSlaveChanged -= FirmwareAPI_NegotiationMasterSlaveChanged;
                    _selectedDevice.FirmwareAPI.PeakVoltageChanged -= FirmwareAPI_PeakVoltageChanged;
                    _selectedDevice.FirmwareAPI.TestModeChanged -= FirmwareAPI_TestModeChanged;
                    _selectedDevice.FirmwareAPI.LoopbackChanged -= FirmwareAPI_LoopbackChanged;
                    _selectedDevice.FirmwareAPI.FrameContentChanged -= FirmwareAPI_FrameContentChanged;
                }

                if (value != null)
                {
                    _selectedDevice = value;
                    _selectedDevice.FirmwareAPI.WriteProcessCompleted += FirmwareAPI_WriteProcessCompleted;
                    _selectedDevice.FirmwareAPI.FrameGenCheckerTextStatusChanged += FirmwareAPI_FrameGenCheckerStatusCompleted;
                    _selectedDevice.FirmwareAPI.ResetFrameGenCheckerStatisticsChanged += FirmwareAPI_ResetFrameGenCheckerStatisticsChanged;
                    _selectedDevice.FirmwareAPI.ErrorOccured += FirmwareAPI_ErrorOccured;
                    _selectedDevice.FirmwareAPI.NegotiationMasterSlaveChanged += FirmwareAPI_NegotiationMasterSlaveChanged;
                    _selectedDevice.FirmwareAPI.PeakVoltageChanged += FirmwareAPI_PeakVoltageChanged;
                    _selectedDevice.FirmwareAPI.TestModeChanged += FirmwareAPI_TestModeChanged;
                    _selectedDevice.FirmwareAPI.LoopbackChanged += FirmwareAPI_LoopbackChanged;
                    _selectedDevice.FirmwareAPI.FrameContentChanged += FirmwareAPI_FrameContentChanged;
                    SelectedDeviceChanged?.Invoke();
                }

                if (value == null)
                {
                    _selectedDevice = value;
                    SelectedDeviceChanged?.Invoke();
                }
            }
        }

        public void OnRegistersValueChanged()
        {
            RegistersValueChanged?.Invoke();
        }

        public void OnLinkLengthChanged(string linkLength)
        {
            LinkLengthChanged?.Invoke(linkLength);
        }

        public void OnLinkStatusChanged(string linkStatus)
        {
            LinkStatusChanged?.Invoke(linkStatus);
        }

        public void OnLoopbackStateChanged(LoopbackListingModel loopback)
        {
            LoopbackStateChanged?.Invoke(loopback);
        }

        public void OnMseValueChanged(string mseValue)
        {
            MseValueChanged?.Invoke(mseValue);
        }

        public void OnSoftwarePowerDownChanged(string linkStatus)
        {
            SoftwarePowerDownChanged?.Invoke(linkStatus);
        }

        public void OnTestModeStateChanged(TestModeListingModel testmode)
        {
            TestModeStateChanged?.Invoke(testmode);
        }

        public void OnViewModelErrorOccured(string errorMessage, FeedbackType errorType = FeedbackType.Error)
        {
            FeedbackModel errorFeedback = new FeedbackModel() { Message = errorMessage, FeedBackType = errorType };
            ViewModelErrorOccured?.Invoke(errorFeedback);
        }

        private void FirmwareAPI_ErrorOccured(object sender, FeedbackModel errorMessage)
        {
            ErrorOccured?.Invoke(errorMessage);
        }

        private void FirmwareAPI_FrameContentChanged(object sender, FrameType e)
        {
            FrameContentChanged?.Invoke(e);
        }

        private void FirmwareAPI_FrameGenCheckerStatusCompleted(object sender, string status)
        {
            FrameGenCheckerStatusChanged?.Invoke(status);
        }

        private void FirmwareAPI_LoopbackChanged(object sender, LoopBackMode e)
        {
            LoopbackChanged?.Invoke(e);
        }

        private void FirmwareAPI_NegotiationMasterSlaveChanged(object sender, AutoNegMasterSlaveAdvertisementItem e)
        {
            NegotiationMasterSlaveChanged?.Invoke(e);
        }

        private void FirmwareAPI_PeakVoltageChanged(object sender, PeakVoltageAdvertisementItem e)
        {
            PeakVoltageChanged?.Invoke(e);
        }

        private void FirmwareAPI_ResetFrameGenCheckerStatisticsChanged(object sender, string status)
        {
            FrameGenCheckerResetDisplay?.Invoke(status);
        }

        private void FirmwareAPI_TestModeChanged(object sender, TestModeType e)
        {
            TestmodeChanged?.Invoke(e);
        }

        private void FirmwareAPI_WriteProcessCompleted(object sender, FeedbackModel feedback)
        {
            ProcessCompleted?.Invoke(feedback);
        }
    }
}